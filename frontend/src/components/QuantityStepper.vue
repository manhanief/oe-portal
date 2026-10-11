<script setup lang="ts">
import { MAX_QUANTITY } from '@/stores/cart'

defineProps<{
  quantity: number
  /** Used by screen readers, e.g. "Nasi Lemak Ayam" */
  itemName: string
}>()

const emit = defineEmits<{
  increase: []
  decrease: []
}>()
</script>

<template>
  <div class="stepper" role="group" :aria-label="`Quantity of ${itemName}`">
    <button type="button" class="step" :aria-label="`Remove one ${itemName}`" @click="emit('decrease')">
      <i :class="quantity === 1 ? 'pi pi-trash' : 'pi pi-minus'" aria-hidden="true" />
    </button>
    <output class="count" aria-live="polite">{{ quantity }}</output>
    <button
      type="button"
      class="step"
      :aria-label="`Add one more ${itemName}`"
      :disabled="quantity >= MAX_QUANTITY"
      @click="emit('increase')"
    >
      <i class="pi pi-plus" aria-hidden="true" />
    </button>
  </div>
</template>

<style scoped>
.stepper {
  display: inline-flex;
  align-items: center;
  border: 1.5px solid var(--pandan);
  border-radius: 999px;
  background: var(--paper);
  overflow: hidden;
}

.step {
  width: 40px;
  height: 40px;
  border: 0;
  background: transparent;
  color: var(--pandan);
  cursor: pointer;
  font-size: 0.85rem;
}

.step:hover:not(:disabled) {
  background: var(--pandan-wash);
}

.step:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.count {
  min-width: 28px;
  text-align: center;
  font-family: var(--font-display);
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}
</style>
