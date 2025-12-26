import { createApp } from 'vue'
import App from './App.vue'
import store from './store'
import router from './router'
import axios from 'axios'
import signalRService from './services/signalr'

//const baseUrl = "http://localhost:5267/";
const baseUrl = "http://192.168.5.58:81/";

axios.defaults.baseURL = baseUrl;

const app = createApp(App);
app.use(router)
app.use(store);
app.mount('#app')

// Initialize SignalR connection
signalRService.connect(baseUrl).catch(err => {
	console.error('Failed to establish SignalR connection:', err);
	// Retry connection after 3 seconds
	setTimeout(() => {
		signalRService.connect(baseUrl).catch(console.error);
	}, 3000);
});

// Handle page visibility changes to reconnect if needed
document.addEventListener('visibilitychange', async () => {
	if (document.visibilityState === 'visible' && !signalRService.isConnected) {
		try {
			await signalRService.connect(baseUrl);
		} catch (err) {
			console.error('Failed to reconnect:', err);
		}
	}
});
