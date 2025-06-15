<template>
	<div class="room" v-if="getGameRoom">
		<div class="table">
			<template v-if="getGameRoom.status === 'waiting' && getGameRoom.otherPlayers?.[0].hand?.length">
				<div class="column">
					<label>Rundi ka perfundu, duke prit lojtart per rund tjeter...</label>
					<div style="display: flex; gap: 1em">
						<button @click="showingResults = true" v-if="!showingResults">Shih tavolinen</button>
						<button @click="triggerReady" v-if="!getGameRoom?.player?.isReady">I'm ready</button>
					</div>
					<div class="cards" v-if="showingResults">
						<img v-for="card in getGameRoom.communityCards" :src="'/cards/' + card + '.svg'" />
					</div>
				</div>
			</template>
			<template v-else-if="getGameRoom.status === 'waiting'">
				<div class="column">
					<label>Rundi ska fillu, duke prit lojtart...</label>
					<button @click="triggerReady" v-if="!getGameRoom?.player?.isReady">I'm ready</button>
				</div>
			</template>
			<template v-else>
				<div class="cards">
					<img v-for="card in getGameRoom.communityCards" :src="'/cards/' + card + '.svg'" />
				</div>
				<label class="cards" style="text-align: right">
					Pare ntavolin: {{getGameRoom.pot}}c<br />
					<template v-if="getGameRoom.currentBet">
						Basti i fundit: {{getGameRoom.currentBet}}c
					</template>
				</label>

				<div class="cards smaller">
					<img v-for="card in getGameRoom.player.hand" :src="'/cards/' + card + '.svg'" />
				</div>
				<div v-if="getGameRoom.currentTurn === getGameRoom.player.id">
					<div class="cards" v-if="inited">
						<label>Amount</label>
						<input type="number" v-model="amount" style="width: 3em">
						<button @click="commitRaise">Raise</button>
						<button @click="inited = false">Cancel</button>
					</div>
					<div class="cards" v-else>
						<button @click="genericAction('fold')">Fold</button>
						<button @click="genericAction('check')" v-if="!getGameRoom?.currentBet">Check</button>
						<button @click="genericAction('call')" v-if="getGameRoom?.currentBet">Call</button>
						<button @click="inited = true">Raise</button>
					</div>
				</div>
			</template>
		</div>


		<div class="users">
			<div class="user" :class="getGameRoom.currentTurn === player.id ? 'bold' : ''" v-for="player in [getGameRoom.player, ...getGameRoom.otherPlayers]">
				<label>Lojtari: {{player.name}}</label>
				<label>Balanci: {{player.balance}}c</label>
				<label v-if="player.isFolded">Statusi: dorzu</label>
				<label v-else-if="player.isAllIn">Statusi: all in</label>
				<label v-else-if="getGameRoom.status === 'waiting'">
					<template v-if="player.isReady">
						Statusi: gati
					</template>
					<template v-else>
						Statusi: npritje
					</template>
				</label>
				<label v-else-if="getGameRoom.currentTurn === player.id">Status: renin</label>
				<label v-else>Status: nloj</label>
				<div v-if="showingResults && player.hand">
					<label>Letrat:</label>
					<div style="display: flex; gap: 1em">
						<img v-for="card in player.hand" :src="'/cards/' + card + '.svg'" style="height: 80px" />
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
	.room {
		display: flex;
	}

	.bold {
		font-weight: 900;
	}

	.table {
		background-color: darkgreen;
		width: 80%;
		height: 50vh;
		border-radius: 2em;
		border: 1em inset black;
		display: flex;
		flex-direction: column;
	}

	.cards {
		display: flex;
		gap: 1em;
		justify-content: center;
		align-items: center;
		height: 40%
	}

		.cards > img {
			height: 80%
		}

		.cards.smaller > img {
			height: 50%
		}

	button {
		background-color: #fff;
		border: none;
		padding: .4em .75em;
		border-radius: .4em;
		font-size: .8em;
		min-width: 80px;
		display: block;
	}

	input {
		background-color: #fff;
		border: none;
		padding: .4em .75em;
		border-radius: .4em;
		font-size: .8em;
		text-align: right;
	}

	label {
		color: #fff;
		display: block;
	}

	.users > div {
		margin-bottom: 1em;
	}

	.users label {
		color: unset;
	}

	.column {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		height: 100%;
		gap: 2em;
	}
</style>

<style>
	* {
		font-family: "Lucida Console";
		user-select: none;
	}

	input::-webkit-outer-spin-button,
	input::-webkit-inner-spin-button {
		-webkit-appearance: none;
		margin: 0;
	}

	input[type=number] {
		-moz-appearance: textfield;
	}
</style>

<script>
	import { mapActions, mapGetters } from "vuex";

	export default {
		data() {
			return {
				raise: {
					inited: false,
					amount: 10,
					showingResults: false
				}
			}
		},
		methods: {
			...mapActions(['getState', 'playerIsReady', 'commitAction']),
			async genericAction(action) {
				await this.commitAction({
					type: action
				});
			},
			async triggerReady() {
				await this.playerIsReady();
				this.showingResults = false;
			},
			async commitRaise() {
				if (!this.amount)
					return;

				this.inited = false;

				await this.commitAction({
					type: "bet",
					amount: this.amount
				});

				this.amount = 0;
			}
		},
		mounted() {
			setInterval(async () => await this.getState(), 750);
		},
		computed: mapGetters(['getGameRoom'])
	}
</script>