<template>
  <div class="poker-table-container">
    <div class="poker-table">
      <div class="table-felt">
        <!-- Community Cards Area -->
        <div class="community-cards-area">
          <transition-group name="card-deal" tag="div" class="community-cards">
            <div 
              v-for="(card, index) in communityCards" 
              :key="`card-${card}-${index}`"
              class="playing-card"
              :style="{ transitionDelay: `${index * 0.1}s` }"
            >
              <img :src="`/cards/${card}.svg`" :alt="card" />
            </div>
          </transition-group>
        </div>

        <!-- Pot Display -->
        <div class="pot-display">
          <div class="pot-container">
            <div class="chips-stack">
              <div class="chip chip-red"></div>
              <div class="chip chip-blue"></div>
              <div class="chip chip-green"></div>
            </div>
            <div class="pot-info">
              <div class="pot-label">POT</div>
              <div class="pot-amount">{{ formatChips(pot) }}</div>
            </div>
          </div>
          <div v-if="currentBet" class="current-bet">
            <span class="bet-label">Current Bet:</span>
            <span class="bet-amount">{{ formatChips(currentBet) }}</span>
          </div>
        </div>

        <!-- Player Positions (seats around table) -->
        <div class="player-seats">
          <div 
            v-for="(player, index) in allPlayers" 
            :key="player.id"
            :class="['player-seat', `seat-${index}`, { 
              'active': isCurrentTurn(player),
              'folded': player.isFolded,
              'all-in': player.isAllIn
            }]"
          >
            <div class="player-info-card">
              <div class="player-avatar">
                <div class="avatar-circle">
                  {{ getInitials(player.name) }}
                </div>
                <div v-if="isCurrentTurn(player)" class="turn-indicator"></div>
              </div>
              <div class="player-details">
                <div class="player-name">{{ player.name }}</div>
                <div class="player-balance">{{ formatChips(player.balance) }}</div>
                <div v-if="player.isFolded" class="player-status status-folded">FOLDED</div>
                <div v-else-if="player.isAllIn" class="player-status status-all-in">ALL IN</div>
                <div v-else-if="gameStatus === 'waiting'" class="player-status">
                  <span v-if="player.isReady" class="status-ready">READY</span>
                  <span v-else class="status-waiting">WAITING</span>
                </div>
              </div>
            </div>
            
            <!-- Player's cards (visible only to current player or during showdown) -->
            <transition name="card-reveal">
              <div v-if="showPlayerCards(player)" class="player-hand">
                <div 
                  v-for="(card, cardIndex) in player.hand" 
                  :key="`${player.id}-${card}-${cardIndex}`"
                  class="playing-card mini"
                >
                  <img :src="`/cards/${card}.svg`" :alt="card" />
                </div>
              </div>
            </transition>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'PokerTable',
  props: {
    communityCards: {
      type: Array,
      default: () => []
    },
    pot: {
      type: Number,
      default: 0
    },
    currentBet: {
      type: Number,
      default: 0
    },
    allPlayers: {
      type: Array,
      default: () => []
    },
    currentTurnId: {
      type: String,
      default: ''
    },
    currentPlayerId: {
      type: String,
      default: ''
    },
    gameStatus: {
      type: String,
      default: 'waiting'
    },
    showResults: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    formatChips(amount) {
      return `${amount}¢`;
    },
    getInitials(name) {
      return name
        .split(' ')
        .map(word => word[0])
        .join('')
        .toUpperCase()
        .substring(0, 2);
    },
    isCurrentTurn(player) {
      return player.id === this.currentTurnId;
    },
    showPlayerCards(player) {
      return (
        (player.id === this.currentPlayerId) || 
        (this.showResults && player.hand && player.hand.length > 0 && !player.isFolded)
      );
    }
  }
};
</script>

<style scoped>
.poker-table-container {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: var(--space-8);
}

.poker-table {
  perspective: 1000px;
}

.table-felt {
  background: radial-gradient(ellipse at center, var(--color-felt-green) 0%, var(--color-felt-border) 100%);
  border-radius: 50% / 40%;
  padding: var(--space-12);
  min-height: 600px;
  position: relative;
  box-shadow: 
    inset 0 0 100px rgba(0, 0, 0, 0.5),
    0 0 0 20px var(--color-table-rail),
    0 0 0 25px var(--color-table-wood),
    var(--shadow-2xl);
  border: 10px solid var(--color-felt-border);
}

/* Community Cards */
.community-cards-area {
  position: absolute;
  top: 30%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: var(--z-base);
}

.community-cards {
  display: flex;
  gap: var(--space-3);
  justify-content: center;
  min-height: 140px;
}

.playing-card {
  width: 90px;
  height: 126px;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  transition: all var(--transition-base);
  cursor: pointer;
  background: white;
  overflow: hidden;
}

.playing-card:hover {
  transform: translateY(-10px) scale(1.05);
  box-shadow: var(--shadow-card-hover);
}

.playing-card img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.playing-card.mini {
  width: 50px;
  height: 70px;
}

/* Card Deal Animation */
.card-deal-enter-active {
  animation: cardDeal 0.5s ease-out;
}

@keyframes cardDeal {
  0% {
    opacity: 0;
    transform: translateY(-100px) rotateX(-90deg);
  }
  100% {
    opacity: 1;
    transform: translateY(0) rotateX(0);
  }
}

/* Card Reveal Animation */
.card-reveal-enter-active, .card-reveal-leave-active {
  transition: all 0.3s ease;
}

.card-reveal-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}

.card-reveal-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

/* Pot Display */
.pot-display {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
  z-index: var(--z-base);
}

.pot-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-3);
}

.chips-stack {
  position: relative;
  width: 60px;
  height: 30px;
}

.chip {
  position: absolute;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  border: 4px solid rgba(255, 255, 255, 0.3);
  box-shadow: 
    0 4px 8px rgba(0, 0, 0, 0.3),
    inset 0 2px 4px rgba(255, 255, 255, 0.3);
}

.chip-red {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-red), #c0392b);
  left: 0;
  top: 0;
  z-index: 3;
}

.chip-blue {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-blue), #2980b9);
  left: 5px;
  top: -5px;
  z-index: 2;
}

.chip-green {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-green), #27ae60);
  left: 10px;
  top: -10px;
  z-index: 1;
}

.pot-info {
  background: rgba(0, 0, 0, 0.6);
  padding: var(--space-4) var(--space-6);
  border-radius: var(--radius-xl);
  backdrop-filter: blur(10px);
  border: 2px solid rgba(255, 215, 0, 0.3);
  box-shadow: 0 0 20px rgba(255, 215, 0, 0.2);
}

.pot-label {
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--color-warning);
  letter-spacing: 0.1em;
  margin-bottom: var(--space-1);
}

.pot-amount {
  font-size: var(--text-3xl);
  font-weight: 800;
  font-family: var(--font-mono);
  color: var(--color-light);
  text-shadow: 0 0 10px rgba(255, 215, 0, 0.5);
}

.current-bet {
  margin-top: var(--space-4);
  padding: var(--space-2) var(--space-4);
  background: rgba(0, 0, 0, 0.4);
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
}

.bet-label {
  color: var(--color-gray-400);
  margin-right: var(--space-2);
}

.bet-amount {
  font-weight: 700;
  color: var(--color-primary);
  font-family: var(--font-mono);
}

/* Player Seats */
.player-seats {
  position: relative;
  width: 100%;
  height: 100%;
}

.player-seat {
  position: absolute;
  transition: all var(--transition-base);
}

/* Seat positions (circular layout) */
.seat-0 { bottom: -80px; left: 50%; transform: translateX(-50%); }
.seat-1 { bottom: 10%; left: 10%; }
.seat-2 { top: 30%; left: 5%; }
.seat-3 { top: 5%; left: 25%; }
.seat-4 { top: 5%; right: 25%; }
.seat-5 { top: 30%; right: 5%; }
.seat-6 { bottom: 10%; right: 10%; }

.player-info-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: var(--radius-xl);
  padding: var(--space-4);
  border: 2px solid rgba(255, 255, 255, 0.2);
  box-shadow: var(--shadow-lg);
  transition: all var(--transition-base);
  min-width: 180px;
}

.player-seat.active .player-info-card {
  border-color: var(--color-primary);
  box-shadow: 0 0 30px rgba(46, 204, 113, 0.6), var(--shadow-xl);
  animation: pulse 2s ease-in-out infinite;
}

.player-seat.folded .player-info-card {
  opacity: 0.4;
  filter: grayscale(100%);
}

.player-avatar {
  position: relative;
  display: inline-block;
  margin-bottom: var(--space-3);
}

.avatar-circle {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xl);
  font-weight: 700;
  color: var(--color-light);
  box-shadow: var(--shadow-md);
}

.turn-indicator {
  position: absolute;
  top: -5px;
  right: -5px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--color-success);
  border: 3px solid var(--color-light);
  box-shadow: 0 0 15px var(--color-success);
  animation: bounce 1s ease-in-out infinite;
}

.player-details {
  text-align: center;
}

.player-name {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-light);
  margin-bottom: var(--space-2);
}

.player-balance {
  font-size: var(--text-lg);
  font-weight: 800;
  font-family: var(--font-mono);
  color: var(--color-warning);
  margin-bottom: var(--space-2);
}

.player-status {
  font-size: var(--text-xs);
  font-weight: 700;
  padding: var(--space-1) var(--space-3);
  border-radius: var(--radius-full);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.status-ready {
  background: var(--color-success);
  color: var(--color-light);
  padding: var(--space-1) var(--space-3);
  border-radius: var(--radius-full);
}

.status-waiting {
  background: var(--color-warning);
  color: var(--color-dark);
  padding: var(--space-1) var(--space-3);
  border-radius: var(--radius-full);
}

.status-folded {
  background: var(--color-danger);
  color: var(--color-light);
}

.status-all-in {
  background: var(--color-accent);
  color: var(--color-light);
  animation: pulse 1.5s ease-in-out infinite;
}

.player-hand {
  display: flex;
  gap: var(--space-2);
  margin-top: var(--space-3);
  justify-content: center;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .poker-table-container {
    padding: var(--space-4);
  }
  
  .table-felt {
    padding: var(--space-8);
    min-height: 500px;
  }
  
  .playing-card {
    width: 70px;
    height: 98px;
  }
  
  .player-info-card {
    min-width: 140px;
    padding: var(--space-3);
  }
}

@media (max-width: 768px) {
  .table-felt {
    border-radius: var(--radius-3xl);
    min-height: 400px;
  }
  
  .community-cards {
    gap: var(--space-2);
  }
  
  .playing-card {
    width: 50px;
    height: 70px;
  }
}
</style>
