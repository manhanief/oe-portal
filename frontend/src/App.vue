<script setup lang="ts">
import ConfirmDialog from 'primevue/confirmdialog'
import Toast from 'primevue/toast'
import { useRoute } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'

const route = useRoute()
</script>

<template>
  <!-- Customer pages share this header; staff pages have their own -->
  <AppHeader v-if="route.matched.length > 0 && !route.meta.staff" />
  <main>
    <RouterView />
  </main>
  <Toast position="top-center" />
  <ConfirmDialog />
</template>
