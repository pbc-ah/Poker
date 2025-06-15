import { createStore } from "vuex";
import createPersistedState from "vuex-persistedstate";
import axios from 'axios';
import router from "../router";

export default createStore({
	plugins: [createPersistedState({
		storage: window.localStorage,
	})],
	state() {
		return {
            player: null,
			gameId: null,
			gameState: null,
			gameRooms: []
		}
    },
    getters: {
        getGameRoom: state => state.gameState,
        getGameRooms: state => state.rooms
    },
	mutations: {
        updateGameData: (state, latest) => state.gameState = latest,
        updatePlayerIds: (state, latest) => state.player = {
            id: latest.id,
            secretId: latest.secretId
        },
		updateGameId: (state, latest) => state.gameId = latest,
		updateRooms: (state, latest) => state.rooms = latest ?? [],
	},
	actions: {
        async createGame(_, startingBet) {
            await axios.get(`/create/${startingBet}`);
        },
        async viewRooms({ commit }) {
            const response = await axios.get(`/rooms`);
            commit("updateRooms", response.data)
        },
        async joinGame({ commit }, {
            gameId,
            identity
        }) {
            const response = await axios.post(`${gameId}/join`, identity);
            commit("updatePlayerIds", response.data)
            commit("updateGameId", gameId)
        },
        async getState({ commit, state }) {
            const response = await axios.get(`${state.gameId}/${state.player.secretId}/state`);
            if (response.status === 404) {
                router.push('/');
                return;
            }
            commit("updateGameData", response.data);
        },
        async playerIsReady({ state }) {
            await axios.get(`${state.gameId}/${state.player.secretId}/ready`)
        },
        async commitAction({ state }, action) {
            await axios.post(`${state.gameId}/${state.player.secretId}/action`, action);
        }
    }
});