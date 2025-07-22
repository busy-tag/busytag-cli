# BusyTag CLI - Device Manager Command Line Interface

[![.NET Tool](https://img.shields.io/nuget/v/BusyTag.CLI?label=.NET%20Tool&color=blue)](https://www.nuget.org/packages/BusyTag.CLI)
[![.NET](https://img.shields.io/badge/.NET-8.0+-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)](https://github.com/dotnet/core)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/BusyTag.CLI?color=green)](https://www.nuget.org/packages/BusyTag.CLI)

A comprehensive command-line interface for managing BusyTag devices. Control your BusyTag displays, upload files, manage storage, update firmware, and more - all from the command line or through an interactive interface.

## 📋 Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage Modes](#-usage-modes)
- [Commands Reference](#-commands-reference)
- [Examples & Use Cases](#-examples--use-cases)
- [File Format Support](#-file-format-support)
- [Advanced Usage](#-advanced-usage)
- [Configuration](#-configuration)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)

## ✨ Features

- **🔍 Device Discovery & Connection** - Automatically find and connect to BusyTag devices
- **🎨 Display Control** - Set colors, patterns, brightness, and display images
- **📁 File Management** - Upload, download, delete, and list files on device storage
- **💾 Storage Operations** - Monitor storage usage and format device storage
- **🔧 Firmware Updates** - Upload and install firmware updates with progress tracking
- **⚡ Interactive & Command-Line Modes** - Use as needed for automation or manual operation
- **📊 Real-time Monitoring** - Progress bars, status updates, and event tracking
- **🌍 Cross-Platform** - Works on Windows, macOS, and Linux

## 🚀 Installation

### Prerequisites
- **.NET 8.0 Runtime** or later
- **BusyTag device** with USB/Serial connection
- **Operating System**: Windows, macOS, or Linux

### Method 1: .NET Global Tool (Recommended)
Install as a global tool using the .NET CLI:

```bash
# Install the latest version
dotnet tool install -g BusyTag.CLI

# Verify installation
busytag-cli --version

# Update to latest version
dotnet tool update -g BusyTag.CLI

# Uninstall if needed
dotnet tool uninstall -g BusyTag.CLI
```

### Method 2: From Source
For development or building from source:

```bash
# Clone the repository
git clone https://github.com/busy-tag/busytag-cli.git
cd busytag-cli

# Build and run
dotnet build -c Release
dotnet run -- --help

# Or build self-contained executable
dotnet publish -c Release --self-contained -r win-x64    # Windows
dotnet publish -c Release --self-contained -r osx-x64    # macOS
dotnet publish -c Release --self-contained -r linux-x64  # Linux
```

### Method 3: Download Binary Release
Download pre-built binaries from the [Releases](https://github.com/busy-tag/busytag-cli/releases) page.

## 🎯 Quick Start

### 1. Install the Tool
```bash
dotnet tool install -g BusyTag.CLI
```

### 2. Find Your Device
```bash
busytag-cli scan
# Output: Found 1 device(s): COM3
```

### 3. Connect and Check Status
```bash
busytag-cli info COM3
```

### 4. Set Display Color
```bash
busytag-cli color COM3 blue 75
```

### 5. Upload and Display an Image
```bash
busytag-cli upload COM3 "my-image.png"
busytag-cli show COM3 "my-image.png"
```

## 🖥️ Usage Modes

### Interactive Mode
Run without arguments to enter an interactive menu-driven interface:
```bash
busytag-cli
```

**Features:**
- 🔄 Automatic device discovery every 3 seconds
- 📊 Real-time progress indicators
- 🛡️ Safe operation confirmations
- 📈 Detailed storage analysis
- 🎛️ Guided workflows for complex operations

### Command-Line Mode
Execute specific commands for automation and scripting:
```bash
busytag-cli <command> [arguments]
```

Perfect for:
- 🤖 Automation scripts
- 📋 Batch operations
- 🔗 CI/CD integration
- ⏰ Scheduled tasks

## 📖 Commands Reference

### Device Management

| Command | Description | Example |
|---------|-------------|---------|
| `scan` / `list` | Discover connected devices | `busytag-cli scan` |
| `connect <port>` | Connect to specific device | `busytag-cli connect COM3` |
| `info <port>` | Show device information | `busytag-cli info COM3` |
| `restart <port>` | Restart device remotely | `busytag-cli restart COM3` |

### Display Control

#### Set Colors
```bash
busytag-cli color <port> <color> [brightness] [led_bits]
```

**Color Formats:**
- **Named:** `red`, `green`, `blue`, `yellow`, `cyan`, `magenta`, `white`, `off`
- **Hex:** `FF0000`, `#FF0000`
- **RGB:** `255,0,0`

**Examples:**
```bash
busytag-cli color COM3 red                    # Full red
busytag-cli color COM3 red 75                 # Red at 75% brightness
busytag-cli color COM3 FF0000 50 127          # Hex red, 50% brightness, all LEDs
busytag-cli color COM3 255,128,0              # Orange using RGB
```

#### Other Display Commands
```bash
busytag-cli brightness <port> <level>         # Set brightness (0-100)
busytag-cli pattern <port> <pattern_name>     # Apply LED pattern
busytag-cli show <port> <filename>            # Display image file
```

### File Management

| Command | Description | Example |
|---------|-------------|---------|
| `upload <port> <file>` | Upload file to device | `busytag-cli upload COM3 "photo.png"` |
| `download <port> <file> <dest>` | Download from device | `busytag-cli download COM3 photo.png ./` |
| `delete <port> <file>` | Delete file from device | `busytag-cli delete COM3 old.png` |
| `files <port>` / `ls <port>` | List device files | `busytag-cli files COM3` |

### Storage Operations

```bash
busytag-cli storage <port>                    # Show storage info
busytag-cli format <port> --force             # Format storage (⚠️ DESTRUCTIVE)
```

### Firmware Management

```bash
busytag-cli firmware <port> <firmware.bin>    # Upload firmware (⚠️ CAUTION)
```

### Information

```bash
busytag-cli version                           # Show version
busytag-cli help                              # Show help
```

## 🌟 Examples & Use Cases

### Basic Device Setup
```bash
# Quick device check and setup
busytag-cli scan
busytag-cli info COM3
busytag-cli color COM3 green 80
```

### Image Management Workflow
```bash
# Check storage space
busytag-cli storage COM3

# Upload multiple images
busytag-cli upload COM3 "logo.png"
busytag-cli upload COM3 "background.gif"

# List all files
busytag-cli files COM3

# Display specific image
busytag-cli show COM3 logo.png

# Clean up old files
busytag-cli delete COM3 old_logo.png
```

### Automation Scripts

#### PowerShell: Batch Upload
```powershell
$port = "COM3"
$imageDir = "C:\DisplayImages\"

Get-ChildItem $imageDir -Include "*.png","*.gif" | ForEach-Object {
    & busytag-cli upload $port $_.FullName
    Write-Host "✅ Uploaded: $($_.Name)"
}

# Set first image as display
$firstImage = (Get-ChildItem $imageDir -Include "*.png","*.gif")[0]
& busytag-cli show $port $firstImage.Name
```

#### Bash: System Status Display
```bash
#!/bin/bash
PORT="/dev/ttyUSB0"

# Get CPU usage
CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1)

# Set color based on CPU usage
if (( $(echo "$CPU_USAGE < 30" | bc -l) )); then
    busytag-cli color $PORT green 50
elif (( $(echo "$CPU_USAGE < 70" | bc -l) )); then
    busytag-cli color $PORT yellow 75
else
    busytag-cli color $PORT red 100
fi
```

#### Python: Weather Display Integration
```python
import subprocess
import requests
import time

def update_weather_display(port="COM3", api_key="your_api_key"):
    """Update BusyTag display based on weather conditions"""
    
    # Get weather data
    response = requests.get(f"http://api.openweathermap.org/data/2.5/weather?q=YourCity&appid={api_key}")
    weather = response.json()
    
    # Map weather to colors
    weather_colors = {
        "clear": "yellow",
        "clouds": "white", 
        "rain": "blue",
        "snow": "cyan",
        "thunderstorm": "purple"
    }
    
    condition = weather["weather"][0]["main"].lower()
    color = weather_colors.get(condition, "white")
    
    # Update display
    subprocess.run(["busytag-cli", "color", port, color, "75"])
    print(f"🌤️  Display updated for {condition} weather")

# Run every hour
while True:
    update_weather_display()
    time.sleep(3600)
```

#### Docker Integration
```dockerfile
# Dockerfile for automated BusyTag control
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Install BusyTag CLI
RUN dotnet tool install -g BusyTag.CLI

# Add to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Your automation script
COPY automation-script.sh /app/
RUN chmod +x /app/automation-script.sh

CMD ["/app/automation-script.sh"]
```

### Maintenance Operations
```bash
# Backup all files
mkdir device_backup
busytag-cli files COM3 | grep -E "\.(png|gif)" | while read file; do
    busytag-cli download COM3 "$file" device_backup/
done

# System maintenance
busytag-cli storage COM3                      # Check storage
busytag-cli firmware COM3 latest.bin         # Update firmware
busytag-cli restart COM3                      # Restart device
```

## 📁 File Format Support

### Images
- **PNG** (recommended) - Best compression and quality
- **GIF** - Supports animation

### Firmware
- **Binary files (.bin)** - Device firmware updates
- **Maximum filename length:** 40 characters

### Best Practices
- ✅ Optimize PNG files for better storage efficiency
- ✅ Check available storage before large uploads
- ✅ Use descriptive but short filenames
- ✅ Keep firmware files backed up separately

## 🔧 Advanced Usage

### Environment Configuration
```bash
# Windows
set BUSYTAG_DEFAULT_PORT=COM3
set BUSYTAG_TIMEOUT=5000
set BUSYTAG_SCAN_INTERVAL=3000

# Linux/macOS  
export BUSYTAG_DEFAULT_PORT="/dev/ttyUSB0"
export BUSYTAG_TIMEOUT=5000
export BUSYTAG_SCAN_INTERVAL=3000
```

### CI/CD Integration Example
```yaml
# GitHub Actions example
name: Update BusyTag Display
on:
  push:
    branches: [main]
    
jobs:
  update-display:
    runs-on: self-hosted  # Requires runner with device access
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Install BusyTag CLI
        run: dotnet tool install -g BusyTag.CLI
      - name: Update display for successful build
        run: |
          busytag-cli scan
          busytag-cli color ${{ env.BUSYTAG_PORT }} green 75
          busytag-cli upload ${{ env.BUSYTAG_PORT }} assets/success-logo.png
          busytag-cli show ${{ env.BUSYTAG_PORT }} success-logo.png
```

### Health Monitoring Script
```bash
#!/bin/bash
# Device health monitoring

PORT="COM3"
LOG="/var/log/busytag.log"

check_device_health() {
    echo "$(date): Checking device health..." >> $LOG
    
    if busytag-cli info $PORT > /dev/null 2>&1; then
        # Device online - check storage
        USAGE=$(busytag-cli storage $PORT | grep "Usage:" | awk '{print $2}' | tr -d '%')
        echo "$(date): Device online, storage: ${USAGE}%" >> $LOG
        
        # Alert if storage > 90%
        if [ "$USAGE" -gt 90 ]; then
            echo "$(date): ⚠️  WARNING - Storage nearly full!" >> $LOG
            busytag-cli color $PORT red 100  # Visual alert
        fi
    else
        echo "$(date): ❌ ERROR - Device offline" >> $LOG
    fi
}

# Run every 5 minutes
while true; do
    check_device_health
    sleep 300
done
```

## ⚙️ Configuration

### Performance Tuning
- **Connection timeout:** Increase for slow connections
- **Scan interval:** Adjust for device discovery frequency
- **Batch size:** Group operations to reduce overhead

### Security Settings
- **Firmware verification:** Always verify firmware sources
- **File scanning:** Scan uploads for malware
- **Access control:** Limit device access in multi-user environments

## 🔧 Troubleshooting

### Common Issues

#### 🔍 Device Not Found
```bash
# Check all possible ports
busytag-cli scan

# Test specific ports
busytag-cli info COM1
busytag-cli info COM2
busytag-cli info /dev/ttyUSB0
busytag-cli info /dev/ttyACM0
```

**Solutions:**
- ✅ Check USB cable connection
- ✅ Verify device drivers are installed
- ✅ Try different USB ports
- ✅ Check device manager (Windows) or `lsusb` (Linux)

#### ⏱️ Connection Timeout
```bash
# Check if device is responding
busytag-cli info <port>

# Try restarting device
busytag-cli restart <port>
```

**Solutions:**
- ✅ Increase timeout in environment variables
- ✅ Check for device conflicts
- ✅ Restart device and retry
- ✅ Update device drivers

#### 💾 Upload Failures
```bash
# Check available storage first
busytag-cli storage <port>

# Verify file exists and check size
ls -la <filename>
```

**Solutions:**
- ✅ Free up storage space
- ✅ Verify file path and permissions
- ✅ Check filename length (max 40 chars)
- ✅ Ensure stable connection during upload

#### 🔧 Firmware Update Issues
```bash
# Verify file format
file firmware.bin

# Check device info before update
busytag-cli info <port>
```

**Solutions:**
- ✅ Use only compatible .bin files
- ✅ Ensure stable power supply
- ✅ Don't disconnect during update
- ✅ Have recovery firmware ready

#### 🛠️ Tool Installation Issues
```bash
# Check if .NET is installed
dotnet --version

# Clear tool cache and reinstall
dotnet tool uninstall -g BusyTag.CLI
dotnet nuget locals all --clear
dotnet tool install -g BusyTag.CLI

# Check tool path
echo $PATH | grep -o '[^:]*\.dotnet[^:]*'
```

### Error Codes

| Error | Description | Solution |
|-------|-------------|----------|
| **Connection Error** | Device not responding | Check connection and drivers |
| **File Not Found** | Missing file | Verify file path and existence |
| **Storage Full** | Insufficient space | Delete files or format storage |
| **Invalid Format** | Unsupported file | Use PNG/GIF for images, .bin for firmware |
| **Timeout** | Operation took too long | Increase timeout or check connection |
| **Device Busy** | Another operation in progress | Wait or restart device |
| **Tool Not Found** | CLI not in PATH | Reinstall tool or check PATH |

### Debug Mode
```bash
# Enable verbose logging (if supported)
BUSYTAG_DEBUG=1 busytag-cli info COM3

# Check system logs
# Windows: Event Viewer
# Linux: journalctl -u busytag
# macOS: Console.app
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📈 Changelog

### Version 1.0.0
- 🎉 Initial stable release
- 🔌 Full device connection and control
- 📁 Complete file management functionality
- 🎨 Advanced color and brightness control
- 🔧 Firmware update capabilities
- 📊 Interactive mode with progress tracking
- 🌍 Cross-platform support (Windows, macOS, Linux)

### Version 0.1.0
- 🎉 Initial beta release
- 🔌 Basic device connection and control
- 📁 File upload/download functionality
- 🎨 Color and brightness control

## 🔗 Links

- **GitHub Repository**: [https://github.com/busy-tag/busytag-cli](https://github.com/busy-tag/busytag-cli)
- **NuGet Package**: [https://www.nuget.org/packages/BusyTag.CLI](https://www.nuget.org/packages/BusyTag.CLI)
- **Issues & Support**: [https://github.com/busy-tag/busytag-cli/issues](https://github.com/busy-tag/busytag-cli/issues)
- **BusyTag Hardware**: [Contact BUSY TAG SIA for device information]

## 🎯 Roadmap

- 🔄 **Auto-update mechanism** for CLI tool
- 🎨 **Custom pattern editor** in interactive mode
- 📱 **Mobile device support** via Bluetooth
- 🌐 **Web dashboard** for remote management
- 🔌 **Plugin system** for extended functionality
- 📊 **Analytics and monitoring** dashboard

---

**Made with ❤️ by BUSY TAG SIA**

For support, please check the [troubleshooting section](#-troubleshooting) or [open an issue](https://github.com/busy-tag/busytag-cli/issues) on GitHub.