using BusyTag.Lib;
using BusyTag.Lib.Util;
using BusyTag.Lib.Util.DevEventArgs;

namespace BusyTag.CLI;

class Program
{
    private static BusyTagManager? _manager;
    private static BusyTagDevice? _currentDevice;
    private static bool _isRunning = true;
    private static List<string>? _lastFoundDevices = null;
    private static DateTime _lastDeviceMessage = DateTime.MinValue;

    static async Task Main(string[] args)
    {
        Console.WriteLine("BusyTag Device Manager CLI");
        Console.WriteLine("==========================");
        
        // Handle command line arguments
        if (args.Length > 0)
        {
            await HandleCommandLineArgs(args);
            return;
        }

        // Interactive mode
        await RunInteractiveMode();
    }

    static async Task HandleCommandLineArgs(string[] args)
{
    string command = args[0].ToLower();
    
    switch (command)
    {
        case "list":
        case "scan":
            await ScanForDevices();
            break;
            
        case "connect":
            if (args.Length > 1)
            {
                await ConnectToDevice(args[1]);
            }
            else
            {
                Console.WriteLine("Usage: busytag-cli connect <port_name>");
            }
            break;
            
        case "info":
            if (args.Length > 1)
            {
                await ShowDeviceInfo(args[1]);
            }
            else
            {
                Console.WriteLine("Usage: busytag-cli info <port_name>");
            }
            break;
            
        case "color":
        case "setcolor":
            await HandleColorCommand(args);
            break;
            
        case "brightness":
        case "bright":
            await HandleBrightnessCommand(args);
            break;
            
        case "upload":
            await HandleUploadCommand(args);
            break;
            
        case "download":
            await HandleDownloadCommand(args);
            break;
            
        case "delete":
        case "remove":
            await HandleDeleteCommand(args);
            break;
            
        case "files":
        case "ls":
            await HandleFilesCommand(args);
            break;
            
        case "show":
        case "display":
            await HandleShowImageCommand(args);
            break;
            
        case "pattern":
            await HandlePatternCommand(args);
            break;
            
        case "storage":
        case "space":
            await HandleStorageCommand(args);
            break;
            
        case "firmware":
            await HandleFirmwareCommand(args);
            break;
            
        case "format":
            await HandleFormatCommand(args);
            break;
            
        case "restart":
        case "reboot":
            await HandleRestartCommand(args);
            break;
            
        case "version":
        case "--version":
        case "-v":
            ShowVersion();
            break;
            
        case "help":
        case "--help":
        case "-h":
            ShowHelp();
            break;
            
        default:
            Console.WriteLine($"Unknown command: {command}");
            ShowHelp();
            break;
    }
}

static async Task HandleColorCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli color <port> <color> [brightness] [led_bits]");
        Console.WriteLine("Examples:");
        Console.WriteLine("  BusyTag color COM3 red");
        Console.WriteLine("  BusyTag color COM3 FF0000 50");
        Console.WriteLine("  BusyTag color COM3 255,0,0 75 127");
        return;
    }

    string port = args[1];
    string color = args[2];
    int brightness = args.Length > 3 && int.TryParse(args[3], out int b) ? Math.Clamp(b, 0, 100) : 100;
    int ledBits = args.Length > 4 && int.TryParse(args[4], out int l) ? Math.Clamp(l, 1, 127) : 127;

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        bool success = false;

        // Check if it's the RGB format
        if (color.Contains(','))
        {
            var rgbParts = color.Split(',');
            if (rgbParts.Length == 3 && 
                int.TryParse(rgbParts[0].Trim(), out int r) &&
                int.TryParse(rgbParts[1].Trim(), out int g) &&
                int.TryParse(rgbParts[2].Trim(), out int bb))
            {
                r = (int)(r * brightness / 100.0);
                g = (int)(g * brightness / 100.0);
                bb = (int)(bb * brightness / 100.0);
                success = await device.SendRgbColorAsync(r, g, bb, ledBits);
            }
        }
        // Check if it's hex format
        else if (color.Replace("#", "").Length == 6 && IsValidHex(color.Replace("#", "")))
        {
            var hexColor = color.Replace("#", "").ToUpper();
            var r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            var bb = Convert.ToInt32(hexColor.Substring(4, 2), 16);
            
            r = (int)(r * brightness / 100.0);
            g = (int)(g * brightness / 100.0);
            bb = (int)(bb * brightness / 100.0);
            
            success = await device.SendRgbColorAsync(r, g, bb, ledBits);
        }
        // Otherwise treat as a color name
        else
        {
            success = await device.SetSolidColorAsync(color.ToLower(), brightness, ledBits);
        }

        Console.WriteLine(success ? "Color set successfully!" : "Failed to set color.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleBrightnessCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli brightness <port> <level>");
        Console.WriteLine("  level: 0-100");
        Console.WriteLine("Example: busytag-cli brightness COM3 75");
        return;
    }

    string port = args[1];
    if (!int.TryParse(args[2], out int brightness) || brightness < 0 || brightness > 100)
    {
        Console.WriteLine("Brightness must be between 0 and 100");
        return;
    }

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        var success = await device.SetDisplayBrightnessAsync(brightness);
        Console.WriteLine(success ? $"Brightness set to {brightness}%" : "Failed to set brightness");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleUploadCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli upload <port> <file_path>");
        Console.WriteLine("Example: busytag-cli upload COM3 \"C:\\images\\photo.png\"");
        return;
    }

    string port = args[1];
    string filePath = string.Join(" ", args.Skip(2)).Trim('"', '\'');

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File not found: {filePath}");
        return;
    }

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"Uploading {fileInfo.Name} ({GetHumanReadableSize(fileInfo.Length)})...");

        await device.SendNewFile(filePath);
        Console.WriteLine("Upload completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Upload failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleDownloadCommand(string[] args)
{
    if (args.Length < 4)
    {
        Console.WriteLine("Usage: busytag-cli download <port> <filename> <destination_path>");
        Console.WriteLine("Example: busytag-cli download COM3 photo.png \"C:\\downloads\\\"");
        return;
    }

    string port = args[1];
    string filename = args[2];
    string destPath = string.Join(" ", args.Skip(3)).Trim('"', '\'');

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        Console.WriteLine($"Downloading {filename}...");
        var result = await device.GetFileAsync(filename);
        
        if (!string.IsNullOrEmpty(result))
        {
            var fullPath = Path.Combine(destPath, filename);
            File.Copy(result, fullPath, true);
            Console.WriteLine($"Downloaded to: {fullPath}");
        }
        else
        {
            Console.WriteLine("Download failed!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Download failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleDeleteCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli delete <port> <filename>");
        Console.WriteLine("Example: busytag-cli delete COM3 photo.png");
        return;
    }

    string port = args[1];
    string filename = args[2];

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        Console.WriteLine($"Deleting {filename}...");
        var success = await device.DeleteFile(filename);
        Console.WriteLine(success ? "File deleted successfully!" : "Failed to delete file");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Delete failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleFilesCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: busytag-cli files <port>");
        Console.WriteLine("Example: busytag-cli files COM3");
        return;
    }

    string port = args[1];
    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        var files = await device.GetFileListAsync();
        if (files.Count == 0)
        {
            Console.WriteLine("No files found on device.");
        }
        else
        {
            Console.WriteLine($"Files on device ({files.Count}):");
            foreach (var file in files.OrderByDescending(f => f.Size))
            {
                var indicator = file.Name == device.CurrentImageName ? " (current)" : "";
                var fileType = IsImageFile(file.Name) ? "[IMG]" : "[FILE]";
                Console.WriteLine($"  {fileType} {file.Name} - {GetHumanReadableSize(file.Size)}{indicator}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleShowImageCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli show <port> <filename>");
        Console.WriteLine("Example: busytag-cli show COM3 photo.png");
        return;
    }

    string port = args[1];
    string filename = args[2];

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        var success = await device.ShowPictureAsync(filename);
        Console.WriteLine(success ? $"Now displaying: {filename}" : "Failed to display image");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandlePatternCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli pattern <port> <pattern_name>");
        Console.WriteLine("Available patterns:");
        var patterns = PatternListCommands.PatternList;
        foreach (var pattern in patterns.Values)
        {
            Console.WriteLine($"  {pattern?.Name}");
        }
        return;
    }

    string port = args[1];
    string patternName = string.Join(" ", args.Skip(2));

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        var patterns = PatternListCommands.PatternList;
        var pattern = patterns.Values.FirstOrDefault(p => 
            p?.Name?.Equals(patternName, StringComparison.OrdinalIgnoreCase) == true);

        if (pattern != null)
        {
            var success = await device.SetNewCustomPattern(pattern.PatternLines, true, false);
            Console.WriteLine(success ? $"Pattern '{pattern.Name}' set successfully!" : "Failed to set pattern");
        }
        else
        {
            Console.WriteLine($"Pattern '{patternName}' not found");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleStorageCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: busytag-cli storage <port>");
        Console.WriteLine("Example: busytag-cli storage COM3");
        return;
    }

    string port = args[1];
    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        await device.GetFreeStorageSizeAsync();
        await device.GetTotalStorageSizeAsync();
        var files = await device.GetFileListAsync();

        Console.WriteLine("Storage Information:");
        Console.WriteLine($"  Total: {GetHumanReadableSize(device.TotalStorageSize)}");
        Console.WriteLine($"  Free: {GetHumanReadableSize(device.FreeStorageSize)}");
        Console.WriteLine($"  Used: {GetHumanReadableSize(device.TotalStorageSize - device.FreeStorageSize)}");
        
        var usage = device.TotalStorageSize > 0 ? 
            (double)(device.TotalStorageSize - device.FreeStorageSize) / device.TotalStorageSize * 100 : 0;
        Console.WriteLine($"  Usage: {usage:F1}%");
        Console.WriteLine($"  Files: {files.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleFirmwareCommand(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Usage: busytag-cli firmware <port> <firmware_file.bin>");
        Console.WriteLine("Example: busytag-cli firmware COM3 \"firmware_v2.1.bin\"");
        return;
    }

    string port = args[1];
    string firmwarePath = string.Join(" ", args.Skip(2)).Trim('"', '\'');

    if (!File.Exists(firmwarePath))
    {
        Console.WriteLine($"Firmware file not found: {firmwarePath}");
        return;
    }

    if (!firmwarePath.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Only .bin files are supported for firmware updates");
        return;
    }

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        Console.WriteLine($"WARNING: Firmware update can brick your device!");
        Console.WriteLine($"Uploading firmware: {Path.GetFileName(firmwarePath)}");
        
        await device.SendNewFile(firmwarePath);
        await device.ActivateFileStorageScanAsync();
        
        Console.WriteLine("Firmware upload completed. Device will process the update.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Firmware update failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleFormatCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: busytag-cli format <port> [--force]");
        Console.WriteLine("Example: busytag-cli format COM3 --force");
        Console.WriteLine("WARNING: This will delete ALL data on the device!");
        return;
    }

    string port = args[1];
    bool force = args.Length > 2 && args[2] == "--force";

    if (!force)
    {
        Console.WriteLine("DANGER: This will permanently delete ALL data on the device!");
        Console.WriteLine("Use --force flag to confirm: BusyTag format <port> --force");
        return;
    }

    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        Console.WriteLine("Formatting device storage...");
        var success = await device.FormatDiskAsync();
        Console.WriteLine(success ? "Format completed successfully!" : "Format failed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Format failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static async Task HandleRestartCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: busytag-cli restart <port>");
        Console.WriteLine("Example: busytag-cli restart COM3");
        return;
    }

    string port = args[1];
    var device = new BusyTagDevice(port);
    try
    {
        await device.Connect();
        if (!device.IsConnected)
        {
            Console.WriteLine($"Failed to connect to {port}");
            return;
        }

        Console.WriteLine("Restarting device...");
        var success = await device.RestartDeviceAsync();
        Console.WriteLine(success ? "Restart command sent successfully!" : "Failed to restart device");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Restart failed: {ex.Message}");
    }
    finally
    {
        device.Disconnect();
    }
}

static void ShowVersion()
{
    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    Console.WriteLine($"BusyTag CLI v{version}");
    Console.WriteLine("BusyTag Device Manager Command Line Interface");
}

    static async Task ScanDeviceStorage()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        try
        {
            Console.WriteLine("[INFO] Scanning device storage...");
            var scanStartTime = DateTime.Now;
            
            // Get storage information
            await _currentDevice.GetFreeStorageSizeAsync();
            await _currentDevice.GetTotalStorageSizeAsync();
            var files = await _currentDevice.GetFileListAsync();
            
            var scanTime = DateTime.Now - scanStartTime;
            
            Console.WriteLine("\n[STORAGE] Device Storage Information:");
            Console.WriteLine($"   Total space: {GetHumanReadableSize(_currentDevice.TotalStorageSize)}");
            Console.WriteLine($"   Free space: {GetHumanReadableSize(_currentDevice.FreeStorageSize)}");
            Console.WriteLine($"   Used space: {GetHumanReadableSize(_currentDevice.TotalStorageSize - _currentDevice.FreeStorageSize)}");
            
            var usagePercentage = _currentDevice.TotalStorageSize > 0 ? 
                (double)(_currentDevice.TotalStorageSize - _currentDevice.FreeStorageSize) / _currentDevice.TotalStorageSize * 100 : 0;
            Console.WriteLine($"   Usage: {usagePercentage:F1}%");
            Console.WriteLine($"   Files count: {files.Count}");
            Console.WriteLine($"   Scan time: {FormatTimeSpanWithMs(scanTime)}");
            
            if (files.Count > 0)
            {
                Console.WriteLine("\n[FILES] Files on device (sorted by size):");
                var totalFileSize = 0L;
                var imageFiles = 0;
                var otherFiles = 0;
                
                foreach (var file in files.OrderByDescending(f => f.Size))
                {
                    var indicator = file.Name == _currentDevice.CurrentImageName ? " (current)" : "";
                    var fileType = IsImageFile(file.Name) ? "[IMG]" : "[FILE]";
                    Console.WriteLine($"     {fileType} {file.Name} - {GetHumanReadableSize(file.Size)}{indicator}");
                    totalFileSize += file.Size;
                    
                    if (IsImageFile(file.Name)) imageFiles++;
                    else otherFiles++;
                }
                
                Console.WriteLine($"\n[STATS] File Statistics:");
                Console.WriteLine($"   Total files size: {GetHumanReadableSize(totalFileSize)}");
                Console.WriteLine($"   Image files: {imageFiles}");
                Console.WriteLine($"   Other files: {otherFiles}");
                Console.WriteLine($"   Current image: {_currentDevice.CurrentImageName ?? "None"}");
                
                if (totalFileSize != (_currentDevice.TotalStorageSize - _currentDevice.FreeStorageSize))
                {
                    var systemSpace = (_currentDevice.TotalStorageSize - _currentDevice.FreeStorageSize) - totalFileSize;
                    Console.WriteLine($"   System/overhead: {GetHumanReadableSize(systemSpace)}");
                }
            }
            else
            {
                Console.WriteLine("\n[INFO] No files found on device.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning device storage: {ex.Message}");
        }
    }

    static bool IsImageFile(string fileName)
    {
        var imageExtensions = new[] { ".png", ".gif" };
        return imageExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    static async Task UploadFirmware()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.WriteLine("[FIRMWARE] Firmware Upload");
        Console.WriteLine("==================");
        Console.WriteLine("[WARNING] Firmware updates can potentially brick your device!");
        Console.WriteLine("   - Only upload firmware files specifically designed for your device");
        Console.WriteLine("   - Ensure stable power supply during update");
        Console.WriteLine("   - Do not disconnect device during firmware update");
        Console.WriteLine();

        Console.Write("Enter firmware file path (.bin file): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("No file path provided.");
            return;
        }

        // Handle paths with quotes
        var filePath = input;
        if ((filePath.StartsWith("\"") && filePath.EndsWith("\"")) ||
            (filePath.StartsWith("'") && filePath.EndsWith("'")))
        {
            filePath = filePath.Substring(1, filePath.Length - 2);
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        var fileInfo = new FileInfo(filePath);
        var fileName = fileInfo.Name;

        // Verify it's a .bin file
        if (!fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[ERROR] Only .bin files are supported for firmware updates.");
            return;
        }

        // Show firmware file information
        Console.WriteLine($"\nFirmware File Information:");
        Console.WriteLine($"  Name: {fileName}");
        Console.WriteLine($"  Size: {fileInfo.Length:N0} bytes ({GetHumanReadableSize(fileInfo.Length)})");
        Console.WriteLine($"  Path: {filePath}");
        Console.WriteLine($"  Current device firmware: {_currentDevice.FirmwareVersion}");

        // Final confirmation
        Console.WriteLine("\n[WARNING] FIRMWARE UPDATE CONFIRMATION");
        Console.Write("Are you sure you want to proceed with firmware update? (y/N): ");
        var confirmation = Console.ReadLine()?.Trim().ToLower();
        
        if (confirmation != "y" && confirmation != "yes")
        {
            Console.WriteLine("Firmware update cancelled.");
            return;
        }

        Console.WriteLine("\n[INFO] Starting firmware upload...");
        Console.WriteLine("[INFO] Firmware update progress will be shown below when device begins processing");
        Console.WriteLine("[WARNING] DO NOT DISCONNECT THE DEVICE DURING UPDATE!\n");

        // Setup progress tracking for upload
        var uploadStartTime = DateTime.Now;
        var lastProgressTime = DateTime.Now;
        var lastProgressBytes = 0L;
        var fileSize = fileInfo.Length;

        void OnProgress(object? sender, UploadProgressArgs args)
        {
            var now = DateTime.Now;
            var currentBytes = (long)(fileSize * args.ProgressLevel / 100.0);
            var elapsed = now - uploadStartTime;
            var sinceLastUpdate = now - lastProgressTime;

            var overallSpeed = elapsed.TotalSeconds > 0 ? currentBytes / elapsed.TotalSeconds : 0;
            var recentSpeed = sinceLastUpdate.TotalSeconds > 0.1 ? 
                (currentBytes - lastProgressBytes) / sinceLastUpdate.TotalSeconds : overallSpeed;

            var remainingBytes = fileSize - currentBytes;
            var eta = recentSpeed > 0 && remainingBytes > 0 ? TimeSpan.FromSeconds(remainingBytes / recentSpeed) : TimeSpan.Zero;
            
            // Ensure ETA is not negative or infinity
            if (eta.TotalSeconds < 0 || double.IsInfinity(eta.TotalSeconds) || double.IsNaN(eta.TotalSeconds))
            {
                eta = TimeSpan.Zero;
            }

            var progressBar = CreateProgressBar(args.ProgressLevel, 30);
            Console.Write($"\r[UPLOAD] Upload: {progressBar} {args.ProgressLevel:F1}% | " +
                         $"{GetHumanReadableSize(currentBytes)}/{GetHumanReadableSize(fileSize)} | " +
                         $"Speed: {GetHumanReadableSize((long)recentSpeed)}/s | " +
                         $"ETA: {FormatTimeSpan(eta)}");

            lastProgressTime = now;
            lastProgressBytes = currentBytes;
        }

        void OnFinished(object? sender, FileUploadFinishedArgs fileUploadFinishedArgs)
        {
            var totalTime = DateTime.Now - uploadStartTime;
            Console.WriteLine(); // New line after progress bar
            
            if (fileUploadFinishedArgs.Success)
            {
                var avgSpeed = totalTime.TotalSeconds > 0 ? fileSize / totalTime.TotalSeconds : 0;
                Console.WriteLine("[OK] Firmware file uploaded successfully!");
                Console.WriteLine($"   Upload time: {FormatTimeSpanWithMs(totalTime)}");
                Console.WriteLine($"   Average speed: {GetHumanReadableSize((long)avgSpeed)}/s");
                Console.WriteLine("\n[INFO] Activating firmware update process...");
                Console.WriteLine("[INFO] Device will now process the firmware - monitor progress below:");
                
                Task.Run(async () =>
                {
                    try
                    {
                        await _currentDevice.ActivateFileStorageScanAsync();
                        Console.WriteLine("[OK] Firmware processing activated");
                        Console.WriteLine("[INFO] Waiting for device to begin firmware update...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Error activating firmware processing: {ex.Message}");
                    }
                });
            }
            else
            {
                Console.WriteLine("[ERROR] Firmware upload failed!");
                Console.WriteLine($"   Upload time: {FormatTimeSpanWithMs(totalTime)}");
                Console.WriteLine("   Device firmware was not updated");
            }
        }

        _currentDevice.FileUploadProgress += OnProgress;
        _currentDevice.FileUploadFinished += OnFinished;

        try
        {
            await _currentDevice.SendNewFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Firmware upload error: {ex.Message}");
        }
        finally
        {
            // Cleanup event handlers
            _currentDevice.FileUploadProgress -= OnProgress;
            _currentDevice.FileUploadFinished -= OnFinished;
        }
    }

    static async Task FormatDisk()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.WriteLine("[FORMAT] Device Storage Format");
        Console.WriteLine("==============================");
        Console.WriteLine("[DANGER] FORMATTING WILL PERMANENTLY DELETE ALL DATA!");
        Console.WriteLine("   - All files on the device will be deleted permanently");
        Console.WriteLine("   - This action cannot be undone");
        Console.WriteLine("   - Device settings may be reset to defaults");
        Console.WriteLine("   - The device will be unusable until formatting completes");
        Console.WriteLine();

        // Show current storage info
        try
        {
            await _currentDevice.GetFileListAsync();
            await _currentDevice.GetFreeStorageSizeAsync();
            await _currentDevice.GetTotalStorageSizeAsync();
            
            Console.WriteLine("Current Device Storage:");
            Console.WriteLine($"   Total space: {GetHumanReadableSize(_currentDevice.TotalStorageSize)}");
            Console.WriteLine($"   Used space: {GetHumanReadableSize(_currentDevice.TotalStorageSize - _currentDevice.FreeStorageSize)}");
            Console.WriteLine($"   Files count: {_currentDevice.FileList.Count}");
            Console.WriteLine($"   Current image: {_currentDevice.CurrentImageName ?? "None"}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not retrieve storage info: {ex.Message}");
            Console.WriteLine();
        }

        // Confirmation for destructive operation
        Console.WriteLine("[WARNING] This will DELETE ALL FILES on the device!");
        Console.Write("Are you sure you want to format the disk? (y/N): ");
        var confirm1 = Console.ReadLine()?.Trim().ToLower();
        if (confirm1 != "y" && confirm1 != "yes")
        {
            Console.WriteLine("Format operation cancelled.");
            return;
        }

        Console.WriteLine("\n[FINAL WARNING] This action cannot be undone!");
        Console.Write("Proceed with formatting? (y/N): ");
        var confirm2 = Console.ReadLine()?.Trim().ToLower();
        if (confirm2 != "y" && confirm2 != "yes")
        {
            Console.WriteLine("Format operation cancelled.");
            return;
        }

        Console.WriteLine("\n[INFO] Starting disk format...");
        Console.WriteLine("[WARNING] Do not disconnect the device during formatting!");
        
        var formatStartTime = DateTime.Now;
        
        try
        {
            var success = await _currentDevice.FormatDiskAsync();
            var formatTime = DateTime.Now - formatStartTime;
            
            if (success)
            {
                Console.WriteLine($"\n[OK] Disk format completed successfully!");
                Console.WriteLine($"   Format time: {FormatTimeSpanWithMs(formatTime)}");
                Console.WriteLine("[INFO] Refreshing device information...");
                
                // Give the device a moment to complete the format
                await Task.Delay(2000);
                
                // Refresh device information
                try
                {
                    await _currentDevice.GetFileListAsync();
                    await _currentDevice.GetFreeStorageSizeAsync();
                    await _currentDevice.GetTotalStorageSizeAsync();
                    
                    Console.WriteLine("\n[POST-FORMAT] Storage Status:");
                    Console.WriteLine($"   Total space: {GetHumanReadableSize(_currentDevice.TotalStorageSize)}");
                    Console.WriteLine($"   Free space: {GetHumanReadableSize(_currentDevice.FreeStorageSize)}");
                    Console.WriteLine($"   Files count: {_currentDevice.FileList.Count}");
                    Console.WriteLine($"   Current image: {_currentDevice.CurrentImageName ?? "None"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Could not refresh device info after format: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"\n[ERROR] Disk format failed!");
                Console.WriteLine($"   Time elapsed: {FormatTimeSpanWithMs(formatTime)}");
                Console.WriteLine("   Device storage was not formatted");
            }
        }
        catch (Exception ex)
        {
            var formatTime = DateTime.Now - formatStartTime;
            Console.WriteLine($"\n[ERROR] Format operation error: {ex.Message}");
            Console.WriteLine($"   Time elapsed: {FormatTimeSpanWithMs(formatTime)}");
            Console.WriteLine("   The device may need to be disconnected and reconnected");
        }
    }

    static async Task RestartDevice()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.WriteLine("[RESTART] Device Restart");
        Console.WriteLine("========================");
        Console.WriteLine("[INFO] This will restart the BusyTag device");
        Console.WriteLine("   - Device will reboot and become temporarily unavailable");
        Console.WriteLine("   - You will need to reconnect after the restart");
        Console.WriteLine("   - Current settings will be preserved");
        Console.WriteLine();

        Console.Write("Are you sure you want to restart the device? (y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();
        
        if (confirm != "y" && confirm != "yes")
        {
            Console.WriteLine("Restart cancelled.");
            return;
        }

        var deviceName = _currentDevice.DeviceName;
        var portName = _currentDevice.PortName;
        
        Console.WriteLine($"\n[INFO] Restarting device '{deviceName}'...");
        
        try
        {
            var success = await _currentDevice.RestartDeviceAsync();
            
            if (success)
            {
                Console.WriteLine("[OK] Restart command sent successfully!");
                Console.WriteLine("[INFO] Device is now restarting...");
                
                // Disconnect our current connection since a device is restarting
                _currentDevice.Disconnect();
                
                Console.WriteLine($"[INFO] Disconnected from {portName}");
                Console.WriteLine("[INFO] Device will take a few seconds to restart");
                Console.WriteLine("[INFO] You can try to reconnect in about 3-5 seconds");;
                Console.WriteLine();
                
                // Offer to automatically reconnect
                Console.Write("Would you like to automatically attempt reconnection? (Y/n): ");
                var autoReconnect = Console.ReadLine()?.Trim().ToLower();
                
                if (autoReconnect != "n" && autoReconnect != "no")
                {
                    Console.WriteLine("[INFO] Waiting for device to restart...");
                    
                    // Wait for a device to restart
                    for (int i = 3; i > 0; i--)
                    {
                        Console.Write($"\rWaiting {i} seconds before reconnection attempt...");
                        await Task.Delay(1000);
                    }
                    Console.WriteLine();
                    
                    // Attempt to reconnect
                    Console.WriteLine($"[INFO] Attempting to reconnect to {portName}...");
                    
                    var reconnectAttempts = 0;
                    const int maxAttempts = 5;
                    
                    while (reconnectAttempts < maxAttempts)
                    {
                        try
                        {
                            reconnectAttempts++;
                            Console.WriteLine($"[INFO] Reconnection attempt {reconnectAttempts}/{maxAttempts}...");
                            
                            await ConnectToDevice(portName);
                            
                            if (_currentDevice?.IsConnected == true)
                            {
                                Console.WriteLine($"[OK] Successfully reconnected to '{_currentDevice.DeviceName}'!");
                                Console.WriteLine($"   Firmware: {_currentDevice.FirmwareVersion}");
                                Console.WriteLine($"   Device appears to have restarted successfully");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Reconnection attempt {reconnectAttempts} failed: {ex.Message}");
                        }
                        
                        if (reconnectAttempts < maxAttempts)
                        {
                            Console.WriteLine("[INFO] Waiting 3 seconds before next attempt...");
                            await Task.Delay(3000);
                        }
                    }
                    
                    Console.WriteLine($"[WARNING] Could not automatically reconnect after {maxAttempts} attempts");
                    Console.WriteLine($"[INFO] You can manually reconnect to {portName} when the device is ready");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to send restart command!");
                Console.WriteLine("   The device may not support remote restart");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Restart operation error: {ex.Message}");
            Console.WriteLine("   The device connection may have been lost");
            
            // Ensure we disconnect if there was an error
            try
            {
                _currentDevice?.Disconnect();
            }
            catch
            {
                // Ignore disconnect errors
            }
        }
    }

    static async Task RunInteractiveMode()
    {
        _manager = new BusyTagManager();
        
        // Set up event handlers
        _manager.FoundBusyTagSerialDevices += OnDevicesFound;
        _manager.DeviceConnected += OnDeviceConnected;
        _manager.DeviceDisconnected += OnDeviceDisconnected;

        Console.WriteLine("\nStarting device discovery... (searching every 3 seconds)");
        _manager.StartPeriodicDeviceSearch(3000);

        while (_isRunning)
        {
            ShowMenu();
            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ScanForDevices();
                        break;
                    case "2":
                        await ConnectToDevice();
                        break;
                    case "3":
                        await ShowCurrentDeviceInfo();
                        break;
                    case "4":
                        await SetSolidColor();
                        break;
                    case "5":
                        await SetPattern();
                        break;
                    case "6":
                        await UploadFile();
                        break;
                    case "7":
                        await ListFiles();
                        break;
                    case "8":
                        await SetBrightness();
                        break;
                    case "9":
                        await SetCurrentImage();
                        break;
                    case "10":
                        await DownloadFile();
                        break;
                    case "11":
                        await DeleteFile();
                        break;
                    case "12":
                        await ScanDeviceStorage();
                        break;
                    case "13":
                        await UploadFirmware();
                        break;
                    case "14":
                        await FormatDisk();
                        break;
                    case "15":
                        await RestartDevice();
                        break;
                    case "16":
                        DisconnectDevice();
                        break;
                    case "0":
                    case "q":
                    case "quit":
                    case "exit":
                        _isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (_isRunning)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        // Cleanup
        _manager?.StopPeriodicDeviceSearch();
        _currentDevice?.Disconnect();
        _manager?.Dispose();
        
        Console.WriteLine("\nGoodbye!");
    }

    static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("BusyTag Device Manager");
        Console.WriteLine("=====================");
        Console.WriteLine($"Current device: {(_currentDevice?.IsConnected == true ? $"{_currentDevice.DeviceName} ({_currentDevice.PortName})" : "None")}");
        Console.WriteLine();
        Console.WriteLine("1. Scan for devices");
        Console.WriteLine("2. Connect to device");
        Console.WriteLine("3. Show device info");
        Console.WriteLine("4. Set solid color");
        Console.WriteLine("5. Set pattern");
        Console.WriteLine("6. Upload file");
        Console.WriteLine("7. List files");
        Console.WriteLine("8. Set brightness");
        Console.WriteLine("9. Set current image");
        Console.WriteLine("10. Download file");
        Console.WriteLine("11. Delete file");
        Console.WriteLine("12. Scan device storage");
        Console.WriteLine("13. Upload firmware (.bin)");
        Console.WriteLine("14. Format disk (DANGER!)");
        Console.WriteLine("15. Restart device");
        Console.WriteLine("16. Disconnect");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Enter your choice: ");
    }

static void ShowHelp()
{
    Console.WriteLine("BusyTag Device Manager CLI");
    Console.WriteLine("==========================");
    Console.WriteLine("Usage: busytag-cli [command] [arguments]");
    Console.WriteLine();
    Console.WriteLine("Device Management:");
    Console.WriteLine("  scan, list              - Scan for BusyTag devices");
    Console.WriteLine("  connect <port>          - Connect to device on specified port");
    Console.WriteLine("  info <port>             - Show device information");
    Console.WriteLine("  restart <port>          - Restart the device");
    Console.WriteLine();
    Console.WriteLine("Display Control:");
    Console.WriteLine("  color <port> <color> [brightness] [led_bits]");
    Console.WriteLine("                          - Set solid color (name/hex/RGB)");
    Console.WriteLine("  brightness <port> <level>");
    Console.WriteLine("                          - Set display brightness (0-100)");
    Console.WriteLine("  pattern <port> <name>   - Set LED pattern");
    Console.WriteLine("  show <port> <filename>  - Display image file");
    Console.WriteLine();
    Console.WriteLine("File Management:");
    Console.WriteLine("  upload <port> <file>    - Upload file to device");
    Console.WriteLine("  download <port> <filename> <dest>");
    Console.WriteLine("                          - Download file from device");
    Console.WriteLine("  delete <port> <filename>");
    Console.WriteLine("                          - Delete file from device");
    Console.WriteLine("  files <port>            - List files on device");
    Console.WriteLine();
    Console.WriteLine("Storage Operations:");
    Console.WriteLine("  storage <port>          - Show storage information");
    Console.WriteLine("  format <port> --force   - Format device storage (DANGER!)");
    Console.WriteLine();
    Console.WriteLine("Firmware:");
    Console.WriteLine("  firmware <port> <file.bin>");
    Console.WriteLine("                          - Upload firmware to device");
    Console.WriteLine();
    Console.WriteLine("Other:");
    Console.WriteLine("  version, -v, --version  - Show version information");
    Console.WriteLine("  help, -h, --help        - Show this help message");
    Console.WriteLine();
    Console.WriteLine("Color Formats:");
    Console.WriteLine("  Named colors: red, green, blue, yellow, cyan, magenta, white, off");
    Console.WriteLine("  Hex colors: FF0000, #FF0000 (red)");
    Console.WriteLine("  RGB values: 255,0,0 (red)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  busytag-cli scan");
    Console.WriteLine("  busytag-cli connect COM3");
    Console.WriteLine("  busytag-cli info /dev/ttyUSB0");
    Console.WriteLine("  busytag-cli color COM3 red 75");
    Console.WriteLine("  busytag-cli color COM3 FF0000 50 127");
    Console.WriteLine("  busytag-cli color COM3 255,128,0");
    Console.WriteLine("  busytag-cli brightness COM3 80");
    Console.WriteLine("  busytag-cli upload COM3 \"photo.png\"");
    Console.WriteLine("  busytag-cli files COM3");
    Console.WriteLine("  busytag-cli show COM3 photo.png");
    Console.WriteLine("  busytag-cli storage COM3");
    Console.WriteLine("  busytag-cli delete COM3 old_image.png");
    Console.WriteLine("  busytag-cli firmware COM3 \"firmware_v2.1.bin\"");
    Console.WriteLine("  busytag-cli format COM3 --force");
    Console.WriteLine();
    Console.WriteLine("Run without arguments for interactive mode.");
}

    static async Task ScanForDevices()
    {
        Console.WriteLine("Scanning for BusyTag devices...");
        
        if (_manager == null)
        {
            _manager = new BusyTagManager();
        }

        var devices = await _manager.FindBusyTagDevice();
        
        if (devices == null || devices.Count == 0)
        {
            Console.WriteLine("No BusyTag devices found.");
        }
        else
        {
            Console.WriteLine($"Found {devices.Count} device(s):");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {devices[i]}");
            }
        }
    }

    static async Task ConnectToDevice(string? portName = null)
    {
        if (portName == null)
        {
            if (_manager == null)
            {
                _manager = new BusyTagManager();
            }

            var devices = await _manager.FindBusyTagDevice();
            
            if (devices == null || devices.Count == 0)
            {
                Console.WriteLine("No devices found. Please scan for devices first.");
                return;
            }

            Console.WriteLine("Available devices:");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {devices[i]}");
            }

            Console.Write("Select device (number): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= devices.Count)
            {
                portName = devices[choice - 1];
            }
            else
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
        }

        try
        {
            Console.WriteLine($"Connecting to {portName}...");
            
            _currentDevice?.Disconnect();
            _currentDevice = new BusyTagDevice(portName);
            
            // Set up event handlers
            SetupDeviceEventHandlers(_currentDevice);
            
            await _currentDevice.Connect();
            
            if (_currentDevice.IsConnected)
            {
                Console.WriteLine($"Successfully connected to {_currentDevice.DeviceName}!");
                Console.WriteLine($"Firmware: {_currentDevice.FirmwareVersion}");
                Console.WriteLine($"ID: {_currentDevice.Id}");
            }
            else
            {
                Console.WriteLine("Failed to connect to device.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }

    static async Task ShowDeviceInfo(string? portName = null)
    {
        BusyTagDevice? device = _currentDevice;
        
        if (portName != null)
        {
            device = new BusyTagDevice(portName);
            await device.Connect();
        }

        if (device == null || !device.IsConnected)
        {
            Console.WriteLine("No device connected. Please connect to a device first.");
            return;
        }

        Console.WriteLine("Device Information:");
        Console.WriteLine($"  Name: {device.DeviceName}");
        Console.WriteLine($"  Manufacturer: {device.ManufactureName}");
        Console.WriteLine($"  ID: {device.Id}");
        Console.WriteLine($"  Firmware: {device.FirmwareVersion}");
        Console.WriteLine($"  Port: {device.PortName}");
        Console.WriteLine($"  Current Image: {device.CurrentImageName}");
        Console.WriteLine($"  Storage: {await device.GetFreeStorageSizeAsync():N0} / {await device.GetTotalStorageSizeAsync():N0} bytes free");
        Console.WriteLine($"  Files: {device.FileList.Count}");

        if (portName != null && device != _currentDevice)
        {
            device.Disconnect();
        }
    }

    static async Task ShowCurrentDeviceInfo()
    {
        await ShowDeviceInfo();
    }

    static async Task SetSolidColor()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.WriteLine("Set solid color options:");
        Console.WriteLine("1. Predefined colors: red, green, blue, yellow, cyan, magenta, white, off");
        Console.WriteLine("2. Hex color (e.g., FF0000 for red, 00FF00 for green)");
        Console.WriteLine("3. RGB values (e.g., 255,0,0 for red)");
        Console.WriteLine();
        
        Console.Write("Enter color (name/hex/RGB): ");
        var colorInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(colorInput))
        {
            Console.WriteLine("No color specified.");
            return;
        }

        Console.Write("Enter brightness (0-100, default 100): ");
        var brightnessInput = Console.ReadLine()?.Trim();
        int brightness = 100;
        if (!string.IsNullOrEmpty(brightnessInput))
        {
            int.TryParse(brightnessInput, out brightness);
        }

        Console.Write("Enter LED bits (1-127, default 127 for all LEDs): ");
        var ledBitsInput = Console.ReadLine()?.Trim();
        int ledBits = 127;
        if (!string.IsNullOrEmpty(ledBitsInput))
        {
            int.TryParse(ledBitsInput, out ledBits);
        }

        // Clamp values
        brightness = Math.Clamp(brightness, 0, 100);
        ledBits = Math.Clamp(ledBits, 1, 127);

        bool success = false;

        try
        {
            // Check if it's an RGB format (e.g., "255,0,0")
            if (colorInput.Contains(','))
            {
                var rgbParts = colorInput.Split(',');
                if (rgbParts.Length == 3 && 
                    int.TryParse(rgbParts[0].Trim(), out int r) &&
                    int.TryParse(rgbParts[1].Trim(), out int g) &&
                    int.TryParse(rgbParts[2].Trim(), out int b))
                {
                    // Apply brightness scaling
                    r = (int)(r * brightness / 100.0);
                    g = (int)(g * brightness / 100.0);
                    b = (int)(b * brightness / 100.0);
                    
                    // Clamp to 0-255
                    r = Math.Clamp(r, 0, 255);
                    g = Math.Clamp(g, 0, 255);
                    b = Math.Clamp(b, 0, 255);
                    
                    success = await _currentDevice.SendRgbColorAsync(r, g, b, ledBits);
                    Console.WriteLine(success ? 
                        $"RGB color set successfully! R:{r}, G:{g}, B:{b}, Brightness:{brightness}%, LEDs:{ledBits}" : 
                        "Failed to set RGB color.");
                    return;
                }
            }
            
            // Check if it's a hex format (e.g., "FF0000" or "#FF0000")
            var hexColor = colorInput.Replace("#", "").ToUpper();
            if (hexColor.Length == 6 && IsValidHex(hexColor))
            {
                var r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                var g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                var b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                
                // Apply brightness scaling
                r = (int)(r * brightness / 100.0);
                g = (int)(g * brightness / 100.0);
                b = (int)(b * brightness / 100.0);
                
                success = await _currentDevice.SendRgbColorAsync(r, g, b, ledBits);
                Console.WriteLine(success ? 
                    $"Hex color set successfully! #{hexColor}, RGB({r},{g},{b}), Brightness:{brightness}%, LEDs:{ledBits}" : 
                    "Failed to set hex color.");
                return;
            }
            
            // Otherwise, treat as predefined color name
            success = await _currentDevice.SetSolidColorAsync(colorInput.ToLower(), brightness, ledBits);
            Console.WriteLine(success ? 
                $"Color '{colorInput}' set successfully! Brightness:{brightness}%, LEDs:{ledBits}" : 
                "Failed to set color. Use: red, green, blue, yellow, cyan, magenta, white, off");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting color: {ex.Message}");
        }
    }

    static bool IsValidHex(string hex)
    {
        return hex.All(c => "0123456789ABCDEF".Contains(c));
    }

    static async Task SetPattern()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.WriteLine("Available patterns:");
        var patterns = PatternListCommands.PatternList;
        var patternNames = patterns.Keys.ToList();
        
        for (int i = 0; i < patternNames.Count; i++)
        {
            var pattern = patterns[patternNames[i]];
            Console.WriteLine($"  {i + 1}. {pattern?.Name}");
        }

        Console.Write("Select pattern (number): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= patternNames.Count)
        {
            var selectedPattern = patterns[patternNames[choice - 1]];
            if (selectedPattern != null)
            {
                var success = await _currentDevice.SetNewCustomPattern(selectedPattern.PatternLines, true, false);
                Console.WriteLine(success ? "Pattern set successfully!" : "Failed to set pattern.");
            }
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    static async Task UploadFile()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        Console.Write("Enter file path (with or without quotes): ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("No file path provided.");
            return;
        }

        // Handle paths with quotes - remove surrounding quotes if present
        var filePath = input;
        if ((filePath.StartsWith("\"") && filePath.EndsWith("\"")) ||
            (filePath.StartsWith("'") && filePath.EndsWith("'")))
        {
            filePath = filePath.Substring(1, filePath.Length - 2);
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        var fileInfo = new FileInfo(filePath);
        var fileName = fileInfo.Name;
        var fileSize = fileInfo.Length;

        // Check if the filename is too long
        if (fileName.Length > 40) // MaxFilenameLength from BusyTagDevice
        {
            Console.WriteLine($"Filename too long ({fileName.Length} chars). Maximum allowed: 40 characters.");
            Console.WriteLine($"Current filename: {fileName}");
            return;
        }

        // Show file information
        Console.WriteLine($"\nFile Information:");
        Console.WriteLine($"  Name: {fileName}");
        Console.WriteLine($"  Size: {fileSize:N0} bytes ({GetHumanReadableSize(fileSize)})");
        Console.WriteLine($"  Path: {filePath}");

        // Check available space
        await _currentDevice.GetFreeStorageSizeAsync();
        Console.WriteLine($"  Device free space: {_currentDevice.FreeStorageSize:N0} bytes ({GetHumanReadableSize(_currentDevice.FreeStorageSize)})");

        if (fileSize > _currentDevice.FreeStorageSize)
        {
            Console.WriteLine($"\n[WARNING] File size exceeds available storage space.");
            Console.Write("Continue anyway? (y/N): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Upload cancelled.");
                return;
            }
        }

        Console.Write("\nProceed with upload? (Y/n): ");
        var proceed = Console.ReadLine()?.Trim().ToLower();
        if (proceed == "n" || proceed == "no")
        {
            Console.WriteLine("Upload cancelled.");
            return;
        }

        Console.WriteLine("\nStarting file upload...");
        
        // Setup progress tracking
        var uploadStartTime = DateTime.Now;
        var lastProgressTime = DateTime.Now;
        var lastProgressBytes = 0L;

        // Subscribe to progress events
        void OnProgress(object? sender, UploadProgressArgs args)
        {
            var now = DateTime.Now;
            var currentBytes = (long)(fileSize * args.ProgressLevel / 100.0);
            var elapsed = now - uploadStartTime;
            var sinceLastUpdate = now - lastProgressTime;

            // Calculate speed (bytes per second)
            var overallSpeed = elapsed.TotalSeconds > 0 ? currentBytes / elapsed.TotalSeconds : 0;
            var recentSpeed = sinceLastUpdate.TotalSeconds > 0.1 ? 
                (currentBytes - lastProgressBytes) / sinceLastUpdate.TotalSeconds : overallSpeed;

            // Calculate ETA
            var remainingBytes = fileSize - currentBytes;
            var eta = recentSpeed > 0 && remainingBytes > 0 ? TimeSpan.FromSeconds(remainingBytes / recentSpeed) : TimeSpan.Zero;
            
            // Ensure ETA is not negative or infinity
            if (eta.TotalSeconds < 0 || double.IsInfinity(eta.TotalSeconds) || double.IsNaN(eta.TotalSeconds))
            {
                eta = TimeSpan.Zero;
            }

            // Update progress display
            var progressBar = CreateProgressBar(args.ProgressLevel, 30);
            Console.Write($"\r{progressBar} {args.ProgressLevel:F1}% | " +
                         $"{GetHumanReadableSize(currentBytes)}/{GetHumanReadableSize(fileSize)} | " +
                         $"Speed: {GetHumanReadableSize((long)recentSpeed)}/s | " +
                         $"ETA: {FormatTimeSpan(eta)}");

            lastProgressTime = now;
            lastProgressBytes = currentBytes;
        }

        void OnFinished(object? sender, FileUploadFinishedArgs fileUploadFinishedArgs)
        {
            var totalTime = DateTime.Now - uploadStartTime;
            Console.WriteLine(); // New line after the progress bar
            
            if (fileUploadFinishedArgs.Success)
            {
                var avgSpeed = totalTime.TotalSeconds > 0 ? fileSize / totalTime.TotalSeconds : 0;
                Console.WriteLine("[OK] Upload completed successfully!");
                Console.WriteLine($"   Time: {FormatTimeSpanWithMs(totalTime)}");
                Console.WriteLine($"   Average speed: {GetHumanReadableSize((long)avgSpeed)}/s");
                
                // Check if an uploaded file is a .bin file and trigger a storage scan
                if (fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\n[INFO] Binary file detected - activating device storage scan...");
                    Console.WriteLine("[INFO] Monitoring firmware update progress...");
                    Console.WriteLine("   Note: If this is a firmware file, update progress will be shown below");
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            var scanSuccess = await _currentDevice?.ActivateFileStorageScanAsync();
                            if (scanSuccess)
                            {
                                Console.WriteLine("[OK] Device storage scan activated successfully!");
                                Console.WriteLine("   Device is now scanning and updating file system...");
                                
                                // Wait a moment for the scan to process
                                await Task.Delay(1000);
                                
                                // Refresh file list after scan
                                Console.WriteLine("[INFO] Refreshing file list...");
                                await _currentDevice.GetFileListAsync();
                                Console.WriteLine($"   Files on device: {_currentDevice.FileList.Count}");
                            }
                            else
                            {
                                Console.WriteLine("[WARNING] Storage scan activation failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Error activating storage scan: {ex.Message}");
                        }
                    });
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Upload failed!");
                Console.WriteLine($"   Time elapsed: {FormatTimeSpanWithMs(totalTime)}");
            }
        }

        _currentDevice.FileUploadProgress += OnProgress;
        _currentDevice.FileUploadFinished += OnFinished;

        try
        {
            await _currentDevice.SendNewFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Upload error: {ex.Message}");
        }
        finally
        {
            // Cleanup event handlers
            _currentDevice.FileUploadProgress -= OnProgress;
            _currentDevice.FileUploadFinished -= OnFinished;
        }
    }

    static string CreateProgressBar(float percentage, int width)
    {
        var filled = (int)(percentage / 100.0 * width);
        var bar = new string('█', filled) + new string('░', width - filled);
        return $"[{bar}]";
    }

    static string GetHumanReadableSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:N1} {suffixes[counter]}";
    }

    static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 1)
            return "0s";
        else if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.TotalSeconds:F0}s";
        else if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        else
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }

    static string FormatTimeSpanWithMs(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 1)
            return $"{timeSpan.TotalMilliseconds:F0}ms";
        else if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.TotalSeconds:F3}s";
        else if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}.{timeSpan.Milliseconds:D3}s";
        else
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}.{timeSpan.Milliseconds:D3}s";
    }

    static async Task ListFiles()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        var files = await _currentDevice.GetFileListAsync();
        
        if (files.Count == 0)
        {
            Console.WriteLine("No files found on device.");
        }
        else
        {
            Console.WriteLine($"Files on device ({files.Count}):");
            foreach (var file in files)
            {
                Console.WriteLine($"  {file.Name} ({file.Size:N0} bytes)");
            }
        }
    }

    static async Task SetBrightness()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        try
        {
            // Get and display current brightness
            var currentBrightness = await _currentDevice.GetDisplayBrightnessAsync();
            Console.WriteLine($"Current brightness: {currentBrightness}%");
            
            Console.Write($"Enter new brightness (0-100, current: {currentBrightness}%, or press Enter to keep current): ");
            var input = Console.ReadLine()?.Trim();
            
            // If the user just pressed Enter, keep the current brightness
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Brightness unchanged.");
                return;
            }
            
            if (int.TryParse(input, out int brightness) && brightness >= 0 && brightness <= 100)
            {
                var success = await _currentDevice.SetDisplayBrightnessAsync(brightness);
                Console.WriteLine(success ? $"Brightness set to {brightness}% successfully!" : "Failed to set brightness.");
            }
            else
            {
                Console.WriteLine("Invalid brightness value. Must be between 0 and 100.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting/setting brightness: {ex.Message}");
        }
    }

    static async Task SetCurrentImage()
{
    if (_currentDevice?.IsConnected != true)
    {
        Console.WriteLine("No device connected.");
        return;
    }

    try
    {
        // Show the current image
        Console.WriteLine($"Current image: {(_currentDevice.CurrentImageName ?? "None")}");
        
        // Get and display available files
        var files = await _currentDevice.GetFileListAsync();
        var imageFiles = files.Where(f => 
            f.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            f.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (imageFiles.Count == 0)
        {
            Console.WriteLine("No image files found on device. Upload some PNG or GIF images first.");
            return;
        }

        Console.WriteLine("\nAvailable images:");
        for (int i = 0; i < imageFiles.Count; i++)
        {
            var indicator = imageFiles[i].Name == _currentDevice.CurrentImageName ? " (current)" : "";
            Console.WriteLine($"  {i + 1}. {imageFiles[i].Name}{indicator} ({imageFiles[i].Size:N0} bytes)");
        }

        Console.Write($"\nSelect image (1-{imageFiles.Count}) or press Enter to cancel: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        if (int.TryParse(input, out int choice) && choice > 0 && choice <= imageFiles.Count)
        {
            var selectedFile = imageFiles[choice - 1];
            Console.WriteLine($"Setting image to: {selectedFile.Name}");
            
            var success = await _currentDevice.ShowPictureAsync(selectedFile.Name);
            if (success)
            {
                Console.WriteLine($"Successfully set current image to: {selectedFile.Name}");
            }
            else
            {
                Console.WriteLine("Failed to set image.");
            }
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting current image: {ex.Message}");
    }
}

    static async Task DownloadFile()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        try
        {
            var files = await _currentDevice.GetFileListAsync();
            
            if (files.Count == 0)
            {
                Console.WriteLine("No files found on device.");
                return;
            }

            Console.WriteLine("\nFiles available for download:");
            for (int i = 0; i < files.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {files[i].Name} ({GetHumanReadableSize(files[i].Size)})");
            }

            Console.Write($"\nSelect file to download (1-{files.Count}) or press Enter to cancel: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Download cancelled.");
                return;
            }

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > files.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedFile = files[choice - 1];
            
            Console.Write($"Enter download path (or press Enter for current directory): ");
            var downloadPath = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(downloadPath))
            {
                downloadPath = Environment.CurrentDirectory;
            }

            // Handle quoted paths
            if ((downloadPath.StartsWith("\"") && downloadPath.EndsWith("\"")) ||
                (downloadPath.StartsWith("'") && downloadPath.EndsWith("'")))
            {
                downloadPath = downloadPath.Substring(1, downloadPath.Length - 2);
            }

            if (!Directory.Exists(downloadPath))
            {
                Console.WriteLine($"Directory not found: {downloadPath}");
                return;
            }

            var fullPath = Path.Combine(downloadPath, selectedFile.Name);
            
            if (File.Exists(fullPath))
            {
                Console.Write($"File '{selectedFile.Name}' already exists. Overwrite? (y/N): ");
                var overwrite = Console.ReadLine()?.Trim().ToLower();
                if (overwrite != "y" && overwrite != "yes")
                {
                    Console.WriteLine("Download cancelled.");
                    return;
                }
            }

            Console.WriteLine($"Downloading {selectedFile.Name}...");
            var downloadStartTime = DateTime.Now;

            var result = await _currentDevice.GetFileAsync(selectedFile.Name);
            
            if (!string.IsNullOrEmpty(result))
            {
                File.Copy(result, fullPath, true);
                
                var downloadTime = DateTime.Now - downloadStartTime;
                var avgSpeed = downloadTime.TotalSeconds > 0 ? selectedFile.Size / downloadTime.TotalSeconds : 0;
                
                Console.WriteLine("[OK] Download completed successfully!");
                Console.WriteLine($"   File: {fullPath}");
                Console.WriteLine($"   Size: {GetHumanReadableSize(selectedFile.Size)}");
                Console.WriteLine($"   Time: {FormatTimeSpanWithMs(downloadTime)}");
                Console.WriteLine($"   Average speed: {GetHumanReadableSize((long)avgSpeed)}/s");
            }
            else
            {
                Console.WriteLine("[ERROR] Download failed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
        }
    }

    static async Task DeleteFile()
    {
        if (_currentDevice?.IsConnected != true)
        {
            Console.WriteLine("No device connected.");
            return;
        }

        try
        {
            var files = await _currentDevice.GetFileListAsync();
            
            if (files.Count == 0)
            {
                Console.WriteLine("No files found on device.");
                return;
            }

            Console.WriteLine("\nFiles available for deletion:");
            for (int i = 0; i < files.Count; i++)
            {
                var indicator = files[i].Name == _currentDevice.CurrentImageName ? " (currently displayed)" : "";
                Console.WriteLine($"  {i + 1}. {files[i].Name} ({GetHumanReadableSize(files[i].Size)}){indicator}");
            }

            Console.Write($"\nSelect file to delete (1-{files.Count}) or press Enter to cancel: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Delete cancelled.");
                return;
            }

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > files.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedFile = files[choice - 1];
            
            if (selectedFile.Name == _currentDevice.CurrentImageName)
            {
                Console.WriteLine("[WARNING] This is the currently displayed image!");
            }

            Console.Write($"Are you sure you want to delete '{selectedFile.Name}'? This cannot be undone! (y/N): ");
            var confirm = Console.ReadLine()?.Trim().ToLower();
            
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Delete cancelled.");
                return;
            }

            Console.WriteLine($"Deleting {selectedFile.Name}...");
            
            var success = await _currentDevice.DeleteFile(selectedFile.Name);
            
            if (success)
            {
                Console.WriteLine($"[OK] File '{selectedFile.Name}' deleted successfully!");
                
                await _currentDevice.GetFreeStorageSizeAsync();
                Console.WriteLine($"   Free space: {GetHumanReadableSize(_currentDevice.FreeStorageSize)}");
            }
            else
            {
                Console.WriteLine($"[ERROR] Failed to delete '{selectedFile.Name}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }

    static void DisconnectDevice()
    {
        if (_currentDevice?.IsConnected == true)
        {
            _currentDevice.Disconnect();
            Console.WriteLine("Device disconnected.");
        }
        else
        {
            Console.WriteLine("No device connected.");
        }
    }

    static void SetupDeviceEventHandlers(BusyTagDevice device)
    {
        device.ConnectionStateChanged += (sender, connected) =>
        {
            Console.WriteLine($"Device {(connected ? "connected" : "disconnected")}");
        };

        device.ReceivedShowingPicture += (sender, imageName) =>
        {
            Console.WriteLine($"Now showing: {imageName}");
        };

        device.FirmwareUpdateStatus += (sender, progress) =>
        {
            var progressBar = CreateProgressBar(progress, 40);
            Console.Write($"\rFirmware Update: {progressBar} {progress:F1}%");
            
            if (progress >= 100.0f)
            {
                Console.WriteLine(); // New line when complete
                Console.WriteLine("[OK] Firmware update completed successfully!");
                Console.WriteLine("[WARNING] Device may restart automatically...");
            }
        };

        device.WritingInStorage += (sender, isWriting) =>
        {
            // if (isWriting)
            // {
            //     Console.WriteLine("[INFO] Device is writing to storage...");
            // }
            // else
            // {
            //     Console.WriteLine("[OK] Storage write operation completed");
            // }
        };
    }
    
    static void OnDevicesFound(object? sender, List<string>? devices)
    {
        var now = DateTime.Now;
        
        if (devices != null && devices.Count > 0)
        {
            bool devicesChanged = _lastFoundDevices == null || !devices.SequenceEqual(_lastFoundDevices);
            bool timeToUpdate = (now - _lastDeviceMessage).TotalSeconds > 30;
            
            if (devicesChanged || timeToUpdate)
            {
                if (devicesChanged)
                {
                    Console.WriteLine($"\n[FOUND] {devices.Count} BusyTag device(s): {string.Join(", ", devices)}");
                }
                _lastFoundDevices = devices.ToList();
                _lastDeviceMessage = now;
            }
        }
        else if (_lastFoundDevices != null && _lastFoundDevices.Count > 0)
        {
            Console.WriteLine("\n[INFO] No BusyTag devices found");
            _lastFoundDevices = null;
            _lastDeviceMessage = now;
        }
    }

    static void OnDeviceConnected(object? sender, string portName)
    {
        Console.WriteLine($"\nDevice connected: {portName}");
    }

    static void OnDeviceDisconnected(object? sender, string portName)
    {
        Console.WriteLine($"\nDevice disconnected: {portName}");
    }
}