<template>
  <transition name="fade">
    <div v-if="isLoading" class="preloader-overlay">
      <div class="preloader-content">
        <div class="logo-container">
          <span class="logo-icon animate">??</span>
          <h1 class="logo-text">Poker</h1>
          <span class="logo-icon animate-delayed">??</span>
        </div>
        
        <div class="loading-container">
          <div class="loading-bar-bg">
            <div class="loading-bar-fill" :style="{ width: `${progress}%` }"></div>
          </div>
          <div class="loading-text">
            <span>Loading cards...</span>
            <span class="loading-percentage">{{ progress }}%</span>
          </div>
          <div class="loading-details">
            {{ loadedCount }} / {{ totalCount }} cards
          </div>
        </div>

        <div class="spinning-cards">
          <div class="card-spin" v-for="i in 4" :key="i" :style="{ animationDelay: `${i * 0.2}s` }">
            ??
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script>
export default {
  name: 'CardPreloader',
  props: {
    isLoading: {
      type: Boolean,
      default: true
    },
    loadedCount: {
      type: Number,
      default: 0
    },
    totalCount: {
      type: Number,
      default: 52
    }
  },
  computed: {
    progress() {
      if (this.totalCount === 0) return 0;
      return Math.round((this.loadedCount / this.totalCount) * 100);
    }
  }
};
</script>

<style scoped>
.preloader-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
}

.preloader-content {
  text-align: center;
  max-width: 500px;
  padding: var(--space-8);
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-4);
  margin-bottom: var(--space-12);
}

.logo-icon {
  font-size: 4rem;
}

.logo-icon.animate {
  animation: bounce 2s ease-in-out infinite;
}

.logo-icon.animate-delayed {
  animation: bounce 2s ease-in-out infinite;
  animation-delay: 0.3s;
}

.logo-text {
  font-size: var(--text-5xl);
  font-family: var(--font-display);
  color: var(--color-light);
  margin: 0;
  text-shadow: 0 4px 20px rgba(46, 204, 113, 0.5);
  background: linear-gradient(135deg, #2ecc71, #27ae60);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.loading-container {
  margin-bottom: var(--space-8);
}

.loading-bar-bg {
  width: 100%;
  height: 8px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: var(--radius-full);
  overflow: hidden;
  margin-bottom: var(--space-4);
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.3);
}

.loading-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #2ecc71, #27ae60, #2ecc71);
  background-size: 200% 100%;
  border-radius: var(--radius-full);
  transition: width 0.3s ease;
  animation: shimmer 2s linear infinite;
  box-shadow: 0 0 10px rgba(46, 204, 113, 0.6);
}

@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

.loading-text {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--color-gray-300);
  font-size: var(--text-base);
  margin-bottom: var(--space-2);
}

.loading-percentage {
  font-family: var(--font-mono);
  font-weight: 700;
  color: var(--color-primary);
  font-size: var(--text-lg);
}

.loading-details {
  font-size: var(--text-sm);
  color: var(--color-gray-400);
  font-family: var(--font-mono);
}

.spinning-cards {
  display: flex;
  justify-content: center;
  gap: var(--space-3);
  margin-top: var(--space-8);
}

.card-spin {
  font-size: var(--text-3xl);
  animation: cardFlip 1.5s ease-in-out infinite;
  opacity: 0.7;
}

@keyframes cardFlip {
  0%, 100% {
    transform: rotateY(0deg) translateY(0);
  }
  25% {
    transform: rotateY(90deg) translateY(-10px);
  }
  50% {
    transform: rotateY(180deg) translateY(0);
  }
  75% {
    transform: rotateY(270deg) translateY(-10px);
  }
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-20px);
  }
}

/* Fade transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

@media (max-width: 768px) {
  .logo-text {
    font-size: var(--text-4xl);
  }
  
  .logo-icon {
    font-size: 3rem;
  }
  
  .preloader-content {
    padding: var(--space-4);
  }
}
</style>
