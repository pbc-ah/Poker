<template>
    <div>Rooms</div>
    <div v-if="getGameRooms" v-for="game in getGameRooms" key="game">
        {{game}}
        <button v-if="!tempGameId && game.status === 'waiting'" @click="tempGameId = game.id">Join game</button>
        <div v-else-if="tempGameId === game.id">
            <label>Name</label>
            <input v-model="name" />
            <label>Balance</label>
            <input v-model="balance" />
            <button @click="joinGameRoom">Commit join</button>
        </div>
    </div>
    <div v-else>
        No rooms available
    </div>
    <button @click="roomCreateInited = true" v-if="!roomCreateInited">Create room</button>
    <div v-if="roomCreateInited">
        <label style="display: block">Starting hand (in eurocent):</label>
        <input type="number" v-model="ante" style="display:block" />
        <button @click="commitCreateRoom">Create room</button>
    </div>
</template>

<script>
	import { mapActions, mapGetters, mapMutations } from "vuex";
    export default {
        data() {
            return {
                roomCreateInited: false,
                ante: 10,
                tempGameId: null,
                name: '',
                balance: 0
            }
        },
        methods: {
            ...mapActions(['viewRooms', 'createGame', 'joinGame']),

			...mapMutations(['updateGameData']),

            async commitCreateRoom() {
                if (this.ante < 5 || this.ante % 5 != 0)
                    return;

                this.roomCreateInited = false;

                await this.createGame(this.ante);

                await this.viewRooms();
            },

            async joinGameRoom() {
                await this.joinGame({
                    gameId: this.tempGameId,
                    identity: {
                        name: this.name,
                        balance: this.balance
                    }
                });

                await this.viewRooms();

                this.$router.push(`/game`);
            }
        },
        async mounted() {
            await this.viewRooms();

            this.updateGameData(null);
        },
        computed: mapGetters(['getGameRooms'])
    }
</script>