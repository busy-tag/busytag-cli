# BusyTag CLI - Device Manager Command Line Interface

[![.NET Tool](https://img.shields.io/nuget/v/BusyTag.CLI?label=.NET%20Tool&color=blue)](https://www.nuget.org/packages/BusyTag.CLI)
[![Homebrew](https://img.shields.io/badge/Homebrew-macOS-orange.svg)](https://github.com/busy-tag/homebrew-busytag)
[![.NET](https://img.shields.io/badge/.NET-8.0+-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg)](https://github.com/dotnet/core)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/BusyTag.CLI?color=green)](https://www.nuget.org/packages/BusyTag.CLI)

A comprehensive command-line interface for managing BusyTag devices. Control your BusyTag displays, upload files, manage storage, update firmware, and more - all from the command line or through an interactive interface.

## ✨ Features

- **🔍 Device Discovery & Connection** - Automatically find and connect to BusyTag devices
- **🎨 Display Control** - Set colors, patterns, brightness, and display images
- **📁 File Management** - Upload, download, delete, and list files on device storage
- **💾 Storage Operations** - Monitor storage usage and format device storage
- **🔧 Firmware Updates** - Upload and install firmware updates with progress tracking
- **⚡ Interactive & Command-Line Modes** - Use as needed for automation or manual operation
- **📊 Real-time Monitoring** - Progress bars, status updates, and event tracking
- **🌍 Cross-Platform** - Works on Windows, macOS, and Linux

## 🚀 Quick Installation

### macOS (Recommended)
```bash
# Add the BusyTag tap and install
brew tap busy-tag/busytag
brew install busytag-cli
```

### Cross-Platform (.NET)
```bash
# Requires .NET 8.0+ runtime
dotnet tool install -g BusyTag.CLI
```

### Other Options
- **From Source**: Clone and build locally
- **Binary Releases**: Download from [Releases](https://github.com/busy-tag/busytag-cli/releases) page

## 🎯 Quick Start

### 1. Find Your Device
```bash
busytag-cli scan
# Output: Found 1 device(s): COM3 (Windows) or /dev/cu.usbserial-xyz (macOS)
```

### 2. Check Device Status
```bash
busytag-cli info COM3
```

### 3. Control Your Display
```bash
# Set color
busytag-cli color COM3 blue 75

# Upload and show image
busytag-cli upload COM3 "my-image.png"
busytag-cli show COM3 "my-image.png"

# Check storage
busytag-cli storage COM3
```

### 4. Interactive Mode
```bash
busytag-cli
# Launches interactive menu for guided operations
```

## 📖 Essential Commands

| Command | Description | Example |
|---------|-------------|---------|
| `scan` | Find connected devices | `busytag-cli scan` |
| `info <port>` | Show device information | `busytag-cli info COM3` |
| `color <port> <color>` | Set display color | `busytag-cli color COM3 red` |
| `upload <port> <file>` | Upload file to device | `busytag-cli upload COM3 image.png` |
| `files <port>` | List device files | `busytag-cli files COM3` |
| `storage <port>` | Show storage information | `busytag-cli storage COM3` |
| `help` | Show all commands | `busytag-cli help` |

## 🎨 Color Examples

```bash
# Named colors
busytag-cli color COM3 red
busytag-cli color COM3 blue 50    # 50% brightness

# Hex colors
busytag-cli color COM3 FF0000     # Red
busytag-cli color COM3 #00FF00    # Green

# RGB values
busytag-cli color COM3 255,0,0    # Red
busytag-cli color COM3 0,255,128  # Teal
```

## 🌟 Use Cases

### **Development & Debugging**
- Visual build status indicators
- System health monitoring
- Test result notifications

### **Creative Projects**
- Photo frame displays
- Art installations
- Interactive exhibits

### **Home Automation**
- Weather displays
- Smart home status
- Notification system

### **Office & Business**
- Meeting room status
- Team availability
- Project status boards

## 🤖 Automation Examples

### Simple Status Script (Bash)
```bash
#!/bin/bash
PORT="/dev/cu.usbserial-xyz"  # Adjust for your device

# Green for success, red for failure
if make test; then
    busytag-cli color $PORT green 75
else
    busytag-cli color $PORT red 100
fi
```

### Weather Display (Python)
```python
import subprocess
import requests

def update_weather_display():
    # Get weather data (example)
    weather = requests.get("http://api.weather.com/...").json()
    
    colors = {"sunny": "yellow", "cloudy": "white", "rainy": "blue"}
    color = colors.get(weather["condition"], "white")
    
    subprocess.run(["busytag-cli", "color", "COM3", color])

update_weather_display()
```

## 🔧 Platform-Specific Notes

### **macOS**
- Device ports typically: `/dev/tty.usbmodem*`
- Homebrew installation recommended
- No additional drivers are usually needed

### **Windows**
- Device ports typically: `COM1`, `COM2`, etc.
- May require USB-to-Serial drivers
- Use Device Manager to find ports

### **Linux**
- Device ports typically: `/dev/ttyUSB0`, `/dev/ttyACM0`
- May need to add user to `dialout` group
- Use `lsusb` to find connected devices

## 🔗 Links

- **[NuGet Package](https://www.nuget.org/packages/BusyTag.CLI)** - .NET tool package
- **[Homebrew Tap](https://github.com/busy-tag/homebrew-busytag)** - macOS installation
- **[Issues & Support](https://github.com/busy-tag/busytag-cli/issues)** - Bug reports and feature requests
- **[Releases](https://github.com/busy-tag/busytag-cli/releases)** - Download binary releases

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by BUSY TAG SIA**

For detailed documentation and advanced usage, see the [BusyTag.CLI directory](BusyTag.CLI/).

For support, please [open an issue](https://github.com/busy-tag/busytag-cli/issues) on GitHub.