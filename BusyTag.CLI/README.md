# BusyTag.CLI - Technical Documentation

This directory contains the BusyTag CLI application source code and comprehensive technical documentation.

## 📋 Table of Contents

- [Project Structure](#-project-structure)
- [Installation Methods](#-installation-methods)
- [Complete Command Reference](#-complete-command-reference)
- [Advanced Usage](#-advanced-usage)
- [Development](#-development)
- [API Reference](#-api-reference)

## 📁 Project Structure

```
BusyTag.CLI/
├── Program.cs              # Main application entry point
├── BusyTag.CLI.csproj      # Project configuration
└── README.md               # This file
```

## 🚀 Installation Methods

### Method 1: Homebrew (macOS - Recommended)

Install using Homebrew for the easiest setup on macOS:

```bash
# Add the BusyTag tap
brew tap busy-tag/busytag

# Install BusyTag CLI
brew install busytag-cli

# Verify installation
busytag-cli --version

# Update to latest version
brew update && brew upgrade busytag-cli

# Uninstall if needed
brew uninstall busytag-cli
```

**✅ Advantages:**
- **No .NET runtime required** - Self-contained executable
- **Automatic updates** - Homebrew keeps it current
- **Native performance** - Optimized for your Mac architecture
- **Easy management** - Standard Homebrew commands

### Method 2: .NET Global Tool (Cross-Platform)

Install as a global tool using the .NET CLI (requires .NET 8.0+ runtime):

```bash
# Install .NET 8.0+ first if not already installed
# Download from: https://dotnet.microsoft.com/download

# Install the latest version
dotnet tool install -g BusyTag.CLI

# Verify installation
busytag-cli --version

# Update to latest version
dotnet tool update -g BusyTag.CLI

# Uninstall if needed
dotnet tool uninstall -g BusyTag.CLI
```

### Method 3: From Source

For development or building from source:

```bash
# Clone the repository
git clone https://github.com/busy-tag/busytag-cli.git
cd busytag-cli/BusyTag.CLI

# Build and run
dotnet build -c Release
dotnet run -- --help

# Or build self-contained executable
dotnet publish -c Release --self-contained -r win-x64    # Windows
dotnet publish -c Release --self-contained -r osx-x64    # macOS Intel
dotnet publish -c Release --self-contained -r osx-arm64  # macOS Apple Silicon
dotnet publish -c Release --self-contained -r linux-x64  # Linux
```

## 📖 Complete Command Reference

### Device Management Commands

#### Device Discovery
```bash
busytag-cli scan                              # Scan for devices
busytag-cli list                              # Alias for scan
```

#### Connection Management
```bash
busytag-cli connect <port>                    # Connect to specific device
busytag-cli info <port>                       # Show device information
busytag-cli restart <port>                    # Restart device remotely
```

### Display Control Commands

#### Color Control
```bash
busytag-cli color <port> <color> [brightness] [led_bits]
```

**Color Formats:**
- **Named colors:** `red`, `green`, `blue`, `yellow`, `cyan`, `magenta`, `white`, `off`
- **Hex colors:** `FF0000`, `#FF0000`
- **RGB values:** `255,0,0`

**Parameters:**
- `brightness`: 0-100 (default: 100)
- `led_bits`: 1-127 (default: 127, controls which LEDs to light)

**Examples:**
```bash
busytag-cli color COM3 red                    # Full red
busytag-cli color COM3 red 75                 # Red at 75% brightness
busytag-cli color COM3 FF0000 50 127          # Hex red, 50% brightness, all LEDs
busytag-cli color COM3 255,128,0              # Orange using RGB
busytag-cli color COM3 blue 100 64            # Blue on specific LED pattern
```

#### Brightness Control
```bash
busytag-cli brightness <port> <level>         # Set brightness (0-100)
```

#### Pattern Control
```bash
busytag-cli pattern <port> <pattern_name>     # Apply LED pattern
```

**Available patterns:**
- `rainbow`
- `pulse`
- `strobe`
- `fade`
- `custom` (user-defined)

#### Image Display
```bash
busytag-cli show <port> <filename>            # Display image file
busytag-cli display <port> <filename>         # Alias for show
```

### File Management Commands

#### Upload Operations
```bash
busytag-cli upload <port> <file_path>         # Upload file to device
```

**Supported formats:**
- **Images:** PNG, GIF (240x280px)
- **Firmware:** .bin files
- **Maximum filename length:** 40 characters

#### Download Operations
```bash
busytag-cli download <port> <filename> <destination_path>
```

#### File Listing
```bash
busytag-cli files <port>                      # List all files
busytag-cli ls <port>                         # Alias for files
```

#### File Deletion
```bash
busytag-cli delete <port> <filename>          # Delete specific file
busytag-cli remove <port> <filename>          # Alias for delete
```

### Storage Operations

#### Storage Information
```bash
busytag-cli storage <port>                    # Show storage statistics
busytag-cli space <port>                      # Alias for storage
```

#### Storage Management
```bash
busytag-cli format <port> --force             # Format device storage (DESTRUCTIVE!)
```

### Firmware Management

#### Firmware Upload
```bash
busytag-cli firmware <port> <firmware.bin>    # Upload firmware update
```

**⚠️ Important Notes:**
- Only use firmware files specifically designed for your device
- Ensure a stable power supply during update
- Do not disconnect device during firmware update
- Keep backup firmware files for recovery

### Information Commands

```bash
busytag-cli version                           # Show CLI version
busytag-cli --version                         # Alternative version command
busytag-cli -v                                # Short version command
busytag-cli help                              # Show help information
busytag-cli --help                            # Alternative help command
busytag-cli -h                                # Short help command
```

## 🔧 Advanced Usage

### Interactive Mode

Launch interactive mode for guided operations:

```bash
busytag-cli
```

**Interactive Mode Features:**
- 🔄 Automatic device discovery every 3 seconds
- 📊 Real-time progress indicators with detailed statistics
- 🛡️ Safe operation confirmations for destructive actions
- 📈 Detailed storage analysis and file management
- 🎛️ Guided workflows for complex operations
- 📋 Device health monitoring and diagnostics

### Environment Configuration

Set environment variables for default behavior:

```bash
# Windows
set BUSYTAG_DEFAULT_PORT=COM3
set BUSYTAG_TIMEOUT=5000
set BUSYTAG_SCAN_INTERVAL=3000
set BUSYTAG_DEBUG=1

# macOS/Linux  
export BUSYTAG_DEFAULT_PORT="/dev/cu.usbserial-xyz"  # macOS
# export BUSYTAG_DEFAULT_PORT="/dev/ttyUSB0"         # Linux
export BUSYTAG_TIMEOUT=5000
export BUSYTAG_SCAN_INTERVAL=3000
export BUSYTAG_DEBUG=1
```

## 🛠️ Development

### Building from Source

#### Prerequisites
- .NET 8.0 SDK or later
- Git

#### Build Commands
```bash
# Clone repository
git clone https://github.com/busy-tag/busytag-cli.git
cd busytag-cli/BusyTag.CLI

# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run tests
dotnet test

# Run the application
dotnet run -- <arguments>
```

#### Publishing

Create platform-specific executables:

```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# macOS x64 (Intel)
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

# macOS ARM64 (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

#### Development Configuration

Create a `appsettings.Development.json` file for development settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "BusyTag": "Trace"
    }
  },
  "BusyTag": {
    "DefaultTimeout": 10000,
    "ScanInterval": 2000,
    "MaxRetries": 3
  }
}
```

### Project Dependencies

The project uses the following key dependencies:

- **BusyTag.Lib** (v0.2.3+) - Core device communication library
- **.NET 8.0** - Runtime framework
- **System.IO.Ports** - Serial port communication
- **Microsoft.Extensions.Logging** - Logging framework

### Error Codes Reference

| Exit Code | Description | Common Causes |
|-----------|-------------|---------------|
| 0 | Success | Operation completed successfully |
| 1 | General error | Invalid arguments, connection failure |
| 2 | Device not found | No device detected, wrong port |
| 3 | Permission denied | Insufficient permissions, driver issues |
| 4 | File operation failed | File not found, storage full |
| 5 | Communication timeout | Device not responding, cable issues |
| 6 | Invalid format | Unsupported file format |
| 7 | Device busy | Another operation in progress |

## 📚 API Reference

### Core Classes

#### BusyTagDevice
Main device interface class providing all device operations.

**Key Methods:**
- `Connect()` - Establish device connection
- `Disconnect()` - Close device connection
- `SendRgbColorAsync(r, g, b, ledBits)` - Set RGB color
- `SetSolidColorAsync(colorName, brightness, ledBits)` - Set named color
- `SendNewFile(filePath)` - Upload file to device
- `GetFileListAsync()` - Retrieve device file list
- `DeleteFile(filename)` - Delete file from device
- `GetFileAsync(filename)` - Download file from device
- `ShowPictureAsync(filename)` - Display image file
- `FormatDiskAsync()` - Format device storage
- `RestartDeviceAsync()` - Restart device

#### BusyTagManager
Device discovery and management class.

**Key Methods:**
- `FindBusyTagDevice()` - Scan for available devices
- `StartPeriodicDeviceSearch(interval)` - Auto-discovery
- `StopPeriodicDeviceSearch()` - Stop auto-discovery

### Configuration Options

The application supports various configuration options through environment variables and command-line arguments:

```bash
# Environment Variables
BUSYTAG_DEFAULT_PORT       # Default device port
BUSYTAG_TIMEOUT           # Connection timeout (ms)
BUSYTAG_SCAN_INTERVAL     # Device scan interval (ms)
BUSYTAG_DEBUG             # Enable debug logging (1/0)
BUSYTAG_MAX_RETRIES       # Maximum retry attempts
BUSYTAG_LOG_LEVEL         # Logging level (Debug/Info/Warning/Error)
```

## 📄 License

This project is licensed under the MIT License. See the main repository LICENSE file for details.

## 📈 Version History

### Latest Versions
- **v0.3.4+** - Homebrew support, automated releases, native executables
- **v0.2.0** - Command improvements and bug fixes
- **v0.1.0** - Initial release with core functionality

## 🎯 Performance Optimization

### Best Practices

#### File Operations
```bash
# Check available space before large uploads
busytag-cli storage COM3

# Use appropriate file formats
# PNG: Best for static images, smaller file sizes
# GIF: Required for animations, larger file sizes

# Optimize images before upload
# Use tools like pngquant, imageoptim, or tinypng
```

#### Batch Operations
```bash
# Group related operations to reduce connection overhead
# Good:
busytag-cli upload COM3 file1.png
busytag-cli upload COM3 file2.png
busytag-cli show COM3 file1.png

# Better: Use scripts that maintain connection state
# (Interactive mode handles this automatically)
```

#### Connection Management
```bash
# For automation, use environment variables to reduce setup time
export BUSYTAG_DEFAULT_PORT="COM3"
export BUSYTAG_TIMEOUT="10000"

# Then commands don't need port specification
busytag-cli color red 75
busytag-cli upload image.png
```

### Memory and Resource Usage

The CLI application is designed to be lightweight:

- **Memory footprint:** ~10-50MB depending on operation
- **CPU usage:** Minimal during normal operations
- **Network usage:** None (local device communication only)
- **Storage:** Self-contained executable ~20-60MB depending on platform

## 🔐 Security Considerations

### Device Access
- CLI requires direct access to serial/USB ports
- On Linux, user must be in `dialout` group
- On macOS/Windows, admin privileges may be required for driver installation

### File Handling
- Always validate file paths and names before upload
- Be cautious with firmware files - only use trusted sources
- Consider scanning uploaded files for malware in shared environments

### Automation Security
```bash
# Use absolute paths in automation scripts
busytag-cli upload COM3 "/full/path/to/file.png"

# Validate inputs in scripts
if [[ -f "$UPLOAD_FILE" ]]; then
    busytag-cli upload "$PORT" "$UPLOAD_FILE"
else
    echo "Error: File not found: $UPLOAD_FILE"
    exit 1
fi

# Set appropriate file permissions
chmod 600 /path/to/automation-script.sh
```
## 🌐 Internationalization

The CLI currently supports English output. For international users:

### Date and Time Formats
- Uses ISO 8601 format for timestamps
- UTC timezone for logs and operations

### Number Formats
- Uses decimal points for floating-point numbers
- Byte sizes in binary format (1024-based)

### Error Messages
- All error messages are in English
- Error codes are language-independent

## 📊 Monitoring and Observability

### Logging Configuration

Configure logging levels and outputs:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "BusyTag.CLI": "Debug",
      "BusyTag.Lib": "Information"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "/var/log/busytag-cli.log",
      "MaxFileSize": "10MB",
      "MaxRollingFiles": 5
    }
  }
}
```

### Metrics and Analytics

For automation and monitoring, the CLI provides:

- **Exit codes** for programmatic success/failure detection
- **Structured output** options for parsing
- **Progress indicators** for long-running operations
- **Timing information** for performance monitoring

```bash
# Example: Monitoring script performance
start_time=$(date +%s)
busytag-cli upload COM3 large-file.png
end_time=$(date +%s)
duration=$((end_time - start_time))
echo "Upload took ${duration} seconds"
```

## 🔄 Migration and Upgrade Guide

### Upgrading from v0.1.x to v0.2.x+

1. **Update installation:**
   ```bash
   # For Homebrew users
   brew update && brew upgrade busytag-cli
   
   # For .NET tool users
   dotnet tool update -g BusyTag.CLI
   ```

2. **Check for breaking changes:**
    - Command syntax remains the same
    - New features available (check changelog)
    - Environment variable names unchanged

3. **Update automation scripts:**
    - Test all automation scripts with new version
    - Update any hardcoded version checks
    - Take advantage of new features

### Backup and Recovery

Before major updates:

```bash
# Backup current configuration
cp ~/.busytag/config.json ~/.busytag/config.json.backup

# Backup automation scripts
tar -czf busytag-scripts-backup.tar.gz /path/to/scripts/

# Test new version in isolation
busytag-cli --version
busytag-cli help
```

## 📞 Support and Community

### Getting Help

1. **Documentation**: Check this comprehensive guide first
2. **GitHub Issues**: [Create an issue](https://github.com/busy-tag/busytag-cli/issues) for bugs or feature requests
3. **Discussions**: Use GitHub Discussions for questions and community support

### Reporting Bugs

When reporting bugs, include:

```bash
# System information
busytag-cli --version
uname -a                    # Linux/macOS
systeminfo                 # Windows

# Device information
busytag-cli scan
busytag-cli info <port>

# Error logs (if available)
cat /var/log/busytag-cli.log

# Steps to reproduce
# Expected vs actual behavior
# Screenshots or terminal output
```

---

**Made with ❤️ by BUSY TAG SIA**

For questions or support, please [open an issue](https://github.com/busy-tag/busytag-cli/issues) or check our [documentation](https://busytag.com/docs).