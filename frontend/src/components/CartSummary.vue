<script setup lang="ts">
import Button from 'primevue/button'
import { useRouter } from 'vue-router'
import TotalsList from '@/components/TotalsList.vue'
import { useCartStore } from '@/stores/cart'
import { formatMoney } from '@/utils/money'

const cart = useCartStore()
const router = useRouter()
</script>

<template>
  <aside class="panel cart" aria-labelledby="cart-title">
    <h2 id="cart-title">Your order</h2>

    <p v-if="cart.isEmpty" class="muted empty">Nothing here yet. Tap Add on any dish to start.</p>

    <template v-else>
      <ul class="lines">
        <li v-for="line in cart.lines" :key="line.key">
          <span class="qty">{{ line.quantity }}×</span>
          <span class="name">
            {{ line.name }}
            <small v-if="line.notes" class="muted">{{ line.notes }}</small>
          </span>
          <span class="price">{{ formatMoney(line.price * line.quantity) }}</span>
        </li>
      </ul>

      <TotalsList :subtotal="cart.subtotal" :tax="cart.tax" :total="cart.total" />

      <Button label="Review order" fluid size="large" @click="router.push('/checkout')" />
    </template>
  </aside>
</template>

<style scoped>
.cart {
  display: grid;
  gap: 16px;
}

h2 {
  font-size: 1.35rem;
}

.empty {
  font-size: 0.95rem;
}

.lines {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 10px;
}

.lines li {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 8px;
  align-items: start;
}

.qty {
  font-weight: 700;
  color: var(--pandan);
  font-variant-numeric: tabular-nums;
}

.name small {
  display: block;
}
</style>
