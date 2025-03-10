# Chrome Version Checker & Updater

## Overview
The **Chrome Version Checker & Updater** is a C# console application that detects the installed Google Chrome version on a Windows system, checks for updates, and provides an option to install the latest version.

## Features
- Detects the installed Chrome version.
- Fetches the latest Chrome version from Google's official API.
- Compares installed and latest versions.
- Prompts the user to update if a new version is available.
- Downloads and installs Chrome silently.
- Logs update status to a file.

## Technologies Used
- C# (.NET Framework/Core)
- Windows Registry (Microsoft.Win32)
- HTTP Requests (System.Net.Http)
- Process Handling (System.Diagnostics)
- File Handling (System.IO)
- Asynchronous Programming (async & Task)

## How It Works
1. **Detects Installed Chrome Version** - Retrieves version info from the Windows Registry.
![Detect Installed Version](https://github.com/koushikballa006/ConsoleApplicationAssignment/blob/main/screenshots/Screenshot%202025-03-10%20at%2011.16.40%E2%80%AFAM.png?raw=true)
2. **Fetches Latest Chrome Version** - Queries Google's API for the latest version.
3. **Version Comparison** - Determines if an update is needed.
4. **User Prompt** - Offers the option to install/update Chrome.
5. **Download & Install** - Fetches and executes the latest Chrome installer silently.
6. **Logging** - Records update status in a log file.

## Code Structure
- `GetInstalledChromeVersion()` - Retrieves the installed version.
- `GetLatestChromeVersion()` - Fetches the latest version from Google.
- `CompareVersions(v1, v2)` - Compares two versions.
- `DownloadFileAsync(url, outputPath)` - Downloads the installer.
- `InstallChrome(filePath)` - Executes the Chrome installer.
- `LogPatchStatus(success)` - Logs update results.
- `RetryGetChromeVersion(retries)` - Checks version post-installation.

## Error Handling
- Manages network failures, registry access issues, and download errors.
- Logs errors while maintaining program stability.

## Future Enhancements
- GUI version for better user interaction.
- Support for updating other browsers.
- Enhanced logging with historical records.

## Conclusion
This utility simplifies Chrome updates by automating version checks and installations, ensuring an up-to-date browser on Windows systems.
