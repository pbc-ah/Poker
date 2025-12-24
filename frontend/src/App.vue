<template>
  <div id="app">
    <CardPreloader 
      :isLoading="isPreloading" 
      :loadedCount="preloadProgress.loaded"
      :totalCount="preloadProgress.total"
    />
    <router-view v-if="!isPreloading"></router-view>
  </div>
</template>

<script>
import CardPreloader from './components/CardPreloader.vue';
import { preloadCardImages } from './utils/cardPreloader.js';

export default {
  name: 'App',
  components: {
    CardPreloader
  },
  data() {
    return {
      isPreloading: true,
      preloadProgress: {
        loaded: 0,
        total: 52
      }
    };
  },
  async mounted() {
    try {
      await preloadCardImages((loaded, total) => {
        this.preloadProgress.loaded = loaded;
        this.preloadProgress.total = total;
      });
      
      // Small delay to show 100% completion
      await new Promise(resolve => setTimeout(resolve, 500));
      
      this.isPreloading = false;
    } catch (error) {
      console.error('Failed to preload cards:', error);
      // Continue anyway after 2 seconds
      setTimeout(() => {
        this.isPreloading = false;
      }, 2000);
    }
  }
};
</script>

<style>
@import './styles/variables.css';
@import './styles/global.css';

#app {
  min-height: 100vh;
}
</style>
