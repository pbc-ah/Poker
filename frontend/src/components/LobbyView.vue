<template>
  <div class="lobby-container">
    <div class="lobby-header">
      <h1 class="logo">
        <span class="logo-icon">?</span>
        <span class="logo-text">Poker Rooms</span>
        <span class="logo-icon">?</span>
      </h1>
      <p class="subtitle">Join a table or create your own</p>
    </div>

    <div class="lobby-content">
      <!-- Room List -->
      <div class="rooms-section">
        <div class="section-header">
          <h2>Available Rooms</h2>
          <button class="btn-primary" @click="showCreateRoom = true">
            <span class="btn-icon">+</span>
            Create Room
          </button>
        </div>

        <div v-if="loading" class="loading-state">
          <div class="spinner"></div>
          <p>Loading rooms...</p>
        </div>

        <div v-else-if="!rooms || rooms.length === 0" class="empty-state">
          <div class="empty-icon">??</div>
          <h3>No rooms available</h3>
          <p>Be the first to create a room!</p>
          <button class="btn-primary" @click="showCreateRoom = true">Create Room</button>
        </div>

        <div v-else class="rooms-grid">
          <div 
            v-for="room in rooms" 
            :key="room.id"
            class="room-card"
            :class="{ 'room-full': room.players && room.players.length >= 10 }"
          >
            <div class="room-header">
              <div class="room-title">
                <h3>Room #{{ room.id.substring(0, 8) }}</h3>
                <span 
                  class="room-status" 
                  :class="`status-${room.status}`"
                >
                  {{ room.status }}
                </span>
              </div>
              <div class="room-ante">
                <span class="ante-label">Ante</span>
                <span class="ante-value">{{ room.anteAmount }}¢</span>
              </div>
            </div>

            <div class="room-info">
              <div class="info-item">
                <span class="info-icon">??</span>
                <span>{{ room.players ? room.players.length : 0 }} / 10 Players</span>
              </div>
              <div class="info-item">
                <span class="info-icon">??</span>
                <span>Pot: {{ room.pot || 0 }}¢</span>
              </div>
            </div>

            <div class="room-actions">
              <button 
                v-if="selectedRoomId !== room.id && room.status === 'waiting'"
                class="btn-primary"
                @click="selectRoom(room.id)"
                :disabled="room.players && room.players.length >= 10"
              >
                Join Room
              </button>
              
              <div v-if="selectedRoomId === room.id" class="join-form">
                <div class="form-group">
                  <label for="playerName">Your Name</label>
                  <input 
                    id="playerName"
                    type="text" 
                    v-model="playerName"
                    placeholder="Enter your name"
                    @keyup.enter="joinRoom(room.id)"
                  />
                </div>
                <div class="form-group">
                  <label for="playerBalance">Starting Balance</label>
                  <input 
                    id="playerBalance"
                    type="number" 
                    v-model.number="playerBalance"
                    placeholder="Enter amount in cents"
                    :min="room.anteAmount * 10"
                    @keyup.enter="joinRoom(room.id)"
                  />
                </div>
                <div class="form-actions">
                  <button class="btn-success" @click="joinRoom(room.id)">
                    Confirm Join
                  </button>
                  <button class="btn-ghost" @click="selectedRoomId = null">
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create Room Modal -->
    <transition name="modal">
      <div v-if="showCreateRoom" class="modal-overlay" @click.self="showCreateRoom = false">
        <div class="modal-content">
          <div class="modal-header">
            <h2>Create New Room</h2>
            <button class="close-btn" @click="showCreateRoom = false">×</button>
          </div>
          
          <div class="modal-body">
            <div class="form-group">
              <label for="anteAmount">Starting Ante (in eurocents)</label>
              <input 
                id="anteAmount"
                type="number" 
                v-model.number="newRoomAnte"
                :min="5"
                :step="5"
                placeholder="Minimum 5¢"
              />
              <p class="form-hint">Must be a multiple of 5</p>
            </div>

            <div class="ante-presets">
              <button 
                v-for="preset in [5, 10, 25, 50, 100]"
                :key="preset"
                class="preset-btn"
                :class="{ 'active': newRoomAnte === preset }"
                @click="newRoomAnte = preset"
              >
                {{ preset }}¢
              </button>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-success" @click="createRoom" :disabled="!isValidAnte">
              Create Room
            </button>
            <button class="btn-ghost" @click="showCreateRoom = false">
              Cancel
            </button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
export default {
  name: 'LobbyView',
  props: {
    rooms: {
      type: Array,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      showCreateRoom: false,
      newRoomAnte: 10,
      selectedRoomId: null,
      playerName: '',
      playerBalance: 1000
    };
  },
  computed: {
    isValidAnte() {
      return this.newRoomAnte >= 5 && this.newRoomAnte % 5 === 0;
    }
  },
  methods: {
    selectRoom(roomId) {
      this.selectedRoomId = roomId;
    },
    async createRoom() {
      if (!this.isValidAnte) return;
      
      this.$emit('create-room', this.newRoomAnte);
      this.showCreateRoom = false;
      this.newRoomAnte = 10;
    },
    async joinRoom(roomId) {
      if (!this.playerName || !this.playerBalance) return;
      
      this.$emit('join-room', {
        roomId,
        playerName: this.playerName,
        playerBalance: this.playerBalance
      });
    }
  }
};
</script>

<style scoped>
.lobby-container {
  min-height: 100vh;
  padding: var(--space-8) var(--space-6);
  max-width: 1400px;
  margin: 0 auto;
}

.lobby-header {
  text-align: center;
  margin-bottom: var(--space-12);
  animation: fadeIn 0.6s ease-out;
}

.logo {
  font-size: var(--text-5xl);
  font-family: var(--font-display);
  color: var(--color-light);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-4);
  margin-bottom: var(--space-3);
  text-shadow: 0 4px 20px rgba(46, 204, 113, 0.3);
}

.logo-icon {
  color: var(--color-primary);
  animation: bounce 2s ease-in-out infinite;
}

.logo-icon:first-child {
  animation-delay: 0.2s;
}

.subtitle {
  font-size: var(--text-xl);
  color: var(--color-gray-400);
}

.lobby-content {
  animation: fadeIn 0.8s ease-out 0.2s both;
}

.rooms-section {
  background: rgba(255, 255, 255, 0.03);
  border-radius: var(--radius-2xl);
  padding: var(--space-8);
  border: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-8);
}

.section-header h2 {
  font-size: var(--text-3xl);
  margin: 0;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: var(--space-12) var(--space-6);
}

.spinner {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(255, 255, 255, 0.1);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  margin: 0 auto var(--space-4);
  animation: spin 1s linear infinite;
}

.empty-icon {
  font-size: 5rem;
  margin-bottom: var(--space-4);
  opacity: 0.5;
}

.empty-state h3 {
  font-size: var(--text-2xl);
  margin-bottom: var(--space-2);
}

.empty-state p {
  color: var(--color-gray-400);
  margin-bottom: var(--space-6);
}

.rooms-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--space-6);
}

.room-card {
  background: rgba(255, 255, 255, 0.05);
  border-radius: var(--radius-xl);
  padding: var(--space-6);
  border: 2px solid rgba(255, 255, 255, 0.1);
  transition: all var(--transition-base);
  animation: fadeIn 0.5s ease-out;
}

.room-card:hover {
  border-color: var(--color-primary);
  box-shadow: 0 0 30px rgba(46, 204, 113, 0.3);
  transform: translateY(-4px);
}

.room-card.room-full {
  opacity: 0.6;
  filter: grayscale(50%);
}

.room-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--space-4);
  padding-bottom: var(--space-4);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.room-title h3 {
  font-size: var(--text-xl);
  margin: 0 0 var(--space-2) 0;
  color: var(--color-light);
}

.room-status {
  display: inline-block;
  padding: var(--space-1) var(--space-3);
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.status-waiting {
  background: var(--color-warning);
  color: var(--color-dark);
}

.status-started {
  background: var(--color-success);
  color: var(--color-light);
}

.room-ante {
  text-align: right;
}

.ante-label {
  display: block;
  font-size: var(--text-xs);
  color: var(--color-gray-400);
  margin-bottom: var(--space-1);
}

.ante-value {
  font-size: var(--text-2xl);
  font-weight: 800;
  font-family: var(--font-mono);
  color: var(--color-primary);
}

.room-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}

.info-item {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  color: var(--color-gray-300);
}

.info-icon {
  font-size: var(--text-lg);
}

.room-actions {
  margin-top: var(--space-4);
}

.join-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  animation: slideIn 0.3s ease-out;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.form-group label {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-gray-300);
}

.form-hint {
  font-size: var(--text-xs);
  color: var(--color-gray-500);
  margin-top: var(--space-1);
}

.form-actions {
  display: flex;
  gap: var(--space-2);
}

.form-actions button {
  flex: 1;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  backdrop-filter: blur(5px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
  padding: var(--space-6);
}

.modal-content {
  background: var(--color-dark);
  border-radius: var(--radius-2xl);
  max-width: 500px;
  width: 100%;
  border: 2px solid rgba(255, 255, 255, 0.1);
  box-shadow: var(--shadow-2xl);
  animation: fadeIn 0.3s ease-out;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-6);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-header h2 {
  margin: 0;
  font-size: var(--text-2xl);
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

.modal-body {
  padding: var(--space-6);
}

.ante-presets {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: var(--space-2);
  margin-top: var(--space-4);
}

.preset-btn {
  padding: var(--space-3);
  background: rgba(255, 255, 255, 0.05);
  border: 2px solid rgba(255, 255, 255, 0.2);
  color: var(--color-light);
  border-radius: var(--radius-lg);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-base);
}

.preset-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: var(--color-primary);
}

.preset-btn.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  box-shadow: 0 0 20px rgba(46, 204, 113, 0.4);
}

.modal-footer {
  padding: var(--space-6);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  gap: var(--space-3);
}

.modal-footer button {
  flex: 1;
}

/* Modal Transition */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-active .modal-content,
.modal-leave-active .modal-content {
  transition: transform 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-content,
.modal-leave-to .modal-content {
  transform: scale(0.9);
}

/* Responsive Design */
@media (max-width: 768px) {
  .lobby-container {
    padding: var(--space-4);
  }
  
  .rooms-grid {
    grid-template-columns: 1fr;
  }
  
  .section-header {
    flex-direction: column;
    gap: var(--space-4);
    align-items: flex-start;
  }
  
  .section-header button {
    width: 100%;
  }
  
  .ante-presets {
    grid-template-columns: repeat(3, 1fr);
  }
}
</style>
