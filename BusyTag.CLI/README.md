# BusyTag CLI - Device Manager Command Line Interface

A comprehensive command-line interface for managing BusyTag devices. Control your BusyTag displays, upload files, manage storage, update firmware, and more - all from the command line or through an interactive interface.

## Features

- **Device Discovery & Connection** - Automatically find and connect to BusyTag devices
- **Display Control** - Set colors, patterns, brightness, and display images
- **File Management** - Upload, download, delete, and list files on device storage
- **Storage Operations** - Monitor storage usage and format device storage
- **Firmware Updates** - Upload and install firmware updates
- **Interactive & Command-Line Modes** - Use as needed for automation or manual operation

## Installation

### Prerequisites
- .NET 9.0 or later
- Windows, macOS, or Linux

### From Source
```bash
git clone [repository-url]
cd BusyTag.CLI
dotnet build -c Release
dotnet run
```

### From Package
```bash
dotnet add package BusyTag.Lib
```

## Usage

### Interactive Mode
Run without arguments to enter interactive mode with a menu-driven interface:
```bash
BusyTag
```

### Command-Line Mode
Use specific commands for automation and scripting:

## Commands Reference

### Device Management

#### Scan for Devices
```bash
BusyTag scan
BusyTag list
```
Discovers all connected BusyTag devices and displays their port names.

#### Connect to Device
```bash
BusyTag connect <port>
```
**Example:**
```bash
BusyTag connect COM3
BusyTag connect /dev/ttyUSB0
```

#### Device Information
```bash
BusyTag info <port>
```
Shows device name, firmware version, storage info, and current status.

#### Restart Device
```bash
BusyTag restart <port>
```
Remotely restarts the BusyTag device.

### Display Control

#### Set Solid Color
```bash
BusyTag color <port> <color> [brightness] [led_bits]
```

**Color Formats:**
- **Named colors:** `red`, `green`, `blue`, `yellow`, `cyan`, `magenta`, `white`, `off`
- **Hex colors:** `FF0000`, `#FF0000` (red)
- **RGB values:** `255,0,0` (red)

**Parameters:**
- `brightness`: 0-100 (default: 100)
- `led_bits`: 1-127 (default: 127, controls which LEDs are affected)

**Examples:**
```bash
BusyTag color COM3 red
BusyTag color COM3 red 75
BusyTag color COM3 FF0000 50 127
BusyTag color COM3 255,128,0
BusyTag color COM3 #00FF00 80
```

#### Set Brightness
```bash
BusyTag brightness <port> <level>
```
**Example:**
```bash
BusyTag brightness COM3 75
```

#### Set Pattern
```bash
BusyTag pattern <port> <pattern_name>
```
Apply predefined LED patterns to the display.

#### Display Image
```bash
BusyTag show <port> <filename>
```
**Example:**
```bash
BusyTag show COM3 photo.png
```

### File Management

#### Upload File
```bash
BusyTag upload <port> <file_path>
```
**Examples:**
```bash
BusyTag upload COM3 photo.png
BusyTag upload COM3 "C:\Images\my photo.jpg"
BusyTag upload COM3 firmware_v2.1.bin
```

#### Download File
```bash
BusyTag download <port> <filename> <destination_path>
```
**Example:**
```bash
BusyTag download COM3 photo.png "C:\Downloads\"
```

#### List Files
```bash
BusyTag files <port>
BusyTag ls <port>
```
Shows all files on the device with sizes and indicates the currently displayed image.

#### Delete File
```bash
BusyTag delete <port> <filename>
BusyTag remove <port> <filename>
```
**Example:**
```bash
BusyTag delete COM3 old_image.png
```

### Storage Operations

#### Storage Information
```bash
BusyTag storage <port>
BusyTag space <port>
```
Displays total, used, and free storage space, plus usage percentage and file count.

#### Format Storage
```bash
BusyTag format <port> --force
```
**⚠️ WARNING:** This permanently deletes ALL data on the device!

**Example:**
```bash
BusyTag format COM3 --force
```

### Firmware Management

#### Upload Firmware
```bash
BusyTag firmware <port> <firmware_file.bin>
```
**⚠️ WARNING:** Firmware updates can potentially brick your device!

**Example:**
```bash
BusyTag firmware COM3 "firmware_v2.1.bin"
```

### Information Commands

#### Version Information
```bash
BusyTag version
BusyTag -v
BusyTag --version
```

#### Help
```bash
BusyTag help
BusyTag -h
BusyTag --help
```

## Examples & Use Cases

### Quick Device Setup
```bash
# Find devices
BusyTag scan

# Connect and check info
BusyTag info COM3

# Set a welcoming display
BusyTag color COM3 green 80
```

### Image Management Workflow
```bash
# Check current storage
BusyTag storage COM3

# Upload new images
BusyTag upload COM3 "vacation_photo.jpg"
BusyTag upload COM3 "company_logo.png"

# List all files
BusyTag files COM3

# Display specific image
BusyTag show COM3 vacation_photo.jpg

# Clean up old files
BusyTag delete COM3 old_logo.png
```

### Automation Scripts

#### Batch Upload Script (PowerShell)
```powershell
# Upload all PNG and GIF images from a directory
$port = "COM3"
Get-ChildItem "C:\Images\" -Include "*.png","*.gif" | ForEach-Object {
    & BusyTag upload $port $_.FullName
    Write-Host "Uploaded: $($_.Name)"
}
```

#### Status Check Script (Bash)
```bash
#!/bin/bash
PORT="/dev/ttyUSB0"

echo "=== BusyTag Status ==="
BusyTag info $PORT
echo ""
echo "=== Storage Usage ==="
BusyTag storage $PORT
echo ""
echo "=== Files ==="
BusyTag files $PORT
```

### Maintenance Operations
```bash
# Backup all image files
mkdir backup
BusyTag files COM3 | grep -E "\.(png|gif)" | while read file; do
    BusyTag download COM3 "$file" backup/
done

# Update firmware
BusyTag firmware COM3 latest_firmware.bin

# Clean storage if needed
BusyTag format COM3 --force
```

## Interactive Mode Features

When run without arguments, BusyTag CLI enters interactive mode with these features:

- **Automatic Device Discovery** - Continuously scans for devices every 3 seconds
- **Real-time Connection Status** - Shows current device and connection state
- **Progress Indicators** - Visual progress bars for uploads and operations
- **Detailed Storage Analysis** - Complete storage breakdown with file statistics
- **Safe Operations** - Confirmation prompts for destructive operations
- **Event Monitoring** - Real-time display of device events and status changes

### Interactive Menu Options

1. **Scan for devices** - Manual device discovery
2. **Connect to device** - Choose from available devices
3. **Show device info** - Complete device information
4. **Set solid color** - Interactive color selection with preview
5. **Set pattern** - Choose from available LED patterns
6. **Upload file** - File browser with drag-and-drop support
7. **List files** - Detailed file listing with sizes and types
8. **Set brightness** - Adjust display brightness
9. **Set current image** - Choose image to display from device files
10. **Download file** - Save files from device to computer
11. **Delete file** - Remove files with confirmation
12. **Scan device storage** - Detailed storage analysis
13. **Upload firmware** - Guided firmware update process
14. **Format disk** - Storage formatting with safety confirmations
15. **Restart device** - Remote device restart with reconnection
16. **Disconnect** - Clean device disconnection

## Configuration

### Environment Variables

- `BUSYTAG_DEFAULT_PORT` - Set default device port
- `BUSYTAG_TIMEOUT` - Set connection timeout (milliseconds)
- `BUSYTAG_SCAN_INTERVAL` - Device scan interval in interactive mode (milliseconds)

### Example Configuration
```bash
# Windows
set BUSYTAG_DEFAULT_PORT=COM3
set BUSYTAG_TIMEOUT=5000

# Linux/macOS
export BUSYTAG_DEFAULT_PORT="/dev/ttyUSB0"
export BUSYTAG_TIMEOUT=5000
```

## File Format Support

### Supported Image Formats
- PNG (recommended)
- GIF

### Firmware Files
- Binary files (.bin)
- Maximum filename length: 40 characters

### File Size Considerations
- Check available storage before uploading large files
- Use `BusyTag storage <port>` to check free space
- Consider PNG optimization for better storage efficiency
- GIF files support animation but may be larger

## Error Handling & Troubleshooting

### Common Issues

#### Device Not Found
```bash
# Check device connections
BusyTag scan

# Try different ports
BusyTag info COM1
BusyTag info COM2
BusyTag info COM3
```

#### Connection Timeout
```bash
# Check if device is busy
BusyTag info <port>

# Try restarting device
BusyTag restart <port>
```

#### Upload Failures
```bash
# Check available storage
BusyTag storage <port>

# Verify file exists and filename length
ls -la <filename>
```

#### Firmware Update Issues
```bash
# Ensure .bin file format
file firmware.bin

# Check device compatibility
BusyTag info <port>

# Try restart before firmware update
BusyTag restart <port>
```

### Error Codes

- **Connection Error** - Device not responding or port unavailable
- **File Not Found** - Specified file doesn't exist locally or on device
- **Storage Full** - Insufficient space for upload operation
- **Invalid Format** - Unsupported file format or invalid parameters
- **Timeout** - Operation took too long to complete
- **Device Busy** - Device is processing another operation

## Advanced Usage

### Scripting and Automation

#### Bulk Operations
```bash
# Upload multiple PNG/GIF files
for file in *.png *.gif; do
    BusyTag upload COM3 "$file"
    echo "Uploaded: $file"
done

# Set random colors
colors=("red" "green" "blue" "yellow" "cyan" "magenta")
for color in "${colors[@]}"; do
    BusyTag color COM3 "$color" 75
    sleep 2
done
```

#### Health Monitoring
```bash
#!/bin/bash
# Device health check script

PORT="COM3"
LOG_FILE="busytag_health.log"

echo "$(date): Starting health check" >> $LOG_FILE

# Check connectivity
if BusyTag info $PORT > /dev/null 2>&1; then
    echo "$(date): Device online" >> $LOG_FILE
    
    # Check storage
    STORAGE=$(BusyTag storage $PORT | grep "Usage:" | awk '{print $2}')
    echo "$(date): Storage usage: $STORAGE" >> $LOG_FILE
    
    # Alert if storage > 90%
    USAGE_NUM=${STORAGE%\%}
    if [ "$USAGE_NUM" -gt 90 ]; then
        echo "$(date): WARNING - Storage nearly full!" >> $LOG_FILE
    fi
else
    echo "$(date): ERROR - Device offline" >> $LOG_FILE
fi
```

#### Integration Examples

**PowerShell Integration:**
```powershell
# Function to update display based on system status
function Update-BusyTagStatus {
    param($Port = "COM3")
    
    $CpuUsage = (Get-Counter "\Processor(_Total)\% Processor Time").CounterSamples.CookedValue
    
    if ($CpuUsage -lt 30) {
        & BusyTag color $Port green 50
    } elseif ($CpuUsage -lt 70) {
        & BusyTag color $Port yellow 75
    } else {
        & BusyTag color $Port red 100
    }
}
```

**Python Integration:**
```python
import subprocess
import time
import psutil

def update_busytag_from_system(port="COM3"):
    """Update BusyTag display based on system metrics"""
    
    # Get system load
    cpu_percent = psutil.cpu_percent(interval=1)
    memory_percent = psutil.virtual_memory().percent
    
    # Determine color based on highest usage
    max_usage = max(cpu_percent, memory_percent)
    
    if max_usage < 30:
        color = "green"
        brightness = 40
    elif max_usage < 70:
        color = "yellow" 
        brightness = 70
    else:
        color = "red"
        brightness = 100
    
    # Update display
    subprocess.run([
        "BusyTag", "color", port, color, str(brightness)
    ])

# Run continuously
while True:
    update_busytag_from_system()
    time.sleep(30)  # Update every 30 seconds
```

## Security Considerations

### Safe Firmware Updates
- Only use firmware files from trusted sources
- Verify firmware compatibility with your device model
- Ensure stable power supply during updates
- Have recovery plan in case of update failure

### File Management
- Scan uploaded files for malware before transfer
- Be cautious with executable or script files
- Regularly backup important display content
- Monitor storage usage to prevent device issues

## Performance Tips

### Optimization
- Use compressed image formats (PNG with optimization)
- Batch operations when possible to reduce connection overhead
- Monitor device temperature during intensive operations
- Close unused connections to free device resources

### Best Practices
- Regular storage cleanup to maintain performance
- Use appropriate brightness levels to extend device life
- Update firmware regularly for bug fixes and improvements
- Document device configurations for easy recovery

## Support and Contributing

### Getting Help
- Check the troubleshooting section above
- Review device documentation
- Check for firmware updates
- Contact device manufacturer for hardware issues

### Bug Reports
When reporting issues, include:
- Device model and firmware version
- Operating system and .NET version
- Complete error messages
- Steps to reproduce the issue
- Output of `BusyTag info <port>`

### Contributing
- Fork the repository
- Create feature branches
- Include tests for new functionality
- Update documentation
- Submit pull requests

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Changelog

### Version 0.2.1
- Enhanced command-line argument handling
- Added comprehensive file management commands
- Improved error handling and user feedback
- Added progress indicators for long operations
- Enhanced storage analysis and reporting

### Version 0.2.0
- Added interactive mode
- Implemented firmware update capabilities
- Added storage formatting options
- Enhanced device discovery
- Improved connection management

### Version 0.1.0
- Initial release
- Basic device connection and control
- File upload/download functionality
- Color and brightness control