# The Silver Slayed - Luxury Casino Web App

A premium Vegas-style online casino web application built with React, featuring a luxury silver and gold theme with light/dark mode support.

## 🎨 Features

- **Luxury Design**: Premium silver/gold themed UI with metallic gradients and glows
- **Light/Dark Mode**: Fully supports both themes with smooth transitions
- **Authentication**: Login and registration with welcome bonus
- **Four Casino Games**:
  - 🎰 **Slots**: Classic slot machine with metallic reels
  - 🃏 **Blackjack**: Beat the dealer to 21
  - ♠️ **Poker**: Simplified Texas Hold'em
  - 🎡 **Roulette**: European roulette with inside and outside bets
- **Balance System**: Track your virtual currency across games
- **Responsive Design**: Works seamlessly on desktop and mobile

## 🚀 Getting Started

### Prerequisites

- Node.js (v16 or higher)
- npm or yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Open your browser and navigate to `http://localhost:3000`

## 🎮 How to Play

1. **Register** for a new account and receive a $15,000 welcome bonus
2. **Login** to access the casino lobby
3. **Choose a game** from the lobby
4. **Place your bets** and start playing
5. **Track your balance** in the header

## 🛠️ Technology Stack

- **React 18**: Frontend framework
- **React Router**: Navigation and routing
- **Context API**: State management (Auth, Theme)
- **CSS3**: Custom styling with CSS variables for theming
- **Vite**: Build tool and dev server

## 📁 Project Structure

```
CasinoProject/
├── src/
│   ├── components/
│   │   ├── common/          # Reusable UI components
│   │   └── layout/          # Layout components (Header)
│   ├── context/             # React contexts (Auth, Theme)
│   ├── pages/
│   │   ├── Auth/            # Login/Register pages
│   │   ├── Lobby/           # Game lobby
│   │   └── Games/           # Individual game pages
│   ├── styles/              # Global styles and theme
│   ├── App.jsx              # Main app component
│   └── main.jsx             # Entry point
├── index.html
├── vite.config.js
└── package.json
```

## 🎨 Theme Customization

The app uses CSS variables for theming. You can customize colors in `src/styles/theme.css`:

- **Light Mode**: Silver, off-white, platinum surfaces
- **Dark Mode**: Deep blacks, charcoals, gold accents
- **Brand Colors**: Gold (#d4af37), Silver (#c0c0c0), Platinum (#e5e4e2)

## 🔐 Authentication

Currently uses mock authentication with local storage. In production, this will be replaced with a C# .NET backend API.

## 🎰 Game Logic

All games currently use client-side logic with random number generation. Backend integration with C# .NET is planned for:
- Secure game logic
- Persistent balance tracking
- Transaction history
- Real-time multiplayer features

## 📱 Responsive Design

The app is fully responsive and optimized for:
- Desktop (1920px and above)
- Tablet (768px - 1919px)
- Mobile (320px - 767px)

## 🚧 Future Enhancements

- C# .NET backend integration
- Real money transactions
- Multiplayer poker tables
- Live dealer games
- Tournament system
- Leaderboards
- Social features

## 📄 License

This project is for demonstration purposes only.

## 🤝 Contributing

This is a personal project, but suggestions and feedback are welcome!

---

**Built with ❤️ and React**
