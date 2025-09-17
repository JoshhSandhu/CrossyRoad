# Solana Crossy Road - Unity Demo

This project is a "Crossy Road" style game developed in Unity, designed to showcase the integration and transaction speeds of the Solana blockchain using the Solana.Unity-SDK. The game serves as a functional demonstration of web3 features within a mobile game context.

## 🎮 Key Features

* **Classic Gameplay**: Familiar "Crossy Road" mechanics with grid-based movement and endless procedural world generation.
* **Solana Integration**: Built using the Solana.Unity-SDK to connect to the Solana Devnet.
* **On-Chain Transactions**: Features a simple in-game shop where players can purchase a new character skin. This action triggers a real SOL transfer on the Devnet, providing a clear demonstration of transaction speed and finality.
* **Advanced Architecture**: Coded using scalable design patterns, including the Singleton pattern for managers, Object Pooling for performance, and an event-driven design for decoupled logic.

## 🛠️ Tech Stack

* **Game Engine**: Unity 2021.3 LTS (or newer) with the Universal Render Pipeline (URP)
* **Language**: C#
* **Blockchain**: Solana (Devnet)
* **SDK**: Solana.Unity-SDK

## 📁 Project Structure

The project follows a clean and scalable folder structure to keep all assets organized. All custom-created assets are located within the `_Project` folder to separate them from imported packages.

```plaintext
Assets/
└── _Project/
    ├── Art/
    │   ├── Materials/
    │   └── Models/
    ├── Prefabs/
    ├── Scenes/
    └── Scripts/
        ├── Data/         (For ScriptableObjects)
        ├── Managers/
        ├── Player/
        ├── Solana/
        └── World/
```

## 🚀 Getting Started

To run this project locally, follow these steps:

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/your-username/your-project-name.git](https://github.com/your-username/your-project-name.git)
    ```
2.  **Open in Unity:** Open the project using Unity Hub. Unity will import all the necessary packages.

3.  **Test the Solana Feature:**
    * Press **Play** in the Unity Editor.
    * The game uses a temporary, in-memory wallet. The first time you run it, the wallet's public key will be printed to the console.
    * Copy this public key.
    * Go to a Solana faucet like [solfaucet.com](https://solfaucet.com), switch to the **Devnet**, and airdrop 1 or 2 SOL to the copied address.
    * You can now use the in-game shop to purchase the skin and see the transaction happen in real-time.

##  roadmap Future Goals

* **NFT Minting**: Evolve the skin shop to mint character skins as NFTs on-chain.
* **More Content**: Add a wider variety of obstacles, lane types, and character models.
* **On-Chain Leaderboard**: Implement a system to store high scores on a custom Solana program.
