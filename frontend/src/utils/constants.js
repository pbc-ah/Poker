// Game Status
export const GAME_STATUS = {
  WAITING: 'waiting',
  STARTED: 'started',
  FINISHED: 'finished'
};

// Player Actions
export const PLAYER_ACTIONS = {
  FOLD: 'fold',
  CHECK: 'check',
  CALL: 'call',
  BET: 'bet',
  RAISE: 'raise'
};

// Betting Rounds
export const BETTING_ROUNDS = {
  PRE_FLOP: 0,
  FLOP: 1,
  TURN: 2,
  RIVER: 3
};

// Game Limits
export const GAME_LIMITS = {
  MIN_PLAYERS: 2,
  MAX_PLAYERS: 10,
  MIN_ANTE: 5,
  ANTE_INCREMENT: 5,
  POLL_INTERVAL: 750, // ms
  ROOM_REFRESH_INTERVAL: 3000 // ms
};

// Card Suits
export const SUITS = {
  HEARTS: 'H',
  DIAMONDS: 'D',
  CLUBS: 'C',
  SPADES: 'S'
};

// Card Ranks
export const RANKS = {
  TWO: '2',
  THREE: '3',
  FOUR: '4',
  FIVE: '5',
  SIX: '6',
  SEVEN: '7',
  EIGHT: '8',
  NINE: '9',
  TEN: 'T',
  JACK: 'J',
  QUEEN: 'Q',
  KING: 'K',
  ACE: 'A'
};

// Hand Rankings
export const HAND_RANKINGS = {
  HIGH_CARD: 0,
  ONE_PAIR: 1,
  TWO_PAIR: 2,
  THREE_OF_A_KIND: 3,
  STRAIGHT: 4,
  FLUSH: 5,
  FULL_HOUSE: 6,
  FOUR_OF_A_KIND: 7,
  STRAIGHT_FLUSH: 8,
  ROYAL_FLUSH: 9
};

// UI Animations
export const ANIMATION_DURATION = {
  FAST: 150,
  BASE: 200,
  SLOW: 300,
  CARD_DEAL: 500
};

// Notification Types
export const NOTIFICATION_TYPES = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info'
};

// Routes
export const ROUTES = {
  LOBBY: '/',
  GAME: '/game'
};

// Local Storage Keys
export const STORAGE_KEYS = {
  GAME_STATE: 'gameState',
  PLAYER_PREFERENCES: 'playerPreferences',
  SOUND_ENABLED: 'soundEnabled',
  ANIMATIONS_ENABLED: 'animationsEnabled'
};
