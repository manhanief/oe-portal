<script setup lang="ts">
import Button from 'primevue/button'
import { computed } from 'vue'
import QuantityStepper from '@/components/QuantityStepper.vue'
import { useCartStore } from '@/stores/cart'
import type { MenuItem } from '@/types/api'
import { formatMoney } from '@/utils/money'

const props = defineProps<{ item: MenuItem }>()

const cart = useCartStore()
const inCart = computed(() => cart.quantityOf(props.item.id))
</script>

<template>
  <li class="row" :class="{ soldout: !item.isAvailable }">
    <div class="text">
      <h3>{{ item.name }}</h3>
      <p v-if="item.description" class="muted">{{ item.description }}</p>
      <p class="price">{{ formatMoney(item.price) }}</p>
    </div>

    <div class="action">
      <span v-if="!item.isAvailable" class="soldout-tag">Sold out</span>
      <QuantityStepper
        v-else-if="inCart > 0"
        :quantity="inCart"
        :item-name="item.name"
        @increase="cart.add(item)"
        @decrease="cart.removeOne(item.id)"
      />
      <Button
        v-else
        label="Add"
        icon="pi pi-plus"
        rounded
        outlined
        :aria-label="`Add ${item.name} to order`"
        @click="cart.add(item)"
      />
    </div>
  </li>
</template>

<style scoped>
.row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 18px 0;
  border-bottom: 1px solid var(--line);
}

.row:last-child {
  border-bottom: 0;
}

.text {
  min-width: 0;
  display: grid;
  gap: 4px;
}

h3 {
  font-size: 1.1rem;
  font-weight: 650;
}

.text .muted {
  font-size: 0.925rem;
  max-width: 52ch;
}

.price {
  color: var(--pandan-deep);
}

.action {
  flex-shrink: 0;
}

.soldout h3,
.soldout .price {
  color: var(--muted);
}

.soldout .price {
  text-decoration: line-through;
}

.soldout-tag {
  display: inline-block;
  padding: 6px 12px;
  border-radius: 999px;
  background: #fbe9e7;
  color: var(--chilli);
  font-size: 0.875rem;
  font-weight: 600;
}
</style>
