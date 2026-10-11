<script setup lang="ts">
import { computed } from 'vue'
import type { OrderStatus } from '@/types/api'

const props = defineProps<{ status: OrderStatus }>()

// The three stages a customer cares about. Completed counts as past "Ready".
const steps = [
  { status: 'Pending', label: 'Received' },
  { status: 'Preparing', label: 'Preparing' },
  { status: 'Ready', label: 'Ready' },
] as const

const currentIndex = computed(() => {
  if (props.status === 'Completed') return steps.length
  return steps.findIndex((s) => s.status === props.status)
})
</script>

<template>
  <ol class="progress" aria-label="Order progress">
    <li
      v-for="(step, i) in steps"
      :key="step.status"
      :class="{ done: i < currentIndex, current: i === currentIndex }"
      :aria-current="i === currentIndex ? 'step' : undefined"
    >
      <span class="dot" aria-hidden="true">
        <i v-if="i < currentIndex || (i === currentIndex && step.status === 'Ready')" class="pi pi-check" />
      </span>
      <span class="label">{{ step.label }}</span>
    </li>
  </ol>
</template>

<style scoped>
.progress {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  grid-template-columns: repeat(3, 1fr);
}

li {
  position: relative;
  display: grid;
  justify-items: center;
  gap: 8px;
  color: var(--muted);
  font-size: 0.9rem;
}

/* connecting line between dots */
li + li::before {
  content: '';
  position: absolute;
  top: 13px;
  right: 50%;
  width: 100%;
  height: 3px;
  background: var(--line);
  z-index: 0;
}

li.done + li::before,
li.done + li.current::before {
  background: var(--pandan);
}

.dot {
  position: relative;
  z-index: 1;
  width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  background: var(--paper);
  border: 3px solid var(--line);
  color: #fff;
  font-size: 0.75rem;
}

.done .dot {
  background: var(--pandan);
  border-color: var(--pandan);
}

.current .dot {
  border-color: var(--pandan);
  box-shadow: 0 0 0 5px var(--pandan-wash);
}

.current .dot:has(.pi-check) {
  background: var(--pandan);
}

.done,
.current {
  color: var(--ink);
}

.current .label {
  font-weight: 700;
}

@media (prefers-reduced-motion: no-preference) {
  .current .dot {
    animation: pulse 1.8s ease-in-out infinite;
  }

  @keyframes pulse {
    50% {
      box-shadow: 0 0 0 9px var(--pandan-wash);
    }
  }
}
</style>
