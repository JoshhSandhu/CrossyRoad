# Crossy Road - Unity Game

This project is a "Crossy Road" style game developed in Unity, featuring classic endless runner gameplay with procedural world generation and object pooling for optimal performance.

## 🎮 Key Features

* **Classic Gameplay**: Familiar "Crossy Road" mechanics with grid-based movement and endless procedural world generation
* **Performance Optimization**: Advanced architecture using Singleton patterns, Object Pooling, and event-driven design
* **Procedural World**: Dynamic lane generation with multiple lane types (road, water, train tracks, grass)
* **Scalable Design**: Clean separation of concerns with interface-based architecture

## 🛠️ Tech Stack

* **Game Engine**: Unity 2021.3 LTS (or newer)
* **Language**: C#
* **Architecture**: Clean Architecture with Interfaces
* **Design Patterns**: Singleton, Object Pooling, Observer Pattern

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

### Additional Rust Projects

The repository also includes Rust-based implementations:

* **`pacman-terminal/`**: A terminal-based Pacman game
* **`pacman-GUI/`**: A graphical Pacman game using macroquad
* **`pacman-dapp/`** and **`pacman-privy/`**: Web3 implementations (separate from Unity game)

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

3. **Run the Game:**
   * Press **Play** in the Unity Editor
   * Use arrow keys or WASD to navigate the character
   * Avoid obstacles and cross as many lanes as possible
   * The game features procedural world generation with different lane types

## 🎮 How to Play

* **Objective**: Navigate across lanes without hitting obstacles
* **Movement**: Use arrow keys or WASD to move in any direction
* **Strategy**: Time your movements to avoid oncoming obstacles
* **Collectibles**: Gather coins and power-ups while avoiding obstacles

## 🛠️ Core Game Systems

* **Lane Management**: Procedural lane generation with multiple types (road, water, train tracks)
* **Object Pooling**: Efficient memory management for spawning obstacles and collectables
* **Event-Driven Architecture**: Spawners communicate through interfaces for clean decoupling
* **Data-Driven Design**: ScriptableObject-based data for easy level editing

## 📝 Future Goals

* **More Content**: Add a wider variety of obstacles, lane types, and character models
* **Score System**: Implement high score tracking and leaderboards
* **Power-ups**: Add special abilities and temporary invincibility
* **Mobile Optimization**: Enhance performance for mobile platforms
* **Sound Effects**: Add audio feedback and background music
* **Level Progression**: Create increasing difficulty patterns

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is distributed under the MIT License. See the `LICENSE` file for more information.
