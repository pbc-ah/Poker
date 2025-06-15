<template>
    <div>
        {{getGameRoom}}

        <div v-for="card in getGameRoom?.communityCards">
            <img :src="'/cards/' + card + '.svg'" />
        </div>
        <div v-if="!getGameRoom?.player?.isReady">
            <button @click="playerIsReady">I'm ready'</button>
        </div>
        <div v-else-if="getGameRoom?.currentTurn === getGameRoom?.player?.id && getGameRoom?.status === 'started'">
            Your turn...
            <button @click="check" v-if="!getGameRoom?.currentBet">Check</button>
            <button @click="call" v-if="getGameRoom?.currentBet">Call</button>
            <button @click="raise.inited=true">Raise</button>
            <button @click="fold">Fold</button>
        </div>
        <div v-if="raise.inited">
            <label style="display:block">Amount</label>
            <input type="number" style="display:block" v-model="raise.amount">
            <button @click="commitRaise">Raise</button>
        </div>
        <div v-for="card in getGameRoom?.player?.hand">
            <img :src="'/cards/' + card + '.svg'" />
        </div>
    </div>
</template>

<script>
    import { mapActions, mapGetters } from "vuex";
    export default {
        data() {
            return {
                raise: {
                    inited: false,
                    amount: 0
                }
            }
        },
        methods: {
            ...mapActions(['getState', 'playerIsReady', 'commitAction']),

            async check() {
                await this.commitAction({
                    type: "check"
                })
            },
            async call() {
                await this.commitAction({
                    type: "call"
                })
            },
            async fold() {
                await this.commitAction({
                    type: "fold"
                })
            },
            async commitRaise() {
                if (!this.raise.amount)
                    return;
                this.raise.inited = false;
                await this.commitAction({
                    type: "bet",
                    amount: this.raise.amount
                });
                this.raise.amount = 0;
            },

        },
        async mounted() {
            setInterval(async () => await this.getState(), 1000);
        },
        computed: mapGetters(['getGameRoom'])
    }
</script>