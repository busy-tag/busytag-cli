#!/bin/bash
set -e

# BusyTag CLI Installer
# Installs busytag-cli via .NET tool and ensures PATH is configured

TOOL_NAME="BusyTag.CLI"
COMMAND_NAME="busytag-cli"
DOTNET_TOOLS_DIR="$HOME/.dotnet/tools"

echo "BusyTag CLI Installer"
echo "====================="
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed."
    echo "Install .NET 8.0+ from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version 2>/dev/null)
echo "Found .NET SDK: $DOTNET_VERSION"

# Install or update the tool
if dotnet tool list -g | grep -qi "busytag.cli"; then
    echo "Updating $TOOL_NAME..."
    dotnet tool update -g "$TOOL_NAME"
else
    echo "Installing $TOOL_NAME..."
    dotnet tool install -g "$TOOL_NAME"
fi

# Check if dotnet tools directory is on PATH
add_to_path() {
    local shell_profile="$1"
    local shell_name="$2"

    if [ -f "$shell_profile" ] && grep -q "$DOTNET_TOOLS_DIR" "$shell_profile" 2>/dev/null; then
        return 0
    fi

    echo "" >> "$shell_profile"
    echo "# .NET tools (added by BusyTag CLI installer)" >> "$shell_profile"
    echo "export PATH=\"\$PATH:$DOTNET_TOOLS_DIR\"" >> "$shell_profile"
    echo "Added $DOTNET_TOOLS_DIR to PATH in $shell_profile"
    return 1
}

if echo "$PATH" | tr ':' '\n' | grep -qx "$DOTNET_TOOLS_DIR"; then
    echo "PATH is already configured."
else
    echo ""
    echo "Configuring PATH..."
    PROFILE_UPDATED=false

    CURRENT_SHELL=$(basename "$SHELL")
    case "$CURRENT_SHELL" in
        zsh)
            add_to_path "$HOME/.zprofile" "zsh" && true || PROFILE_UPDATED=true
            ;;
        bash)
            if [ "$(uname)" = "Darwin" ]; then
                add_to_path "$HOME/.bash_profile" "bash" && true || PROFILE_UPDATED=true
            else
                add_to_path "$HOME/.bashrc" "bash" && true || PROFILE_UPDATED=true
            fi
            ;;
        fish)
            FISH_CONFIG="$HOME/.config/fish/config.fish"
            if ! grep -q "$DOTNET_TOOLS_DIR" "$FISH_CONFIG" 2>/dev/null; then
                mkdir -p "$(dirname "$FISH_CONFIG")"
                echo "" >> "$FISH_CONFIG"
                echo "# .NET tools (added by BusyTag CLI installer)" >> "$FISH_CONFIG"
                echo "fish_add_path $DOTNET_TOOLS_DIR" >> "$FISH_CONFIG"
                echo "Added $DOTNET_TOOLS_DIR to PATH in $FISH_CONFIG"
                PROFILE_UPDATED=true
            fi
            ;;
        *)
            add_to_path "$HOME/.profile" "sh" && true || PROFILE_UPDATED=true
            ;;
    esac

    # Make available in current session
    export PATH="$PATH:$DOTNET_TOOLS_DIR"

fi

echo ""

# Verify installation (within this script's process)
if [ -x "$DOTNET_TOOLS_DIR/$COMMAND_NAME" ]; then
    echo "Installation successful!"
    echo ""
    "$DOTNET_TOOLS_DIR/$COMMAND_NAME" --version
    echo ""
    echo "To start using busytag-cli, either:"
    echo "  1. Open a new terminal window, or"
    echo "  2. Run: export PATH=\"\$PATH:$DOTNET_TOOLS_DIR\""
else
    echo "Warning: Installation completed but '$COMMAND_NAME' was not found."
    echo "Try restarting your terminal, then run: $COMMAND_NAME --version"
fi
