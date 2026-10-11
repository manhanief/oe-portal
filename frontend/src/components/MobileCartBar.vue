<script setup lang="ts">
import { useCartStore } from '@/stores/cart'
import { formatMoney } from '@/utils/money'

const cart = useCartStore()
</script>

<template>
  <div v-if="!cart.isEmpty" class="bar">
    <RouterLink to="/checkout" class="go">
      <span class="count">{{ cart.itemCount }}</span>
      <span>Review order</span>
      <span class="price">{{ formatMoney(cart.total) }}</span>
    </RouterLink>
  </div>
</template>

<style scoped>
.bar {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 15;
  padding: 12px var(--gutter) calc(12px + env(safe-area-inset-bottom));
  background: linear-gradient(to top, var(--tile) 70%, transparent);
}

.go {
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 12px;
  padding: 14px 18px;
  border-radius: var(--radius-m);
  background: var(--pandan);
  color: #fff;
  text-decoration: none;
  font-weight: 600;
}

.count {
  min-width: 28px;
  height: 28px;
  padding: 0 6px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  background: var(--kaya);
  color: var(--ink);
  font-family: var(--font-display);
  font-weight: 700;
}

@media (min-width: 960px) {
  .bar {
    display: none;
  }
}
</style>
