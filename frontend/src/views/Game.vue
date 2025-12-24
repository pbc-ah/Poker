<template>
	<div class="game-page">
		<ConnectionStatus :isConnected="isConnected" />
		
		<div class="game-container" v-if="getGameRoom">
			<!-- Waiting Screen -->
			<template v-if="getGameRoom.status === 'waiting'">
				<div class="waiting-screen">
					<div class="waiting-content">
						<template v-if="hasCompletedRound">
							<h2 class="waiting-title">Round Complete!</h2>
							
							<!-- Winner Announcement -->
							<div v-if="roundResult" class="winner-announcement">
								<div class="trophy-icon">??</div>
								<h3 class="winner-title">
									{{ roundResult.winners.length === 1 ? 'Winner' : 'Winners' }}
								</h3>
								<div class="winners-list">
									<div 
										v-for="winner in roundResult.winners" 
										:key="`winner-${winner.playerId}`"
										class="winner-card"
										:class="{ 'is-current-player': winner.playerId === getGameRoom.player.id }"
									>
										<div class="winner-name">{{ winner.playerName }}</div>
										<div class="winner-hand">{{ winner.handName }}</div>
										<div class="winner-amount">Won {{ formatChips(winner.amountWon) }}</div>
									</div>
								</div>
								<div class="pot-total">
									Total Pot: {{ formatChips(roundResult.totalPot) }}
								</div>
							</div>

							<p class="waiting-subtitle">Waiting for players to ready up for next round...</p>
							
							<div class="waiting-actions">
								<button 
									v-if="!showingResults" 
									class="btn-secondary"
									@click="showingResults = true"
								>
									Show All Cards
								</button>
								<button 
									v-if="!getGameRoom?.player?.isReady" 
									class="btn-success"
									@click="triggerReady"
								>
									I'm Ready
								</button>
							</div>

							<transition name="fade">
								<div v-if="showingResults" class="results-display">
									<h3>Community Cards</h3>
									<div class="community-cards-display">
										<img 
											v-for="(card, index) in getGameRoom.communityCards" 
											:key="`result-${card}-${index}`"
											:src="'/cards/' + card + '.svg'" 
											class="result-card"
										/>
									</div>

									<div class="players-results">
										<template v-for="playerInfo in allPlayers" :key="`result-player-${playerInfo.id}`">
											<div 
												v-if="playerInfo.hand && playerInfo.hand.length > 0"
												class="player-result-card"
												:class="{ 'winner': isWinner(playerInfo.id) }"
											>
												<div class="player-result-header">
													<h4>{{ playerInfo.name }}</h4>
													<span v-if="isWinner(playerInfo.id)" class="winner-badge">?? Winner</span>
												</div>
												<div class="player-result-hand">
													<img 
														v-for="(card, index) in playerInfo.hand" 
														:key="`player-card-${playerInfo.id}-${card}-${index}`"
														:src="'/cards/' + card + '.svg'" 
														class="result-card-mini"
													/>
												</div>
												<div v-if="getWinnerInfo(playerInfo.id)" class="winner-details">
													<div class="hand-name">{{ getWinnerInfo(playerInfo.id).handName }}</div>
													<div class="amount-won">+{{ formatChips(getWinnerInfo(playerInfo.id).amountWon) }}</div>
												</div>
											</div>
										</template>
									</div>
								</div>
							</transition>
						</template>

						<template v-else>
							<div class="pre-game-waiting">
								<h2 class="waiting-title">Waiting to Start</h2>
								<p class="waiting-subtitle">Waiting for all players to ready up...</p>
								
								<div class="players-ready-list">
									<div 
										v-for="playerInfo in allPlayers"
										:key="`ready-${playerInfo.id}`"
										class="ready-player"
										:class="{ 'is-ready': playerInfo.isReady }"
									>
										<span class="ready-icon">{{ playerInfo.isReady ? '?' : '?' }}</span>
										<span class="ready-name">{{ playerInfo.name }}</span>
									</div>
								</div>

								<button 
									v-if="!getGameRoom?.player?.isReady" 
									class="btn-success btn-large"
									@click="triggerReady"
								>
									I'm Ready to Play
								</button>
							</div>
						</template>
					</div>
				</div>
			</template>

			<!-- Active Game -->
			<template v-else>
				<PokerTable 
					:communityCards="getGameRoom.communityCards"
					:pot="getGameRoom.pot"
					:currentBet="getGameRoom.currentBet"
					:allPlayers="allPlayers"
					:currentTurnId="getGameRoom.currentTurn"
					:currentPlayerId="getGameRoom.player.id"
					:gameStatus="getGameRoom.status"
					:showResults="showingResults"
				/>

				<ActionPanel 
					:isPlayerTurn="getGameRoom.currentTurn === getGameRoom.player.id"
					:currentBet="getGameRoom.currentBet"
					:playerCurrentBet="getGameRoom.playerCurrentBet || 0"
					:playerBalance="getGameRoom.player.balance"
					:pot="getGameRoom.pot"
					@action="handleAction"
				/>
			</template>

			<!-- Exit Button -->
			<button class="exit-btn" @click="exitGame">
				? Back to Lobby
			</button>
		</div>

		<div v-else class="loading-screen">
			<div class="spinner-large"></div>
			<p>Loading game...</p>
		</div>
	</div>
</template>

<script>
import { mapActions, mapGetters } from "vuex";
import PokerTable from "../components/PokerTable.vue";
import ActionPanel from "../components/ActionPanel.vue";
import ConnectionStatus from "../components/ConnectionStatus.vue";
import signalRService from "../services/signalr";

export default {
	name: 'Game',
	components: {
		PokerTable,
		ActionPanel,
		ConnectionStatus
	},
	data() {
		return {
			showingResults: false,
			isConnected: false
		};
	},
	computed: {
		...mapGetters(['getGameRoom']),
		
		allPlayers() {
			if (!this.getGameRoom) return [];
			const otherPlayers = this.getGameRoom.otherPlayers || [];
			return [this.getGameRoom.player, ...otherPlayers];
		},
		
		hasCompletedRound() {
			if (!this.getGameRoom) return false;
			// Check if we have round results (works for all win types including fold wins)
			if (this.getGameRoom.lastRoundResult) return true;
			// Fallback: check if other players have visible hands (showdown scenario)
			const otherPlayers = this.getGameRoom.otherPlayers || [];
			return otherPlayers.length > 0 && otherPlayers.some(p => p.hand && p.hand.length > 0);
		},
		
		roundResult() {
			return this.getGameRoom?.lastRoundResult;
		}
	},
	async mounted() {
		// Set up SignalR callbacks
		signalRService.onGameStateUpdate((gameState) => {
			this.updateGameData(gameState);
		});

		signalRService.onConnectionChange((connected) => {
			this.isConnected = connected;
			if (!connected) {
				console.log('Disconnected from game server, reconnecting...');
			}
		});

		// Join the game room with player ID
		try {
			const playerId = this.$store.state.player?.secretId;
			const gameId = this.$store.state.gameId;
			
			if (playerId && gameId) {
				await signalRService.joinGameRoom(gameId, playerId);
				this.isConnected = true;
				// Get initial state
				await this.getState();
			} else {
				console.error('Missing player or game ID');
				this.$router.push('/');
			}
		} catch (error) {
			console.error('Failed to join game room:', error);
		}
	},
	async beforeUnmount() {
		// Leave the game room
		try {
			const playerId = this.$store.state.player?.secretId;
			const gameId = this.$store.state.gameId;
			
			if (playerId && gameId) {
				await signalRService.leaveGameRoom(gameId, playerId);
			}
		} catch (error) {
			console.error('Failed to leave game room:', error);
		}
	},
	methods: {
		...mapActions(['getState', 'playerIsReady', 'commitAction']),
		
		updateGameData(gameState) {
			this.$store.commit('updateGameData', gameState);
		},
		
		formatChips(amount) {
			return `${amount}¢`;
		},
		
		isWinner(playerId) {
			if (!this.roundResult) return false;
			return this.roundResult.winners.some(w => w.playerId === playerId);
		},
		
		getWinnerInfo(playerId) {
			if (!this.roundResult) return null;
			return this.roundResult.winners.find(w => w.playerId === playerId);
		},
		
		async handleAction(action) {
			try {
				await this.commitAction(action);
			} catch (error) {
				console.error('Action failed:', error);
				alert('Action failed. Please try again.');
			}
		},
		
		async triggerReady() {
			try {
				await this.playerIsReady();
				this.showingResults = false;
			} catch (error) {
				console.error('Ready failed:', error);
			}
		},
		
		exitGame() {
			if (confirm('Are you sure you want to leave the game?')) {
				this.$router.push('/');
			}
		}
	}
};
</script>

<style scoped>
.game-page {
	min-height: 100vh;
	position: relative;
}

.game-container {
	min-height: 100vh;
	padding-bottom: 200px;
}

/* Waiting Screen */
.waiting-screen {
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 100vh;
	padding: var(--space-6);
}

.waiting-content {
	max-width: 900px;
	width: 100%;
	text-align: center;
}

.waiting-title {
	font-size: var(--text-4xl);
	margin-bottom: var(--space-4);
	color: var(--color-light);
	text-shadow: 0 4px 20px rgba(46, 204, 113, 0.3);
}

.waiting-subtitle {
	font-size: var(--text-xl);
	color: var(--color-gray-400);
	margin-bottom: var(--space-8);
}

/* Winner Announcement */
.winner-announcement {
	background: linear-gradient(135deg, rgba(255, 215, 0, 0.2), rgba(255, 193, 7, 0.1));
	border: 3px solid gold;
	border-radius: var(--radius-2xl);
	padding: var(--space-8);
	margin-bottom: var(--space-6);
	box-shadow: 0 0 40px rgba(255, 215, 0, 0.3);
	animation: fadeIn 0.6s ease-out, winnerGlow 2s ease-in-out infinite;
}

@keyframes winnerGlow {
	0%, 100% {
		box-shadow: 0 0 40px rgba(255, 215, 0, 0.3);
	}
	50% {
		box-shadow: 0 0 60px rgba(255, 215, 0, 0.5);
	}
}

.trophy-icon {
	font-size: 5rem;
	margin-bottom: var(--space-4);
	animation: bounce 2s ease-in-out infinite;
}

.winner-title {
	font-size: var(--text-3xl);
	color: gold;
	margin-bottom: var(--space-6);
	text-shadow: 0 2px 10px rgba(255, 215, 0, 0.5);
}

.winners-list {
	display: flex;
	flex-direction: column;
	gap: var(--space-4);
	margin-bottom: var(--space-6);
}

.winner-card {
	background: rgba(0, 0, 0, 0.4);
	border: 2px solid rgba(255, 215, 0, 0.5);
	border-radius: var(--radius-xl);
	padding: var(--space-5);
	transition: all var(--transition-base);
}

.winner-card.is-current-player {
	background: rgba(46, 204, 113, 0.2);
	border-color: var(--color-primary);
	box-shadow: 0 0 20px rgba(46, 204, 113, 0.4);
}

.winner-name {
	font-size: var(--text-2xl);
	font-weight: 800;
	color: gold;
	margin-bottom: var(--space-2);
}

.winner-card.is-current-player .winner-name {
	color: var(--color-primary);
}

.winner-hand {
	font-size: var(--text-lg);
	color: var(--color-gray-300);
	margin-bottom: var(--space-2);
	font-style: italic;
}

.winner-amount {
	font-size: var(--text-3xl);
	font-weight: 800;
	font-family: var(--font-mono);
	color: var(--color-light);
	text-shadow: 0 2px 10px rgba(255, 215, 0, 0.3);
}

.pot-total {
	font-size: var(--text-xl);
	color: var(--color-gray-300);
	padding-top: var(--space-4);
	border-top: 1px solid rgba(255, 215, 0, 0.3);
}

.waiting-actions {
	display: flex;
	gap: var(--space-4);
	justify-content: center;
	margin-bottom: var(--space-8);
}

.waiting-actions button {
	min-width: 200px;
}

.btn-large {
	padding: var(--space-5) var(--space-10);
	font-size: var(--text-xl);
}

/* Pre-game Waiting */
.pre-game-waiting {
	animation: fadeIn 0.6s ease-out;
}

.players-ready-list {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
	gap: var(--space-4);
	margin-bottom: var(--space-8);
	max-width: 600px;
	margin-left: auto;
	margin-right: auto;
}

.ready-player {
	background: rgba(255, 255, 255, 0.05);
	padding: var(--space-4);
	border-radius: var(--radius-xl);
	border: 2px solid rgba(255, 255, 255, 0.1);
	display: flex;
	align-items: center;
	gap: var(--space-3);
	transition: all var(--transition-base);
}

.ready-player.is-ready {
	background: rgba(46, 204, 113, 0.1);
	border-color: var(--color-success);
}

.ready-icon {
	font-size: var(--text-2xl);
}

.ready-name {
	font-weight: 600;
	color: var(--color-light);
}

/* Results Display */
.results-display {
	background: rgba(255, 255, 255, 0.05);
	border-radius: var(--radius-2xl);
	padding: var(--space-8);
	border: 2px solid rgba(255, 255, 255, 0.1);
	backdrop-filter: blur(10px);
}

.results-display h3 {
	font-size: var(--text-2xl);
	margin-bottom: var(--space-6);
	color: var(--color-light);
}

.community-cards-display {
	display: flex;
	gap: var(--space-4);
	justify-content: center;
	margin-bottom: var(--space-8);
}

.result-card {
	width: 90px;
	height: 126px;
	border-radius: var(--radius-lg);
	box-shadow: var(--shadow-card);
	transition: transform var(--transition-base);
}

.result-card:hover {
	transform: translateY(-10px) scale(1.05);
}

.result-card-mini {
	width: 60px;
	height: 84px;
	border-radius: var(--radius-md);
	box-shadow: var(--shadow-md);
}

.players-results {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
	gap: var(--space-4);
}

.player-result-card {
	background: rgba(0, 0, 0, 0.3);
	padding: var(--space-4);
	border-radius: var(--radius-xl);
	border: 2px solid rgba(255, 255, 255, 0.1);
	transition: all var(--transition-base);
}

.player-result-card.winner {
	background: linear-gradient(135deg, rgba(255, 215, 0, 0.2), rgba(255, 193, 7, 0.1));
	border-color: gold;
	box-shadow: 0 0 20px rgba(255, 215, 0, 0.4);
}

.player-result-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: var(--space-3);
}

.player-result-card h4 {
	font-size: var(--text-base);
	margin: 0;
	color: var(--color-light);
}

.player-result-card.winner h4 {
	color: gold;
	font-weight: 800;
}

.winner-badge {
	font-size: var(--text-sm);
	background: gold;
	color: var(--color-dark);
	padding: var(--space-1) var(--space-2);
	border-radius: var(--radius-full);
	font-weight: 700;
}

.player-result-hand {
	display: flex;
	gap: var(--space-2);
	justify-content: center;
	margin-bottom: var(--space-3);
}

.winner-details {
	text-align: center;
	padding-top: var(--space-3);
	border-top: 1px solid rgba(255, 255, 255, 0.2);
}

.hand-name {
	font-size: var(--text-sm);
	color: var(--color-gray-400);
	margin-bottom: var(--space-1);
	font-style: italic;
}

.player-result-card.winner .hand-name {
	color: gold;
	font-weight: 600;
}

.amount-won {
	font-size: var(--text-lg);
	font-weight: 800;
	font-family: var(--font-mono);
	color: var(--color-success);
}

/* Exit Button */
.exit-btn {
	position: fixed;
	top: var(--space-6);
	left: var,--space-6);
	background: rgba(0, 0, 0, 0.7);
	backdrop-filter: blur(10px);
	color: var(--color-light);
	border: 2px solid rgba(255, 255, 255, 0.2);
	padding: var(--space-3) var(--space-6);
	border-radius: var(--radius-lg);
	font-weight: 600;
	cursor: pointer;
	transition: all var(--transition-base);
	z-index: var(--z-sticky);
}

.exit-btn:hover {
	background: rgba(0, 0, 0, 0.9);
	border-color: var(--color-primary);
	transform: translateX(-4px);
}

/* Loading Screen */
.loading-screen {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	min-height: 100vh;
	gap: var(--space-6);
}

.spinner-large {
	width: 80px;
	height: 80px;
	border: 6px solid rgba(255, 255, 255, 0.1);
	border-top-color: var(--color-primary);
	border-radius: 50%;
	animation: spin 1s linear infinite;
}

.loading-screen p {
	font-size: var(--text-xl);
	color: var(--color-gray-400);
}

/* Fade Transition */
.fade-enter-active,
.fade-leave-active {
	transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
	opacity: 0;
}

/* Responsive Design */
@media (max-width: 768px) {
	.game-container {
		padding-bottom: 250px;
	}

	.waiting-actions {
		flex-direction: column;
	}

	.waiting-actions button {
		width: 100%;
	}

	.players-ready-list {
		grid-template-columns: 1fr;
	}

	.community-cards-display {
		gap: var(--space-2);
		flex-wrap: wrap;
	}

	.result-card {
		width: 60px;
		height: 84px;
	}

	.trophy-icon {
		font-size: 3rem;
	}

	.winner-name {
		font-size: var(--text-xl);
	}

	.winner-amount {
		font-size: var(--text-2xl);
	}
}
</style>