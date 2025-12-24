<template>
  <div class="loading-overlay" v-if="show">
    <div class="loading-content">
      <div class="poker-chips-loader">
        <div class="chip chip-1"></div>
        <div class="chip chip-2"></div>
        <div class="chip chip-3"></div>
      </div>
      <p class="loading-text">{{ message }}</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'LoadingOverlay',
  props: {
    show: {
      type: Boolean,
      default: false
    },
    message: {
      type: String,
      default: 'Loading...'
    }
  }
};
</script>

<style scoped>
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.85);
  backdrop-filter: blur(5px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
  animation: fadeIn 0.2s ease-out;
}

.loading-content {
  text-align: center;
}

.poker-chips-loader {
  position: relative;
  width: 100px;
  height: 100px;
  margin: 0 auto var(--space-6);
}

.chip {
  position: absolute;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  border: 4px solid rgba(255, 255, 255, 0.3);
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
}

.chip-1 {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-red), #c0392b);
  animation: chipSpin1 1.5s ease-in-out infinite;
}

.chip-2 {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-blue), #2980b9);
  animation: chipSpin2 1.5s ease-in-out infinite;
  animation-delay: 0.2s;
}

.chip-3 {
  background: radial-gradient(circle at 30% 30%, var(--color-chip-green), #27ae60);
  animation: chipSpin3 1.5s ease-in-out infinite;
  animation-delay: 0.4s;
}

@keyframes chipSpin1 {
  0%, 100% {
    transform: translate(-50%, -50%) rotate(0deg) translateY(0);
    opacity: 1;
  }
  50% {
    transform: translate(-50%, -50%) rotate(180deg) translateY(-30px);
    opacity: 0.7;
  }
}

@keyframes chipSpin2 {
  0%, 100% {
    transform: translate(-50%, -50%) rotate(0deg) translateX(30px);
    opacity: 1;
  }
  50% {
    transform: translate(-50%, -50%) rotate(180deg) translateX(-30px);
    opacity: 0.7;
  }
}

@keyframes chipSpin3 {
  0%, 100% {
    transform: translate(-50%, -50%) rotate(0deg) translateY(0);
    opacity: 1;
  }
  50% {
    transform: translate(-50%, -50%) rotate(180deg) translateY(30px);
    opacity: 0.7;
  }
}

.loading-text {
  font-size: var(--text-xl);
  color: var(--color-light);
  font-weight: 600;
  animation: pulse 1.5s ease-in-out infinite;
}
</style>
