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
- **🎨 Display Control** - Set colors, LED patterns with count control, brightness, and display images
- **✨ Advanced Pattern Control** - 24 built-in patterns with customizable repeat counts (1-254, 255=infinite)
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

# Set LED pattern
busytag-cli pattern COM3 "Police 1" 3   # Police pattern, 3 times

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
| `pattern <port> <name/number> [count]` | Set LED pattern | `busytag-cli pattern COM3 "Police 1" 5` |
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

## ✨ LED Pattern Examples

```bash
# Pattern by name (default: play once)
busytag-cli pattern COM3 "Police 1"

# Pattern by number (see available patterns with: busytag-cli pattern COM3)
busytag-cli pattern COM3 1

# Pattern with custom count
busytag-cli pattern COM3 "Red flashes" 5     # Play 5 times
busytag-cli pattern COM3 "White pulses" 10   # Play 10 times
busytag-cli pattern COM3 1 255               # Pattern #1, infinite loop

# Count options:
# 1-254: Play pattern that many times
# 255:   Play infinitely (until new command)

# Available patterns (24 total):
# 1. Default           2. Police 1         3. Police 2         4. Red flashes
# 5. Green flashes     6. Blue flashes     7. Yellow flashes   8. Cyan flashes
# 9. Magenta flashes   10. White flashes   11. Red running     12. Green running
# 13. Blue running     14. Yellow running  15. Cyan running    16. Magenta running
# 17. White running    18. Red pulses      19. Green pulses    20. Blue pulses
# 21. Yellow pulses    22. Cyan pulses     23. Magenta pulses  24. White pulses
```

## 🌟 Use Cases

### **Development & Debugging**
- Visual build status indicators with custom patterns
- System health monitoring with attention-grabbing alerts
- Test result notifications with pattern count control

### **Creative Projects**
- Photo frame displays with dynamic LED effects
- Art installations with programmable light patterns
- Interactive exhibits with timed pattern sequences

### **Home Automation**
- Weather displays with pattern-based alerts
- Smart home status with color and pattern coding
- Notification system with pattern repetition for urgency

### **Office & Business**
- Meeting room status with pulsing availability indicators
- Team availability with police patterns for urgent calls
- Project status boards with running light progress indicators

## 🤖 Automation Examples

### Simple Status Script (Bash)
```bash
#!/bin/bash
PORT="/dev/cu.usbserial-xyz"  # Adjust for your device

# Green for success, red pattern for failure
if make test; then
    busytag-cli color $PORT green 75
else
    busytag-cli pattern $PORT "Red pulses" 5    # Red pulses pattern 5 times for attention
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

### Build Status Monitor (PowerShell)
```powershell
# Enhanced build monitor with pattern alerts
$PORT = "COM3"  # Adjust for your device

function Monitor-BuildStatus {
    while ($true) {
        $buildResult = & dotnet build --no-restore

        if ($LASTEXITCODE -eq 0) {
            # Success: Green with brief celebration pattern
            & busytag-cli pattern $PORT "Green flashes" 3
            Start-Sleep -Seconds 2
            & busytag-cli color $PORT green 75
        } else {
            # Failure: Urgent red pattern then solid red
            & busytag-cli pattern $PORT "Red pulses" 10
            & busytag-cli color $PORT red 100
        }

        Start-Sleep -Seconds 300  # Check every 5 minutes
    }
}

Monitor-BuildStatus
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

## 📈 Version History

### v0.5.1 (Latest)
- **🎨 Enhanced LED Pattern Control**: Added count parameter for pattern repetition (1-254 times, 255 for infinite)
- **🔢 Pattern Selection by Number**: Patterns can now be activated by number (1-24) or by name
- **📋 Complete Pattern Documentation**: All 24 available patterns listed with numbers and names
- **✨ Improved Pattern Syntax**: Better command-line pattern handling with proper quoting support
- **📖 Enhanced Documentation**: Updated examples, automation scripts, and help text with real pattern names

### v0.5.0
- Updated library versions
- Enhanced device communication stability
- Improved error handling

### v0.4.1
- Project cleanup and code optimization
- Minor bug fixes

### v0.4.0
- Updated library versions
- Performance improvements
- Better compatibility across platforms

### v0.3.6
- General version update
- Stability improvements

### v0.3.5
- Fixed Homebrew formula path
- Enhanced macOS installation process

### v0.3.4 & v0.3.3
- Minor updates and fixes
- Code refinements

### v0.3.2 & v0.3.1
- Project file reorganization
- Debug improvements

### v0.3.0
- Added Homebrew automation
- Improved deployment process

### v0.2.0
- Major README.md improvements
- Enhanced documentation

### v0.1.0
- Initial public release
- Core CLI functionality
- Device discovery and basic operations

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by BUSY TAG SIA**

For detailed documentation and advanced usage, see the [BusyTag.CLI directory](BusyTag.CLI/).

For support, please [open an issue](https://github.com/busy-tag/busytag-cli/issues) on GitHub.