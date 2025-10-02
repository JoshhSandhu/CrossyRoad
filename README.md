# Solana Crossy Road - Unity Demo

This project is a "Crossy Road" style game developed in Unity, designed to showcase the integration and transaction speeds of the Solana blockchain using the Solana.Unity-SDK. The game serves as a functional demonstration of web3 features within a mobile game context.

## 🎮 Key Features

* **Classic Gameplay**: Familiar "Crossy Road" mechanics with grid-based movement and endless procedural world generation.
* **Solana Integration**: Built using the Solana.Unity-SDK to connect to the Solana Devnet.
* **On-Chain Transactions**: Features a simple in-game shop where players can purchase a new character skin. This action triggers a real SOL transfer on the Devnet, providing a clear demonstration of transaction speed and finality.
* **Advanced Architecture**: Coded using scalable design patterns, including the Singleton pattern for managers, Object Pooling for performance, and an event-driven design for decoupled logic.

## 🛠️ Tech Stack

* **Game Engine**: Unity 2021.3 LTS (or newer);

* **Language**: C#
* **Blockchain**: Solana Devnet
* **SDK**: Solana.Unity-SDK
* **Frontend/Web**: React + TypeScript (for web3 integration)

## 📁 Project Structure

The project follows a clean and scalable folder structure. The Unity scripts are organized with clean architecture in mind:

```plaintext
Scripts/
├── Data/              (ScriptableObjects)
├── Managers/         (Singleton pattern implementations)
│   ├── GameManager.cs
│   ├── LaneManager.cs
│   └── ObjectPooler.cs
├── Player/           (Player-related scripts)
│   ├── PlayerController.cs
│   └── PlayerInputActions.cs
├── World/            (World generation and spawning)
│   ├── WorldGenerator.cs
│   ├── LaneSpawner.cs
│   ├── ObstacleSpawner.cs
│   ├── CollectableSpawner.cs
│   └── DecorationSpawner.cs
├── Solana/          (Integration)
└── Utils/           (Camera, Effects)
    ├── CamFollow.cs
    └── WaterFoamController.cs
```

### Web3 Projects

The repository also includes multiple web3 integration approaches:

* **`pacman-dapp/`**: A direct Solana dApp implementation
* **`pacman-privy/`**: An enhanced version using Privy for wallet management and UX

Each contains:
* **Frontend**: React + TypeScript + Vite
* **Smart Contract**: Anchor framework + Rust
* **SDK**: Solana.Unity-SDK integration

## 🚀 Getting Started

To run this project locally, follow these steps:

1. **Clone the repository:**
   ```bash
   git clone https://github.com/JoshhSandhu/Rust-Pacman.git
   ```

2. **Open in Unity:**
   * Install Unity Hub
   * Open the project folder in Unity 2021.3 LTS (or newer)
   * Unity will import all necessary packages automatically

3. **Test the Solana Feature:**
   * Press **Play** in the Unity Editor.
   * The game uses a temporary, in-memory wallet. The first time you run it, the wallet's public key will be printed to the console.
   * Copy this public key.
   * Go to a Solana faucet like [solfaucet.com](https://solfaucet.com), switch to the **Devnet**, and airdrop 1 or 2 SOL to the copied address.
   * You can now use the in-game shop to purchase the skin and see the transaction happen in real-time.

4. **For Web3 Projects** (optional):
   ```bash
   # Navigate to frontend
   cd pacman-privy/frontend
   
   # Install dependencies
   npm install
   
   # Start development server
   npm run dev
   ```

## 🎮 How to Play

* **Objective**: Navigate across lanes without hitting obstacles
* **Movement**: Use arrow keys or WASD to move in any direction
* **Strategy**: Time your movements to avoid oncoming obstacles
* **Shop**: Collect coins and purchase new skins using SOL transactions

## 🛠️ Core Game Systems

* **Lane Management**: Procedural lane generation with multiple types (road, water, train tracks)
* **Object Pooling**: Efficient memory management for spawning obstacles and collectables
* **Event-Driven Architecture**: Spawners communicate through interfaces for clean decoupling
* **Data-Driven Design**: ScriptableObject-based data for easy level editing

## 📝 Future Goals

* **NFT Minting**: Evolve the skin shop to mint character skins as NFTs on-chain
* **More Content**: Add a wider variety of obstacles, lane types, and character models
* **On-Chain Leaderboard**: Implement a system to store high scores on a custom Solana program
* **Mobile Optimization**: Enhance performance for mobile platforms
* **Multiplayer**: Add real-time multiplayer functionality using Solana's speed

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is distributed under the MIT License. See the `LICENSE` file for more information.
