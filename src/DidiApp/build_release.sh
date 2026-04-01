#!/bin/bash
set -e

APP_NAME="Didi Workspace"
EXECUTABLE_NAME="DidiApp"

# 1. Detect your Mac's chip automatically
ARCH="$(uname -m)"
if [[ "$ARCH" == "arm64" ]]; then
  RID="osx-arm64"
else
  RID="osx-x64"
fi

echo "🚀 Compiling Release version for $RID..."
# 2. Build the self-contained Release version
dotnet publish -c Release -r "$RID" --self-contained true -o ./mac_release_build

APP_DIR="./mac_release_build/$APP_NAME.app"
MACOS_DIR="$APP_DIR/Contents/MacOS"

echo "📦 Packaging native macOS .app bundle..."
mkdir -p "$MACOS_DIR"

# 3. Move the compiled files into the macOS bundle structure
cp -R ./mac_release_build/* "$MACOS_DIR/" 2>/dev/null || true
rm -rf "$MACOS_DIR/$APP_NAME.app" 2>/dev/null || true

# 4. Create the Info.plist (tells macOS how to run your app)
cat > "$APP_DIR/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>$APP_NAME</string>
  <key>CFBundleExecutable</key>
  <string>$EXECUTABLE_NAME</string>
  <key>CFBundleIdentifier</key>
  <string>com.girlyworkspace.didi</string>
  <key>CFBundleVersion</key>
  <string>1.0</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>LSMinimumSystemVersion</key>
  <string>11.0</string>
</dict>
</plist>
PLIST

echo "✨ Build complete! Opening your app..."
# 5. Launch the app natively!
open "$APP_DIR"
