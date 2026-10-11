<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { RESTAURANT_NAME } from '@/config'
import { useCartStore } from '@/stores/cart'

const cart = useCartStore()
const route = useRoute()

// Show a shortcut back to the last order, except when already looking at it
const showOrderLink = computed(
  () => cart.lastOrderId !== null && !(route.name === 'order' && Number(route.params.id) === cart.lastOrderId),
)
</script>

<template>
  <header class="app-header">
    <div class="inner">
      <RouterLink to="/" class="brand">
        <img src="/favicon.svg" alt="" width="32" height="32" />
        <span>{{ RESTAURANT_NAME }}</span>
      </RouterLink>

      <RouterLink v-if="showOrderLink" :to="`/orders/${cart.lastOrderId}`" class="order-link">
        <i class="pi pi-receipt" aria-hidden="true" />
        Your order
      </RouterLink>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 20;
  height: var(--header-h);
  background: var(--pandan);
  color: #fff;
}

.inner {
  max-width: var(--page-max);
  height: 100%;
  margin: 0 auto;
  padding: 0 var(--gutter);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  color: inherit;
  text-decoration: none;
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.25rem;
}

.brand img {
  border-radius: 8px;
  box-shadow: 0 0 0 2px rgb(255 255 255 / 0.25);
}

.order-link {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 14px;
  border-radius: 999px;
  background: var(--kaya);
  color: var(--ink);
  font-weight: 600;
  text-decoration: none;
}

.order-link:hover {
  background: #f6cf6f;
}
</style>
