<template>
	<div class="lobby-page">
		<LobbyView 
			:rooms="getGameRooms"
			:loading="loading"
			@create-room="handleCreateRoom"
			@join-room="handleJoinRoom"
		/>
	</div>
</template>

<script>
import { mapActions, mapGetters, mapMutations } from "vuex";
import LobbyView from "../components/LobbyView.vue";
import signalRService from "../services/signalr";

export default {
	name: 'Lobby',
	components: {
		LobbyView
	},
	data() {
		return {
			loading: true,
			isConnected: false
		};
	},
	async mounted() {
		// Set up SignalR callbacks
		signalRService.onLobbyUpdate((rooms) => {
			this.$store.commit('updateRooms', rooms);
		});

		signalRService.onConnectionChange((connected) => {
			this.isConnected = connected;
		});

		// Join the lobby room
		try {
			await signalRService.joinLobby();
			this.isConnected = true;
		} catch (error) {
			console.error('Failed to join lobby:', error);
		}

		await this.loadRooms();
	},
	async beforeUnmount() {
		// Leave the lobby room
		try {
			await signalRService.leaveLobby();
		} catch (error) {
			console.error('Failed to leave lobby:', error);
		}
	},
	methods: {
		...mapActions(['viewRooms', 'createGame', 'joinGame', 'getState']),
		...mapMutations(['updateGameData']),

		async loadRooms() {
			try {
				this.loading = true;
				this.updateGameData(null);
				await this.viewRooms();
			} catch (error) {
				console.error('Failed to load rooms:', error);
			} finally {
				this.loading = false;
			}
		},

		async handleCreateRoom(ante) {
			if (ante < 5 || ante % 5 !== 0) {
				alert('Ante must be at least 5 and a multiple of 5');
				return;
			}

			try {
				this.loading = true;
				await this.createGame(ante);
				// Give SignalR a moment to broadcast, then fallback to manual refresh
				await new Promise(resolve => setTimeout(resolve, 300));
				await this.loadRooms();
			} catch (error) {
				console.error('Failed to create room:', error);
				alert('Failed to create room. Please try again.');
			} finally {
				this.loading = false;
			}
		},

		async handleJoinRoom({ roomId, playerName, playerBalance }) {
			if (!playerName || !playerBalance || playerBalance < 0) {
				alert('Please enter valid player details');
				return;
			}

			try {
				this.loading = true;
				await this.joinGame({
					gameId: roomId,
					identity: {
						name: playerName,
						balance: playerBalance
					}
				});

				await this.getState();
				this.$router.push('/game');
			} catch (error) {
				console.error('Failed to join room:', error);
				alert('Failed to join room. Please try again.');
			} finally {
				this.loading = false;
			}
		}
	},
	computed: {
		...mapGetters(['getGameRooms'])
	}
};
</script>

<style scoped>
.lobby-page {
	min-height: 100vh;
}
</style>