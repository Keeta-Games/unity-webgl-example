# Unity WebGL Keeta Wallet Integration Example

Unity 6 WebGL game that connects to Keethings Wallet browser extension and displays wallet address and KTA balance from Keeta Test Network.

## Quick Start

1. Install [Keethings Wallet](https://chromewebstore.google.com/detail/keythings-wallet/jhngbkboonmpephhenljbljnpffabloh) extension
2. Open project in Unity 6000.2.8f1
3. Build for WebGL (File > Build Settings > WebGL > Build)
4. Serve build folder: `python -m http.server 8000 -d Build`
5. Open http://localhost:8000 in your browser
6. Click "Connect Wallet" and approve the connection

## Features

- Detect and connect to Keethings Wallet extension
- Display connected wallet address
- Query and display KTA balance from Keeta Network testnet
- Disconnect wallet functionality

## Project Structure

```
Assets/
├── Plugins/WebGL/
│   └── KeetaWalletBridge.jslib  # JavaScript bridge to wallet
├── Scripts/
│   ├── WalletManager.cs         # Wallet connection manager
│   └── UIController.cs          # UI event handler
└── WebGLTemplates/KeetaWallet/  # Custom template with Keeta SDK
    └── index.html               # Includes Keeta Client SDK
```

## Requirements

### Software
- Unity 6000.2.8f1 (Unity 6.2) or newer
- Modern web browser (Chrome 119+ recommended)
- Local web server (Python 3 or Node.js)

### Browser Extensions
- [Keethings Wallet](https://chromewebstore.google.com/detail/keythings-wallet/jhngbkboonmpephhenljbljnpffabloh)

### Keeta Network Account
- Test wallet with KTA on Keeta testnet (for balance display)
- Get testnet tokens from [Keeta Faucet](https://faucet.test.keeta.com/)

## Setup Instructions

### 1. Clone and Open Project

```bash
git clone git@github.com:Keeta-Games/unity-webgl-example.git
cd unity-webgl-example
```

Open the project in Unity Hub > Add > Select the `unity-webgl-example` folder

### 2. Install Keethings Wallet

1. Install the extension from [Chrome Web Store](https://chromewebstore.google.com/detail/keythings-wallet/jhngbkboonmpephhenljbljnpffabloh)
2. Create a new wallet or import existing
3. Switch to Keeta Testnet in the wallet settings
4. Request testnet KTA tokens if needed from [Keeta Faucet](https://faucet.test.keeta.com/)

### 3. Verify Unity Settings

Open Edit > Project Settings > Player > WebGL:

- API Compatibility: .NET Standard 2.1
- Compression Format: **Disabled** (for local testing)
- WebGL Template: KeetaWallet

## Building the Unity Project

1. Open File > Build Settings
2. Select WebGL platform
3. Click "Build" and choose output folder
4. Wait for build to complete

## Running Locally

### Option 1: Python HTTP Server (Easiest)

```bash
# Navigate to build folder
cd Build

# Python 3
python -m http.server 8000

# Python 2 (if needed)
python -m SimpleHTTPServer 8000
```

Open http://localhost:8000 in your browser

### Option 2: Node.js HTTP Server

```bash
# Install http-server globally
npm install -g http-server

# Serve build folder
cd Build
http-server -p 8000
```

Open http://localhost:8000 in your browser


### Important: Disable Compression for Local Testing

Unity's default Brotli compression requires HTTPS. For local HTTP testing:
1. Player Settings > Publishing Settings > Compression Format: **Disabled**
2. Rebuild the project

## Testing the Integration

### Manual Testing Checklist

1. **Wallet Detection**
   - Open browser console (F12)
   - Should see "WalletManager initialized" message
   - If not, verify Keethings Wallet is installed and enabled

2. **Connect Wallet**
   - Click "Connect Wallet" button
   - Wallet address should display in game

3. **View Balance**
   - After connection, KTA balance should load automatically
   - Balance updates from Keeta testnet

4. **Disconnect**
   - Click "Disconnect" button
   - UI should reset to initial state


## Architecture Overview

### Communication Flow

```
Unity C# (Game Logic)
    ↕ [DllImport/SendMessage]
.jslib Plugin (Bridge Layer)
    ↕ [Native calls]
JavaScript (Browser/Wallet)
    ↕ [RPC/SDK]
Keeta Network (Blockchain)
```

### How It Works

1. **Unity to Browser**: C# calls `[DllImport]` methods defined in `.jslib`
2. **Browser to Unity**: JavaScript calls `unityInstance.SendMessage()`
3. **Wallet Operations**: JavaScript uses `window.keeta` API
4. **Blockchain Queries**: Keeta Client SDK queries network

### Key Components

**WalletManager.cs**
- Singleton manager for wallet state
- Exposes public methods (Connect, Disconnect, GetBalance)
- Fires events (OnWalletConnected, OnBalanceUpdated)

**KeetaWalletBridge.jslib**
- JavaScript bridge for wallet operations
- Handles async wallet requests
- Uses SendMessage to return data to Unity

**KeetaWallet/index.html**
- Custom WebGL template
- Includes Keeta Client SDK
- Stores Unity instance globally

## Troubleshooting

### Wallet Not Detected

**Symptoms**: "Wallet not detected" error

**Solutions**:
- Verify Keethings Wallet extension is installed and enabled
- Hard refresh browser (Ctrl+Shift+R or Cmd+Shift+R)
- Check browser console for JavaScript errors
- Try incognito/private mode to rule out extension conflicts

### Build Won't Load

**Symptoms**: Blank screen or "Failed to load" error

**Solutions**:
- Check compression settings (disable for local HTTP)
- Verify file permissions on Build folder
- Try different browser (Chrome recommended)
- Check browser console for specific errors
- Ensure serving via HTTP/HTTPS (not file://)

## Development Tips

### Debugging WebGL Builds

1. Always use Development Builds for testing
2. Open browser console (F12) for logs
3. Use `Debug.Log()` in C# - appears in browser console
4. Check Network tab for SDK/RPC requests

### Iterating Quickly

- Use "Build and Run" in Unity (auto-opens browser)
- Keep browser console open to catch errors
- Use browser dev tools to test JavaScript separately
- Test wallet scenarios in isolation before Unity integration

### Best Practices

1. **Always destroy Keeta clients**: Call `await client.destroy()` when done
2. **Handle errors gracefully**: Wallet operations can fail for many reasons
3. **Validate addresses**: Check format before blockchain operations
4. **Use events**: Decouple wallet logic from game logic with C# events
5. **Cache when possible**: Don't query blockchain on every frame

### Performance Considerations

- WebGL is single-threaded (no C# Tasks/async)
- Use coroutines for delayed operations
- Minimize JavaScript ↔ Unity calls
- Cache wallet address and balance
- Query blockchain only when needed (on events, not per frame)

## Important Notes

- Does NOT work in Unity Editor (WebGL builds only)
- DISABLE compression when using simple HTTP server
- Must be served via HTTP/HTTPS (file:// protocol won't work)
- Keethings Wallet must be installed before testing
- Uses Keeta testnet by default 

## Resources

### Keeta Network
- Official Site: https://keeta.com/
- Documentation: https://docs.keeta.com/
- SDK Documentation: https://static.network.keeta.com/docs/
- Explorer: https://explorer.keeta.com/
- GitHub: https://github.com/KeetaNetwork

### Keethings Wallet
- Chrome Extension: https://chromewebstore.google.com/detail/keythings-wallet/jhngbkboonmpephhenljbljnpffabloh

### Unity WebGL
- Browser Scripting: https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
- WebGL Templates: https://docs.unity3d.com/6000.2/Documentation/Manual/webgl-templates.html
- Performance Tips: https://docs.unity3d.com/Manual/webgl-performance.html
