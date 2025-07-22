#!/bin/bash
# debug-homebrew.sh

echo "🔍 Debugging Homebrew Formula"

# Test download
VERSION="v0.2.0"
URL="https://github.com/busy-tag/busytag-cli/archive/refs/tags/${VERSION}.tar.gz"

echo "Testing download: ${URL}"
curl -I "${URL}"

echo "Calculating SHA256..."
curl -L "${URL}" | sha256sum

echo "Testing formula syntax..."
brew install --formula ./busytag-cli.rb --dry-run