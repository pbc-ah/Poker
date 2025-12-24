<template>
  <transition name="slide-down">
    <div v-if="!isConnected" class="connection-status-bar">
      <div class="status-content">
        <div class="status-icon">
          <div class="spinner-small"></div>
        </div>
        <span class="status-text">Reconnecting to server...</span>
      </div>
    </div>
  </transition>
</template>

<script>
export default {
  name: 'ConnectionStatus',
  props: {
    isConnected: {
      type: Boolean,
      default: true
    }
  }
};
</script>

<style scoped>
.connection-status-bar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  background: linear-gradient(135deg, #e74c3c, #c0392b);
  color: white;
  padding: var(--space-3) var(--space-6);
  z-index: 10000;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
}

.status-content {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-3);
}

.status-icon {
  display: flex;
  align-items: center;
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.status-text {
  font-weight: 600;
  font-size: var(--text-sm);
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from {
  transform: translateY(-100%);
  opacity: 0;
}

.slide-down-leave-to {
  transform: translateY(-100%);
  opacity: 0;
}
</style>
