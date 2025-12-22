// API Configuration
// Switch between ports for testing:
// - Port 5001: dotnet run (command line)
// - Port 44331: IIS Express (Visual Studio)

const API_PORTS = {
  DOTNET_RUN: 5001,
  IIS_EXPRESS: 44331
};

// Change this to switch between ports
const CURRENT_PORT = API_PORTS.IIS_EXPRESS; // or API_PORTS.DOTNET_RUN

export const API_BASE_URL = `https://localhost:${CURRENT_PORT}/api`;

export const API_ENDPOINTS = {
  AUTH: `${API_BASE_URL}/auth`,
  WALLET: `${API_BASE_URL}/wallet`,
  GAMESTATE: `${API_BASE_URL}/gamestate`,
  ADMIN: `${API_BASE_URL}/admin`,
  BLACKJACK: `${API_BASE_URL}/blackjack`
};
