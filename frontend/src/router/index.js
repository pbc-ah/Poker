import { createRouter, createWebHistory } from 'vue-router'

import Lobby from "../views/Lobby.vue";
import Game from "../views/Game.vue";

const router = createRouter({
	history: createWebHistory(),
	routes: [
		{
			path: "/",
			name: "Lobby",
			component: Lobby
		},
		{
			path: "/game",
			name: "Game",
			component: Game
		}
	]
});

export default router;