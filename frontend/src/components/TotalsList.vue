<script setup lang="ts">
import { TAX_RATE } from '@/config'
import { formatMoney } from '@/utils/money'

defineProps<{
  subtotal: number
  tax: number
  total: number
}>()
</script>

<template>
  <dl class="totals">
    <div>
      <dt>Subtotal</dt>
      <dd class="price">{{ formatMoney(subtotal) }}</dd>
    </div>
    <div>
      <dt>SST ({{ Math.round(TAX_RATE * 100) }}%)</dt>
      <dd class="price">{{ formatMoney(tax) }}</dd>
    </div>
    <div class="grand">
      <dt>Total</dt>
      <dd class="price">{{ formatMoney(total) }}</dd>
    </div>
  </dl>
</template>

<style scoped>
.totals {
  margin: 0;
  display: grid;
  gap: 6px;
}

.totals div {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

dt {
  color: var(--muted);
}

dd {
  margin: 0;
}

.grand {
  margin-top: 6px;
  padding-top: 10px;
  border-top: 1px dashed var(--line);
  font-size: 1.2rem;
}

.grand dt {
  color: var(--ink);
  font-weight: 600;
}
</style>
