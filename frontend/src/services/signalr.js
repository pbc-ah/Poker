import * as signalR from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 10;
    this.callbacks = {
      onGameStateUpdate: null,
      onLobbyUpdate: null,
      onConnectionChange: null
    };
  }

  async connect(baseUrl) {
    if (this.connection) {
      await this.disconnect();
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}gameHub`, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < 5) {
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
          return null; // Stop reconnecting after 5 attempts
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('GameStateUpdate', (gameState) => {
      if (this.callbacks.onGameStateUpdate) {
        this.callbacks.onGameStateUpdate(gameState);
      }
    });

    this.connection.on('LobbyUpdate', (rooms) => {
      if (this.callbacks.onLobbyUpdate) {
        this.callbacks.onLobbyUpdate(rooms);
      }
    });

    this.connection.onclose(() => {
      this.isConnected = false;
      if (this.callbacks.onConnectionChange) {
        this.callbacks.onConnectionChange(false);
      }
    });

    this.connection.onreconnecting(() => {
      this.isConnected = false;
      if (this.callbacks.onConnectionChange) {
        this.callbacks.onConnectionChange(false);
      }
    });

    this.connection.onreconnected(() => {
      this.isConnected = true;
      this.reconnectAttempts = 0;
      if (this.callbacks.onConnectionChange) {
        this.callbacks.onConnectionChange(true);
      }
    });

    try {
      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;
      if (this.callbacks.onConnectionChange) {
        this.callbacks.onConnectionChange(true);
      }
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      this.isConnected = false;
      throw err;
    }
  }

  async disconnect() {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
        console.error('Error disconnecting SignalR:', err);
      }
      this.connection = null;
      this.isConnected = false;
    }
  }

  async joinGameRoom(gameId, playerId) {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('JoinGameRoom', gameId, playerId);
    }
  }

  async leaveGameRoom(gameId, playerId) {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('LeaveGameRoom', gameId, playerId);
    }
  }

  async joinLobby() {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('JoinLobby');
    }
  }

  async leaveLobby() {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('LeaveLobby');
    }
  }

  onGameStateUpdate(callback) {
    this.callbacks.onGameStateUpdate = callback;
  }

  onLobbyUpdate(callback) {
    this.callbacks.onLobbyUpdate = callback;
  }

  onConnectionChange(callback) {
    this.callbacks.onConnectionChange = callback;
  }
}

export default new SignalRService();
