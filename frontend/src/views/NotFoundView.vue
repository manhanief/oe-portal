<script setup lang="ts">
import Button from 'primevue/button'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="page">
    <div class="panel box">
      <h1>This page doesn't exist</h1>
      <p class="muted">The link may be wrong or out of date.</p>
      <Button label="Go to menu" @click="router.push('/')" />
    </div>
  </div>
</template>

<style scoped>
.box {
  display: grid;
  gap: 12px;
  justify-items: start;
  max-width: 480px;
}
</style>
