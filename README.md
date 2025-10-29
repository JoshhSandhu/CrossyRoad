# Crossy Road - Unity Game with Solana Blockchain Integration

A modern Unity-based endless runner game inspired by Crossy Road, featuring Solana blockchain integration for on-chain player movement tracking and seamless wallet authentication.

## 🎮 Game Overview

Crossy Road is an endless runner where players navigate a character through procedurally generated lanes filled with obstacles, vehicles, water hazards, and collectibles. The objective is to move as far forward as possible while avoiding collisions and collecting coins.

## ✨ Features

### Core Gameplay
- **Endless Runner Mechanics**: Procedurally generated world with infinite lanes
- **Multiple Lane Types**: Grass, Roads, Water, Train Tracks
- **Dynamic Obstacles**: Moving cars, trains, logs, and environmental hazards
- **Coin Collection System**: Collect coins to unlock new character skins
- **Progressive Difficulty**: Game difficulty increases as you progress further

### Blockchain Integration
- **Solana Blockchain Support**: Player movements recorded on Solana devnet
- **MagicBlocks SDK Integration**: Reliable transaction batching and execution
- **Privy Wallet Authentication**: Seamless wallet connection and management
- **Hybrid Transaction Service**: Combines Privy authentication with MagicBlocks SDK
- **Transaction Batching**: Optimized transaction handling for better performance

### Technical Features
- **SOLID Principles**: Clean architecture with dependency injection
- **Unity Input System**: Support for both touch and keyboard controls
- **Object Pooling**: Optimized object management for better performance
- **Dynamic World Generation**: Procedural lane generation with decorations
- **Camera System**: Smooth following camera with shake effects

## 🛠️ Technology Stack

- **Game Engine**: Unity 6000.2.6f2
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Input System**: Unity Input System 1.15.0
- **Blockchain**: Solana (via Solana Unity SDK)
- **Wallet**: Privy + MagicBlocks SDK
- **Architecture**: SOLID principles with dependency injection

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **Unity Hub** with Unity 6000.2.6f2 or compatible version
- **Visual Studio** or **Visual Studio Code** (for C# development)
- **Git** (for version control)
- **Solana Wallet** (Phantom, Solflare, or compatible wallet for blockchain features)

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CrossyRoad
```

### 2. Open in Unity

1. Launch Unity Hub
2. Click "Open" or "Add" and select the `CrossyRoad` folder
3. Wait for Unity to import all assets and packages

### 3. Install Dependencies

The project uses Unity Package Manager. All dependencies should be automatically resolved from `Packages/manifest.json`:

- Solana Unity SDK
- Unity Input System
- Universal Render Pipeline
- And other required packages

### 4. Configure MagicBlocks (Optional)

For blockchain integration:

1. Navigate to `Assets/_Project/Scripts/Managers/MagicBlocks/`
2. Review `MagicBlocks_Installation_Guide.md` for detailed setup instructions
3. Create a `MagicBlocksConfig` asset in the Project window
4. Configure RPC endpoint (default: `https://api.devnet.solana.com`)
5. Set wallet network to `devnet` (or `mainnet` for production)

### 5. Configure Privy (Optional)

For wallet authentication:

1. Navigate to `Assets/_Project/Scripts/Managers/privy/`
2. Set up your Privy configuration
3. Assign the configuration to the authentication manager

## 🎮 Controls

### Mobile (Touch)
- **Swipe Up**: Move forward
- **Swipe Down**: Move backward
- **Swipe Left**: Move left
- **Swipe Right**: Move right

### Desktop (Keyboard)
- **W / Up Arrow**: Move forward
- **S / Down Arrow**: Move backward
- **A / Left Arrow**: Move left
- **D / Right Arrow**: Move right

## 📁 Project Structure

```
Assets/
├── _Project/
│   ├── Art/                    # Models, Materials, Textures, Animations
│   ├── Audio/                  # Music and SFX
│   ├── Prefabs/                # Game object prefabs
│   ├── Scenes/                 # Unity scenes
│   ├── Scripts/
│   │   ├── Camera/             # Camera controllers
│   │   ├── Data/               # ScriptableObjects and data structures
│   │   ├── Managers/
│   │   │   ├── GameManager.cs       # Main game state management
│   │   │   ├── UIManager.cs         # UI management
│   │   │   ├── ShopManager.cs       # Skin shop system
│   │   │   ├── MagicBlocks/         # Solana blockchain integration
│   │   │   └── privy/               # Privy wallet integration
│   │   ├── Obstacles/          # Car, Train, Log, Coin scripts
│   │   ├── Player/
│   │   │   ├── PlayerController.cs  # Main player controller
│   │   │   ├── Components/          # SOLID principle components
│   │   │   ├── Adapters/            # Interface adapters
│   │   │   └── Interfaces/          # SOLID interfaces
│   │   ├── UI/                 # UI scripts
│   │   └── World/              # World generation and lane management
│   └── UI/                     # UI assets (sprites, prefabs)
└── Plugins/
    ├── Android/                # Android-specific plugins
    └── Privy/                  # Privy SDK integration
```

## 🔗 Blockchain Integration

### Solana Integration

The game integrates with Solana blockchain to record player movements on-chain:

- **Network**: Solana Devnet (configurable)
- **Transactions**: Player movement transactions batched for efficiency
- **Wallet**: Privy wallet adapter for seamless authentication
- **SDK**: MagicBlocks Solana Unity SDK

### Transaction Flow

1. Player makes a movement
2. Movement is validated
3. Transaction message is created
4. Transaction is batched (if batching enabled)
5. Batch is sent to Solana via MagicBlocks SDK
6. Transaction signature is received and tracked

### Configuration

Edit the `MagicBlocksSolanaAdapter` component in your scene:

- **RPC Endpoint**: Solana RPC endpoint URL
- **Wallet Network**: `devnet` or `mainnet`
- **Transaction Batching**: Enable/disable batching
- **Batch Size**: Number of transactions per batch (default: 5)
- **Transaction Timeout**: Timeout in seconds (default: 5)

## 🏗️ Architecture

### SOLID Principles

The project follows SOLID principles for maintainable and extensible code:

- **Single Responsibility**: Each component has one clear purpose
- **Open/Closed**: Extensible through interfaces without modifying existing code
- **Liskov Substitution**: Components can be swapped via interfaces
- **Interface Segregation**: Focused, minimal interfaces
- **Dependency Injection**: Dependencies injected through constructors

### Key Components

- **PlayerController**: Orchestrates player behavior
- **GameManager**: Manages game state and score
- **WorldGenerator**: Procedurally generates game lanes
- **MagicBlocksSolanaAdapter**: Handles Solana transactions
- **HybridTransactionService**: Combines authentication and transaction services

## 🎨 Customization

### Adding New Skins

1. Create skin assets in `Assets/_Project/Art/`
2. Add skin data to `SkinDatabase` asset
3. Configure skin properties (name, price, category)
4. Skins will appear in the shop automatically

### Adding New Lane Types

1. Create lane data scriptable object (extends `LaneData`)
2. Configure lane properties (obstacles, decorations, spawn settings)
3. Add lane type to lane spawner configuration
4. Update `LaneType` enum if needed

## 🐛 Troubleshooting

### Blockchain Issues

- **Transactions not sending**: Check wallet connection and network configuration
- **MagicBlocks not initializing**: Verify SDK installation and configuration asset setup
- **Wallet not connecting**: Ensure Privy configuration is correct

### Game Issues

- **Player not moving**: Check input system configuration
- **Obstacles not spawning**: Verify obstacle spawner configuration and prefabs
- **Performance issues**: Check object pooling settings and reduce batch sizes

## 📝 Development Notes

- The game uses Unity 6 (6000.2.6f2)
- Universal Render Pipeline (URP) is required
- Some features require Solana devnet setup
- All blockchain features are optional - game can run without wallet connection

## 🤝 Contributing

This is a game development project. Contributions should follow the existing SOLID architecture and code style.

## 📄 License

[Specify your license here]

## 🙏 Credits

- **Unity Technologies**: Unity Engine
- **Solana Labs**: Solana Blockchain
- **MagicBlocks**: MagicBlocks SDK
- **Privy**: Wallet Authentication SDK
- **Polyperfect**: Low Poly Ultimate Pack (asset pack)

## 🔗 Links

- [Unity Documentation](https://docs.unity3d.com/)
- [Solana Documentation](https://docs.solana.com/)
- [MagicBlocks SDK](https://github.com/magicblock-labs/Solana.Unity-SDK)
- [Privy Documentation](https://docs.privy.io/)

---

**Note**: This game is in active development. Some features may be incomplete or subject to change.
