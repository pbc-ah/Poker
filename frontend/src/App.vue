<template>
  <div id="app">
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  name: 'App'
};
</script>

<style>
@import './styles/variables.css';
@import './styles/global.css';

#app {
  min-height: 100vh;
}
</style>
