#!/bin/bash
set -euo pipefail

APP_NAME="Didi Workspace"
EXECUTABLE_NAME="DidiApp"
TFM="net8.0"
CONFIG="Release"

OS_TYPE="$(uname)"
ARCH="$(uname -m)"

if [[ "$OS_TYPE" == "Darwin" ]]; then
    PLATFORM="osx"
    DESKTOP_PATH="$HOME/Desktop"
    [[ "$ARCH" == "arm64" ]] && RID="osx-arm64" || RID="osx-x64"
    FINAL_EXECUTABLE="$EXECUTABLE_NAME"
else
    PLATFORM="win"
    DESKTOP_PATH="/c/Users/$USER/Desktop" 
    [ -d "$USERPROFILE/Desktop" ] && DESKTOP_PATH="$USERPROFILE/Desktop"
    
    [[ "$ARCH" == "x86_64" ]] && RID="win-x64" || RID="win-arm64"
    FINAL_EXECUTABLE="${EXECUTABLE_NAME}.exe"
fi

echo " Building for $PLATFORM ($RID)..."


if [[ "$PLATFORM" == "win" ]]; then
    dotnet publish -c "$CONFIG" -r "$RID" --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=None \
    -p:DebugSymbols=false
else
    dotnet publish -c "$CONFIG" -r "$RID" --self-contained true
fi


PUBLISH_DIR="bin/$CONFIG/$TFM/$RID/publish"


if [[ "$PLATFORM" == "osx" ]]; then
    echo " Creating macOS .app bundle..."
    APP_DIR="bin/$CONFIG/$TFM/$RID/$APP_NAME.app"
    MACOS_DIR="$APP_DIR/Contents/MacOS"
    
    mkdir -p "$MACOS_DIR"
    cp -R "$PUBLISH_DIR/"* "$MACOS_DIR/"
    

    echo " Unblocking native libraries for macOS Gatekeeper..."
    find "$MACOS_DIR" -name "*.dylib" -exec xattr -d com.apple.quarantine {} \; 2>/dev/null || true

    
    cat > "$APP_DIR/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<plist version="1.0"><dict>
  <key>CFBundleName</key><string>$APP_NAME</string>
  <key>CFBundleExecutable</key><string>$EXECUTABLE_NAME</string>
  <key>CFBundleIdentifier</key><string>com.younes.didiworkspace</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
</dict></plist>
EOF
    INSTALL_SOURCE="$APP_DIR"
else
    echo " Preparing Windows executable..."
    INSTALL_SOURCE="$PUBLISH_DIR/$FINAL_EXECUTABLE"
fi


echo " Moving to Desktop for easy access..."
rm -rf "$DESKTOP_PATH/$APP_NAME.app" 2>/dev/null || true
rm -f "$DESKTOP_PATH/$FINAL_EXECUTABLE" 2>/dev/null || true

cp -R "$INSTALL_SOURCE" "$DESKTOP_PATH/"

echo "-------------------------------------------"
echo " Done! Your app is now on your Desktop (probably)"
echo " Location: $DESKTOP_PATH"