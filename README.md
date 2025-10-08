# Crossy Road - Unity Game

This project is a "Crossy Road" style game developed in Unity, featuring classic endless runner gameplay with procedural world generation, object pooling for optimal performance, and a comprehensive skin customization system with Solana blockchain integration.

## 🎮 Key Features

* **Classic Gameplay**: Familiar "Crossy Road" mechanics with grid-based movement and endless procedural world generation
* **Skin Customization System**: Complete character skin management with categories (Man, Woman, Animal, Farm) and rarity levels (Common, Rare, Epic, Legendary)
* **Solana Integration**: Blockchain-based skin purchasing and ownership with wallet connectivity
* **Advanced UI System**: Modern UI with safe area handling, smooth transitions, and category-based skin browsing
* **Performance Optimization**: Advanced architecture using Singleton patterns, Object Pooling, and event-driven design
* **Procedural World**: Dynamic lane generation with multiple lane types (road, water, train tracks, grass) and intelligent path validation
* **Scalable Design**: Clean separation of concerns with interface-based architecture

## 🛠️ Tech Stack

* **Game Engine**: Unity 2021.3 LTS (or newer)
* **Language**: C#
* **Blockchain**: Solana integration for NFT skins and wallet connectivity
* **UI Framework**: Unity UI (uGUI) with TextMeshPro
* **Architecture**: Clean Architecture with Interfaces
* **Design Patterns**: Singleton, Object Pooling, Observer Pattern, Factory Pattern
* **Input System**: Unity Input System with touch and keyboard support

## 📁 Project Structure

The project follows a clean and scalable folder structure. The Unity scripts are organized with clean architecture in mind:

```plaintext
Scripts/
├── Data/              (ScriptableObjects and Data Management)
│   ├── SkinData.cs           (Skin system data structures)
│   └── LaneType.cs           (Lane configuration data)
├── Managers/         (Singleton pattern implementations)
│   ├── GameManager.cs        (Core game state management)
│   ├── LaneManager.cs        (Lane lifecycle management)
│   ├── ObjectPooler.cs       (Object pooling system)
│   ├── ShopManager.cs        (Skin shop and blockchain integration)
│   └── StartScreenManager.cs (Main menu and UI flow)
├── Player/           (Player-related scripts)
│   ├── PlayerController.cs   (Player movement and input)
│   └── PlayerInputActions.cs (Input action definitions)
├── World/            (World generation and spawning)
│   ├── WorldGenerator.cs     (Main world orchestration)
│   ├── LaneManager.cs        (Lane queue management)
│   ├── PathValidator.cs      (Lily pad path validation)
│   └── Spawners/
│       ├── LaneSpawner.cs        (Lane spawning logic)
│       ├── ObstacleSpawner.cs    (Obstacle placement)
│       ├── CollectableSpawner.cs (Coin and power-up spawning)
│       └── DecorationSpawner.cs  (Environmental decorations)
├── UI/               (User Interface Components)
│   ├── SkinItemUI.cs         (Individual skin display)
│   ├── CategoryDetailPanel.cs (Skin category browsing)
│   ├── EquippedSkinDisplay.cs (Currently equipped skin)
│   ├── SafeArea.cs           (Device safe area handling)
│   └── FadeTransition.cs     (Screen transition effects)
├── Solana/          (Blockchain Integration - Future)
└── Camera/          (Camera and Visual Effects)
    ├── CamFollow.cs
    └── WaterFoamController.cs
```

## 🚀 Getting Started

To run this project locally, follow these steps:

1. **Clone the repository:**
   ```bash
   [git clone https://github.com/JoshhSandhu/CrossyToad.git](https://github.com/JoshhSandhu/CrossyRoad.git)
   ```

2. **Open in Unity:**
   * Install Unity Hub
   * Open the project folder in Unity 2021.3 LTS (or newer)
   * Unity will import all necessary packages automatically

3. **Run the Game:**
   * Press **Play** in the Unity Editor
   * Use arrow keys or WASD to navigate the character
   * Avoid obstacles and cross as many lanes as possible
   * Access the skin shop to customize your character
   * Connect your Solana wallet to purchase NFT skins
   * The game features procedural world generation with different lane types

## 🎮 How to Play

* **Objective**: Navigate across lanes without hitting obstacles
* **Movement**: Use arrow keys or WASD to move in any direction
* **Strategy**: Time your movements to avoid oncoming obstacles
* **Collectibles**: Gather coins and power-ups while avoiding obstacles
* **Skin Customization**: Access the shop to browse and purchase character skins
* **Blockchain Integration**: Connect your Solana wallet to buy NFT skins
* **Categories**: Explore different skin categories (Man, Woman, Animal, Farm)
* **Rarity**: Collect skins of different rarity levels for unique appearances

## 🛠️ Core Game Systems

* **Advanced Lane Management**: Procedural lane generation with multiple types (road, water, train tracks, grass) and intelligent sequencing to prevent consecutive water lanes
* **Path Validation System**: Smart lily pad path validation ensuring solvable routes with configurable jump distances
* **Object Pooling**: Efficient memory management for spawning obstacles, collectables, and lanes with automatic cleanup
* **Event-Driven Architecture**: Spawners communicate through interfaces for clean decoupling and modular design
* **Data-Driven Design**: ScriptableObject-based data for easy level editing and skin configuration
* **Skin Management System**: Complete character customization with categories, rarity levels, ownership tracking, and blockchain integration
* **Safe Area UI**: Automatic UI adaptation for different device screen sizes and safe areas (notches, home indicators)
* **Smooth Transitions**: Fade effects and animated UI transitions for polished user experience

## 🎨 Skin System Features

* **Category-Based Organization**: Skins organized into Man, Woman, Animal, and Farm categories
* **Rarity System**: Four rarity levels (Common, Rare, Epic, Legendary) with visual indicators
* **Ownership Tracking**: Persistent skin ownership and equipped status
* **Blockchain Integration**: Solana-based purchasing and NFT ownership
* **Wallet Connectivity**: Secure wallet connection for transactions
* **Visual Customization**: Real-time skin preview and equipped skin display
* **Shop Interface**: Intuitive browsing with category filters and detailed views

## 📝 Future Goals

* **Enhanced Blockchain Features**: Complete Solana integration with NFT marketplace
* **More Skin Content**: Expand skin categories and add seasonal collections
* **Social Features**: Share high scores and skin collections
* **Advanced Scoring**: Implement combo systems and achievement tracking
* **Sound Design**: Add comprehensive audio feedback and background music
* **Mobile Optimization**: Enhanced touch controls and performance tuning
* **Level Progression**: Create increasing difficulty patterns and unlockable content

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is distributed under the MIT License. See the `LICENSE` file for more information.
