using System.Diagnostics;
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
                    Console.WriteLine("Usage: BusyTag connect <port_name>");
                }
                break;
            case "info":
                if (args.Length > 1)
                {
                    await ShowDeviceInfo(args[1]);
                }
                else
                {
                    Console.WriteLine("Usage: BusyTag info <port_name>");
                }
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

    static async Task RunInteractiveMode()
    {
        _manager = new BusyTagManager();
        
        // Disable verbose logging to reduce console spam
        _manager.EnableVerboseLogging = false;
        
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
        Console.WriteLine("12. Disconnect");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Enter your choice: ");
    }

    static void ShowHelp()
    {
        Console.WriteLine("BusyTag Device Manager CLI");
        Console.WriteLine("Usage: BusyTag [command] [arguments]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  scan, list          - Scan for BusyTag devices");
        Console.WriteLine("  connect <port>      - Connect to device on specified port");
        Console.WriteLine("  info <port>         - Show device information");
        Console.WriteLine("  help                - Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  BusyTag scan");
        Console.WriteLine("  BusyTag connect COM3");
        Console.WriteLine("  BusyTag info /dev/tty.usbserial-1234");
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
            // Check if it's RGB format (e.g., "255,0,0")
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
            
            // Check if it's hex format (e.g., "FF0000" or "#FF0000")
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

        // Check if filename is too long
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
            Console.WriteLine("⚠️  Warning: File size exceeds available storage space.");
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
            var eta = recentSpeed > 0 ? TimeSpan.FromSeconds(remainingBytes / recentSpeed) : TimeSpan.Zero;

            // Update progress display
            var progressBar = CreateProgressBar(args.ProgressLevel, 30);
            Console.Write($"\r{progressBar} {args.ProgressLevel:F1}% | " +
                         $"{GetHumanReadableSize(currentBytes)}/{GetHumanReadableSize(fileSize)} | " +
                         $"Speed: {GetHumanReadableSize((long)recentSpeed)}/s | " +
                         $"ETA: {FormatTimeSpan(eta)}");

            lastProgressTime = now;
            lastProgressBytes = currentBytes;
        }

        void OnFinished(object? sender, bool success)
        {
            var totalTime = DateTime.Now - uploadStartTime;
            Console.WriteLine(); // New line after progress bar
            
            if (success)
            {
                var avgSpeed = totalTime.TotalSeconds > 0 ? fileSize / totalTime.TotalSeconds : 0;
                Console.WriteLine($"✅ Upload completed successfully!");
                Console.WriteLine($"   Time: {FormatTimeSpan(totalTime)}");
                Console.WriteLine($"   Average speed: {GetHumanReadableSize((long)avgSpeed)}/s");
            }
            else
            {
                Console.WriteLine($"❌ Upload failed!");
                Console.WriteLine($"   Time elapsed: {FormatTimeSpan(totalTime)}");
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
            Console.WriteLine($"\n❌ Upload error: {ex.Message}");
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
        if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.TotalSeconds:F0}s";
        else if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        else
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
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
            
            // If user just pressed Enter, keep current brightness
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
            // Show current image
            Console.WriteLine($"Current image: {(_currentDevice.CurrentImageName ?? "None")}");
            
            // Get and display available files
            var files = await _currentDevice.GetFileListAsync();
            var imageFiles = files.Where(f => 
                f.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                f.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (imageFiles.Count == 0)
            {
                Console.WriteLine("No image files found on device. Upload some images first.");
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
                
                Console.WriteLine($"✅ Download completed successfully!");
                Console.WriteLine($"   File: {fullPath}");
                Console.WriteLine($"   Size: {GetHumanReadableSize(selectedFile.Size)}");
                Console.WriteLine($"   Time: {FormatTimeSpan(downloadTime)}");
                Console.WriteLine($"   Average speed: {GetHumanReadableSize((long)avgSpeed)}/s");
            }
            else
            {
                Console.WriteLine("❌ Download failed!");
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
                Console.WriteLine("⚠️  Warning: This is the currently displayed image!");
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
                Console.WriteLine($"✅ File '{selectedFile.Name}' deleted successfully!");
                
                await _currentDevice.GetFreeStorageSizeAsync();
                Console.WriteLine($"   Free space: {GetHumanReadableSize(_currentDevice.FreeStorageSize)}");
            }
            else
            {
                Console.WriteLine($"❌ Failed to delete '{selectedFile.Name}'");
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
                    Console.WriteLine($"\n Found {devices.Count} BusyTag device(s): {string.Join(", ", devices)}");
                }
                _lastFoundDevices = devices.ToList();
                _lastDeviceMessage = now;
            }
        }
        else if (_lastFoundDevices != null && _lastFoundDevices.Count > 0)
        {
            Console.WriteLine("\n✗ No BusyTag devices found");
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
