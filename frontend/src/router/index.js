import { createRouter, createWebHistory } from 'vue-router'

import Test from "../views/Test.vue";
import Lobby from "../views/Lobby.vue";
import Game from "../views/Game.vue";

const router = createRouter({
	history: createWebHistory(),
	routes: [
		{
			path: "/",
			name: "Test",
			component: Test
		},
		{
			path: "/lobby",
			name: "Lobby",
			component: Lobby
		},
		{
			path: "/game",
			name: "Game",
			component: Test
		}
	]
});

export default router;