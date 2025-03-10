using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Threading;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Checking installed Chrome version...");
        string currentVersion = GetInstalledChromeVersion();

        if (!string.IsNullOrEmpty(currentVersion))
        {
            Console.WriteLine($"Current Chrome version: {currentVersion}");

            string latestVersion = await GetLatestChromeVersion();
            Console.WriteLine($"Latest Chrome version available: {latestVersion}");

            if (CompareVersions(latestVersion, currentVersion) <= 0)
            {
                Console.WriteLine($"Your Chrome version ({currentVersion}) is already up-to-date or newer than the available version ({latestVersion}).");
                return;
            }

            Console.WriteLine("Would you like to update Chrome to the latest version? (yes/no)");
            string response = Console.ReadLine().Trim().ToLower();

            if (response != "yes")
            {
                Console.WriteLine("Update canceled.");
                return;
            }
        }
        else
        {
            Console.WriteLine("Chrome is not installed on your system.");

            string latestVersion = await GetLatestChromeVersion();
            Console.WriteLine($"Latest Chrome version available: {latestVersion}");

            Console.WriteLine("Would you like to install Chrome? (yes/no)");
            string response = Console.ReadLine().Trim().ToLower();

            if (response != "yes")
            {
                Console.WriteLine("Installation canceled.");
                return;
            }
        }

        string downloadUrl = "https://dl.google.com/chrome/install/latest/chrome_installer.exe";
        string localFilePath = Path.Combine(Path.GetTempPath(), "chrome_installer.exe");

        bool downloadSuccess = await DownloadFileAsync(downloadUrl, localFilePath);
        if (downloadSuccess)
        {
            Console.WriteLine("Download complete. Installing Chrome...");
            bool installSuccess = InstallChrome(localFilePath);

            if (installSuccess)
            {
                Console.WriteLine("Waiting for Chrome to register in the system...");
                Thread.Sleep(10000);  

                string newVersion = RetryGetChromeVersion(3);
                bool versionChanged = !string.IsNullOrEmpty(newVersion) && newVersion != currentVersion;

                LogPatchStatus(versionChanged);

                if (!string.IsNullOrEmpty(newVersion))
                {
                    if (versionChanged)
                    {
                        Console.WriteLine($"Chrome successfully updated from {currentVersion} to {newVersion}");
                    }
                    else
                    {
                        Console.WriteLine($"Chrome installation completed, but version remains: {newVersion}");
                        Console.WriteLine("The update may have failed or was unnecessary.");
                    }
                }
                else
                {
                    Console.WriteLine("Chrome installation completed, but version check failed.");
                    LogPatchStatus(false);
                }
            }
            else
            {
                Console.WriteLine("Failed to install Chrome.");
                LogPatchStatus(false);
            }
        }
        else
        {
            Console.WriteLine("Failed to download Chrome installer.");
            LogPatchStatus(false);
        }
    }

    static async Task<string> GetLatestChromeVersion()
    {
        try
        {
            using HttpClient client = new HttpClient();
            string versionUrl = "https://chromedriver.storage.googleapis.com/LATEST_RELEASE";
            return await client.GetStringAsync(versionUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching latest Chrome version: {ex.Message}");
            return "Unknown";
        }
    }

    static async Task<bool> DownloadFileAsync(string url, string outputPath)
    {
        using HttpClient client = new HttpClient();
        try
        {
            byte[] data = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(outputPath, data);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Download error: {ex.Message}");
            return false;
        }
    }

    static bool InstallChrome(string filePath)
    {
        try
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = "/silent /install",
                    UseShellExecute = true,
                    Verb = "runas" 
                }
            };
            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Installation error: {ex.Message}");
            return false;
        }
    }

    static string GetInstalledChromeVersion()
    {
        try
        {
            string version = ReadRegistryKey(@"SOFTWARE\Google\Chrome\BLBeacon", "version");
            if (string.IsNullOrEmpty(version))
            {
                version = ReadRegistryKey(@"SOFTWARE\WOW6432Node\Google\Chrome\BLBeacon", "version");
            }

            if (string.IsNullOrEmpty(version) && File.Exists(@"C:\Program Files\Google\Chrome\Application\chrome.exe"))
            {
                version = FileVersionInfo.GetVersionInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe").FileVersion;
            }
            if (string.IsNullOrEmpty(version) && File.Exists(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"))
            {
                version = FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe").FileVersion;
            }

            return version;
        }
        catch
        {
            return null;
        }
    }

    static string RetryGetChromeVersion(int retries)
    {
        string version = null;
        for (int i = 0; i < retries; i++)
        {
            version = GetInstalledChromeVersion();
            if (!string.IsNullOrEmpty(version)) return version;

            Console.WriteLine("Retrying version check...");
            Thread.Sleep(3000);
        }
        return version;
    }

    static string ReadRegistryKey(string path, string keyName)
    {
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
        return key?.GetValue(keyName) as string;
    }

    static void LogPatchStatus(bool success)
    {
        string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patch_log.txt");
        string status = success ? "Patch applied successfully." : "Patch failed to apply.";
        File.AppendAllText(logFilePath, $"{DateTime.Now}: {status}\n");

        Console.WriteLine($"Patch status logged: {status}");
    }

    static int CompareVersions(string v1, string v2)
    {
        if (v1 == "Unknown") return -1;
        if (v2 == "Unknown") return 1;

        try
        {
            var v1Parts = v1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = v2.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(v1Parts.Length, v2Parts.Length); i++)
            {
                if (v1Parts[i] != v2Parts[i])
                    return v1Parts[i].CompareTo(v2Parts[i]);
            }

            return v1Parts.Length.CompareTo(v2Parts.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error comparing versions: {ex.Message}");
            return 0;
        }
    }
}