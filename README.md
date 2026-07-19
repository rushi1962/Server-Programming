# Card Game TCP Server

A small **authoritative multiplayer card game server built in C# from scratch** to learn networking, server architecture, concurrency, authentication, persistence, deployment, and scalable backend design.

This project started as a simple TCP server and gradually evolved into a complete multiplayer backend for a two-player PvP card game.

---

## 🎮 About the Game

The game is a two-player PvP card game.

Each player has:

* ❤️ Health
* 🔷 Mana
* 🃏 Three card types:

  * Attack
  * Heal
  * Mana Boost

Players take turns performing actions.

The server is **authoritative** and owns the complete game state.

The client only sends actions.

The server validates and executes them.

---

## 🏗️ Architecture

```text
                    ┌─────────────────┐
                    │   Unity Client  │
                    └────────┬────────┘
                             │
                             │ TCP
                             │
                    ┌────────▼────────┐
                    │   TCP Server    │
                    └────────┬────────┘
                             │
       ┌─────────────────────┼─────────────────────┐
       │                     │                     │
┌──────▼──────┐      ┌───────▼──────┐      ┌───────▼──────┐
│ Authentication│      │ Matchmaking │      │ Game Server  │
│   Service     │      │   Service   │      │   / Match    │
└──────┬───────┘      └──────────────┘      └───────┬──────┘
       │                                             │
       │                                             │
┌──────▼───────┐                              ┌──────▼──────┐
│ Account       │                              │ Game State  │
│ Service       │                              │             │
└──────┬───────┘                              └─────────────┘
       │
┌──────▼───────┐
│   SQLite      │
│   Database    │
└──────────────┘
```

---

## ✨ Features

### Networking

* TCP socket server built using C#
* Custom binary packet protocol
* Client connection management
* Separate system and gameplay packets
* Thread-safe network communication
* `BinaryReader` / `BinaryWriter` based packet handling

### Multiplayer Architecture

* Authoritative server architecture
* Server-owned game state
* Server-side action validation
* Two-player matchmaking
* Match lifecycle management
* Thread-safe shared collections

### Connection Management

* Heartbeat monitoring
* Connection health states:

  * `Connected`
  * `Lagging`
  * `Disconnected`
* Reconnection grace period
* Reconnection token verification
* TCP socket and stream replacement during reconnection

### Authentication

* Guest login
* Account ID based login
* Persistent player accounts
* Authentication before matchmaking

### Persistence

* SQLite database integration
* Account persistence
* Match history
* Player statistics
* Wins, losses and draws
* In-memory account cache

### Server Operations

* Structured logging
* Console server commands
* Graceful server shutdown
* Configurable server settings
* Standalone executable deployment

---

## 🧠 What I Learned

This project was built as a practical way to learn **server programming and multiplayer backend development from first principles**.

The major topics covered were:

* IP addresses and ports
* TCP and UDP
* Sockets
* TCP message boundaries
* Client-server architecture
* Threading and asynchronous programming
* `Task`, `Task.Run` and thread pools
* Custom packet protocols
* Serialization
* Authoritative game servers
* Matchmaking
* Thread safety
* `lock` and `ConcurrentQueue`
* Heartbeat monitoring
* Reconnection systems
* Authentication
* SQL and SQLite
* Database caching
* Persistent data
* Graceful shutdown
* Deployment
* Scalability concepts

I also studied how this architecture could evolve into a larger production backend using technologies such as **PostgreSQL, Redis, Docker, load balancers and multiple game servers**.

---

## 🔄 Match Flow

```text
Client Connects
       ↓
Authentication
       ↓
Matchmaking
       ↓
Match Created
       ↓
Gameplay
       ↓
Heartbeat Monitoring
       ↓
Reconnection (if required)
       ↓
Match Completed
       ↓
Match Result Saved
       ↓
Player Statistics Updated
```

---

## 🔐 Authoritative Server

The server owns the actual game state.

For example:

```text
Client → "Attack"
```

The client does **not** decide whether the attack is valid.

Instead:

```text
Client
   ↓
Attack Request
   ↓
Server
   ↓
Validate Action
   ↓
Update Game State
   ↓
Send New State
```

This prevents the client from directly modifying important gameplay data and provides a foundation for cheat-resistant multiplayer architecture.

---

## 💾 Database Design

The project uses SQLite for persistent storage.

The database stores:

### Accounts

```text
AccountID
DisplayName
CreatedAt
```

### Match Results

```text
MatchID
WinnerAccountID
LoserAccountID
```

### Player Statistics

```text
AccountID
Wins
Losses
Draws
```

SQLite was intentionally chosen because this is a small learning project and does not require a dedicated database server.

---

## 🚀 Running the Server

### Requirements

* .NET SDK
* C# development environment

### Build

Clone the repository:

```bash
git clone https://github.com/rushi1962/Server-Programming.git
```

Navigate to the project:

```bash
cd CardGameTCPServer
```

Build the project:

```bash
dotnet build
```

Run the server:

```bash
dotnet run
```

> Configuration and deployment instructions may vary depending on the current project setup.

---

## 📁 Project Structure

```text
CardGameTCPServer/
│
├── Services/
│   ├── AccountService
│   ├── DatabaseService
│   ├── HeartbeatMonitor
│   └── ...
│
├── TCP/
│   ├── ClientConnection
│   ├── TCPServer
│   └── ...
│
├── Packets/
│   ├── System Packets
│   ├── Game Packets
│   └── ...
│
├── Game/
│   ├── Game
│   ├── Match
│   ├── Player
│   └── ...
│
└── Program.cs
```

---

## 🛣️ Future Improvements

Potential future improvements include:

* Token-based authentication
* Improved cryptographic session tokens
* UDP networking for real-time movement-based games
* PostgreSQL integration
* Redis-based matchmaking
* Docker deployment
* Multiple game server instances
* Centralized matchmaking service
* Load balancing
* Automated deployment

These features are intentionally not implemented yet.

The goal of this project was to first understand the **fundamentals of multiplayer server architecture** before introducing production-scale infrastructure.

---

## 📚 Project Documentation

I documented the learning journey and architecture in a course-style handbook covering:

1. Networking Fundamentals
2. IPs and Ports
3. TCP and UDP
4. Sockets
5. TCP Server Architecture
6. Packet Protocols
7. Asynchronous Programming
8. Client Connection Management
9. Matchmaking
10. Authoritative Game Servers
11. Heartbeat Monitoring
12. Player Reconnection
13. Graceful Server Shutdown
14. Authentication and Player Accounts
15. SQLite Database Integration
16. Deployment and Shipping
17. Scaling Multiplayer Servers

---

## 🎯 Project Goal

The goal of this project was not to build the next massive multiplayer backend.

The goal was to understand:

> **How does a multiplayer server actually work?**

I started with a basic TCP server and progressively built the systems required to support a small authoritative multiplayer game.

This project is primarily a **learning project focused on server programming, networking and multiplayer backend architecture**.

---

## 📄 License

This project is available for learning and experimentation.
