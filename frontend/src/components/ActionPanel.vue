<template>
  <div class="action-panel" :class="{ 'disabled': !isPlayerTurn }">
    <transition name="slide-up">
      <div v-if="isPlayerTurn && !showRaiseInput" class="action-buttons">
        <button 
          class="action-btn btn-danger"
          @click="$emit('action', { type: 'fold' })"
        >
          <span class="btn-icon">??</span>
          <span class="btn-text">Fold</span>
        </button>
        
        <button 
          v-if="callAmount === 0"
          class="action-btn btn-secondary"
          @click="$emit('action', { type: 'check' })"
        >
          <span class="btn-icon">?</span>
          <span class="btn-text">Check</span>
        </button>
        
        <button 
          v-if="callAmount > 0"
          class="action-btn btn-primary"
          @click="$emit('action', { type: 'call' })"
        >
          <span class="btn-icon">??</span>
          <span class="btn-text">Call {{ formatChips(callAmount) }}</span>
        </button>
        
        <button 
          class="action-btn btn-success"
          @click="showRaiseInput = true"
        >
          <span class="btn-icon">??</span>
          <span class="btn-text">{{ callAmount > 0 ? 'Raise' : 'Bet' }}</span>
        </button>
      </div>
    </transition>

    <transition name="slide-up">
      <div v-if="isPlayerTurn && showRaiseInput" class="raise-panel">
        <div class="raise-header">
          <h3>{{ callAmount > 0 ? 'Raise Amount' : 'Bet Amount' }}</h3>
          <button class="close-btn" @click="cancelRaise">×</button>
        </div>
        
        <div class="raise-controls">
          <div class="amount-input-group">
            <label for="raiseAmount">Total Bet Amount</label>
            <div class="input-with-buttons">
              <button 
                class="amount-adjust-btn"
                @click="adjustAmount(-10)"
                :disabled="raiseAmount <= minRaise"
              >
                -10
              </button>
              <input 
                id="raiseAmount"
                type="number" 
                v-model.number="raiseAmount"
                :min="minRaise"
                :max="maxBet"
                class="amount-input"
              />
              <button 
                class="amount-adjust-btn"
                @click="adjustAmount(10)"
                :disabled="raiseAmount >= maxBet"
              >
                +10
              </button>
            </div>
            <div class="bet-hint" v-if="callAmount > 0">
              You'll add {{ formatChips(raiseAmount - playerCurrentBet) }} more
            </div>
          </div>
          
          <div class="quick-bet-buttons">
            <button 
              class="quick-bet-btn"
              @click="setQuickBet(minRaise)"
              :disabled="maxBet < minRaise"
            >
              Min
            </button>
            <button 
              class="quick-bet-btn"
              @click="setQuickBet(Math.min(currentBet + Math.floor(pot / 2), maxBet))"
              :disabled="maxBet < minRaise"
            >
              1/2 Pot
            </button>
            <button 
              class="quick-bet-btn"
              @click="setQuickBet(Math.min(currentBet + pot, maxBet))"
              :disabled="maxBet < currentBet + pot"
            >
              Pot
            </button>
            <button 
              class="quick-bet-btn"
              @click="setQuickBet(maxBet)"
            >
              All-In
            </button>
          </div>
          
          <div class="raise-info">
            <div class="info-row">
              <span>Your Balance:</span>
              <span class="info-value">{{ formatChips(playerBalance) }}</span>
            </div>
            <div class="info-row" v-if="playerCurrentBet > 0">
              <span>Already Bet:</span>
              <span class="info-value">{{ formatChips(playerCurrentBet) }}</span>
            </div>
            <div class="info-row">
              <span>Additional Cost:</span>
              <span class="info-value">{{ formatChips(raiseAmount - playerCurrentBet) }}</span>
            </div>
            <div class="info-row">
              <span>After Bet:</span>
              <span class="info-value">{{ formatChips(playerBalance - (raiseAmount - playerCurrentBet)) }}</span>
            </div>
          </div>
          
          <button 
            class="confirm-btn btn-success"
            @click="confirmRaise"
            :disabled="!isValidRaise"
          >
            Confirm {{ callAmount > 0 ? 'Raise' : 'Bet' }}
          </button>
        </div>
      </div>
    </transition>

    <div v-if="!isPlayerTurn" class="waiting-indicator">
      <div class="spinner"></div>
      <p>Waiting for other players...</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ActionPanel',
  props: {
    isPlayerTurn: {
      type: Boolean,
      default: false
    },
    currentBet: {
      type: Number,
      default: 0
    },
    playerCurrentBet: {
      type: Number,
      default: 0
    },
    playerBalance: {
      type: Number,
      default: 0
    },
    pot: {
      type: Number,
      default: 0
    }
  },
  data() {
    return {
      showRaiseInput: false,
      raiseAmount: 10
    };
  },
  computed: {
    callAmount() {
      return this.currentBet - this.playerCurrentBet;
    },
    minRaise() {
      return Math.max(this.currentBet + 10, 10);
    },
    maxBet() {
      return this.playerCurrentBet + this.playerBalance;
    },
    isValidRaise() {
      return this.raiseAmount >= this.minRaise && 
             this.raiseAmount <= this.maxBet &&
             (this.raiseAmount - this.playerCurrentBet) <= this.playerBalance;
    }
  },
  watch: {
    isPlayerTurn(newVal) {
      if (newVal) {
        this.showRaiseInput = false;
        this.raiseAmount = this.minRaise;
      }
    }
  },
  methods: {
    formatChips(amount) {
      return `${amount}¢`;
    },
    adjustAmount(delta) {
      const newAmount = this.raiseAmount + delta;
      if (newAmount >= this.minRaise && newAmount <= this.maxBet) {
        this.raiseAmount = newAmount;
      }
    },
    setQuickBet(amount) {
      this.raiseAmount = Math.min(Math.max(amount, this.minRaise), this.maxBet);
    },
    confirmRaise() {
      if (this.isValidRaise) {
        this.$emit('action', {
          type: 'bet',
          amount: this.raiseAmount
        });
        this.showRaiseInput = false;
        this.raiseAmount = this.minRaise;
      }
    },
    cancelRaise() {
      this.showRaiseInput = false;
      this.raiseAmount = this.minRaise;
    }
  }
};
</script>

<style scoped>
.action-panel {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: rgba(0, 0, 0, 0.95);
  backdrop-filter: blur(20px);
  padding: var(--space-6);
  border-top: 2px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 -10px 40px rgba(0, 0, 0, 0.5);
  z-index: var(--z-fixed);
  transition: all var(--transition-base);
}

.action-panel.disabled {
  opacity: 0.6;
  pointer-events: none;
}

.action-buttons {
  display: flex;
  gap: var(--space-4);
  justify-content: center;
  max-width: 800px;
  margin: 0 auto;
}

.action-btn {
  flex: 1;
  min-width: 120px;
  padding: var(--space-4) var(--space-6);
  font-size: var(--text-lg);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-2);
}

.btn-icon {
  font-size: var(--text-3xl);
}

.btn-text {
  font-size: var(--text-sm);
  font-weight: 700;
}

.raise-panel {
  max-width: 600px;
  margin: 0 auto;
  background: rgba(255, 255, 255, 0.05);
  border-radius: var(--radius-2xl);
  padding: var(--space-6);
  border: 2px solid rgba(255, 255, 255, 0.1);
}

.raise-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-6);
}

.raise-header h3 {
  font-size: var(--text-2xl);
  color: var(--color-light);
  margin: 0;
}

.close-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  border: none;
  font-size: var(--text-3xl);
  color: var(--color-light);
  cursor: pointer;
  transition: all var(--transition-base);
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}

.close-btn:hover {
  background: var(--color-danger);
  transform: rotate(90deg);
}

.raise-controls {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.amount-input-group label {
  display: block;
  margin-bottom: var(--space-3);
  color: var(--color-gray-200);
  font-weight: 600;
}

.bet-hint {
  margin-top: var(--space-2);
  font-size: var(--text-sm);
  color: var(--color-primary);
  font-weight: 600;
  text-align: center;
}

.input-with-buttons {
  display: flex;
  gap: var(--space-2);
  align-items: center;
}

.amount-adjust-btn {
  width: 60px;
  padding: var(--space-3);
  background: rgba(255, 255, 255, 0.1);
  border: 2px solid rgba(255, 255, 255, 0.2);
  color: var(--color-light);
  border-radius: var(--radius-lg);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-base);
}

.amount-adjust-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.2);
  border-color: var(--color-primary);
}

.amount-adjust-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.amount-input {
  flex: 1;
  text-align: center;
  font-size: var(--text-2xl);
  font-weight: 800;
  font-family: var(--font-mono);
  padding: var(--space-4);
}

.quick-bet-buttons {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-2);
}

.quick-bet-btn {
  padding: var(--space-3);
  background: rgba(255, 255, 255, 0.05);
  border: 2px solid rgba(255, 255, 255, 0.2);
  color: var(--color-light);
  border-radius: var(--radius-lg);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-base);
}

.quick-bet-btn:hover:not(:disabled) {
  background: var(--color-primary);
  border-color: var(--color-primary);
  transform: translateY(-2px);
}

.quick-bet-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.raise-info {
  background: rgba(0, 0, 0, 0.3);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.info-row {
  display: flex;
  justify-content: space-between;
  font-size: var(--text-base);
  color: var(--color-gray-300);
}

.info-value {
  font-weight: 700;
  font-family: var(--font-mono);
  color: var(--color-primary);
}

.confirm-btn {
  width: 100%;
  padding: var(--space-4);
  font-size: var(--text-lg);
}

.waiting-indicator {
  text-align: center;
  padding: var(--space-4);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(255, 255, 255, 0.1);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  margin: 0 auto var(--space-3);
  animation: spin 1s linear infinite;
}

.waiting-indicator p {
  color: var(--color-gray-400);
  font-size: var(--text-base);
  margin: 0;
}

/* Animations */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from {
  transform: translateY(100%);
  opacity: 0;
}

.slide-up-leave-to {
  transform: translateY(100%);
  opacity: 0;
}

/* Responsive Design */
@media (max-width: 768px) {
  .action-buttons {
    flex-wrap: wrap;
  }
  
  .action-btn {
    min-width: calc(50% - var(--space-2));
  }
  
  .quick-bet-buttons {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
