import { ref, computed } from 'vue';
import { useStore } from 'vuex';

export function useGame() {
  const store = useStore();
  const loading = ref(false);
  const error = ref(null);

  const gameState = computed(() => store.getters.getGameRoom);
  const rooms = computed(() => store.getters.getGameRooms);

  const createRoom = async (ante) => {
    loading.value = true;
    error.value = null;
    
    try {
      await store.dispatch('createGame', ante);
      await store.dispatch('viewRooms');
      return true;
    } catch (err) {
      error.value = err.message || 'Failed to create room';
      return false;
    } finally {
      loading.value = false;
    }
  };

  const joinRoom = async (gameId, identity) => {
    loading.value = true;
    error.value = null;
    
    try {
      await store.dispatch('joinGame', { gameId, identity });
      await store.dispatch('getState');
      return true;
    } catch (err) {
      error.value = err.message || 'Failed to join room';
      return false;
    } finally {
      loading.value = false;
    }
  };

  const fetchRooms = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      await store.dispatch('viewRooms');
    } catch (err) {
      error.value = err.message || 'Failed to fetch rooms';
    } finally {
      loading.value = false;
    }
  };

  const performAction = async (action) => {
    loading.value = true;
    error.value = null;
    
    try {
      await store.dispatch('commitAction', action);
      return true;
    } catch (err) {
      error.value = err.message || 'Action failed';
      return false;
    } finally {
      loading.value = false;
    }
  };

  const markReady = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      await store.dispatch('playerIsReady');
      return true;
    } catch (err) {
      error.value = err.message || 'Failed to mark ready';
      return false;
    } finally {
      loading.value = false;
    }
  };

  const refreshGameState = async () => {
    try {
      await store.dispatch('getState');
    } catch (err) {
      console.error('Failed to refresh game state:', err);
    }
  };

  return {
    loading,
    error,
    gameState,
    rooms,
    createRoom,
    joinRoom,
    fetchRooms,
    performAction,
    markReady,
    refreshGameState
  };
}
