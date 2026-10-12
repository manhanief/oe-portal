<script setup lang="ts">
import Button from 'primevue/button'
import { computed } from 'vue'
import type { Order, OrderStatus } from '@/types/api'

const props = defineProps<{
  order: Order
  /** Current time, passed in so all cards tick together */
  now: number
  busy: boolean
  fresh: boolean
}>()

const emit = defineEmits<{
  advance: [status: OrderStatus]
  cancel: []
}>()

const LATE_AFTER_MINUTES = 15

const minutes = computed(() =>
  Math.max(0, Math.floor((props.now - new Date(props.order.createdAt).getTime()) / 60000)),
)
const isLate = computed(() => minutes.value >= LATE_AFTER_MINUTES)

const where = computed(() =>
  props.order.orderType === 'DineIn'
    ? `Table ${props.order.tableNo}`
    : `Take away${props.order.customerName ? `: ${props.order.customerName}` : ''}`,
)

const nextStep = computed<{ label: string; status: OrderStatus } | null>(() => {
  switch (props.order.status) {
    case 'Pending':
      return { label: 'Start preparing', status: 'Preparing' }
    case 'Preparing':
      return { label: 'Mark ready', status: 'Ready' }
    case 'Ready':
      return {
        label: props.order.orderType === 'DineIn' ? 'Served' : 'Picked up',
        status: 'Completed',
      }
    default:
      return null
  }
})

const canCancel = computed(() => props.order.status === 'Pending' || props.order.status === 'Preparing')
</script>

<template>
  <article class="card" :class="{ fresh, late: isLate }" :aria-label="`Order ${order.orderNo}`">
    <header class="head">
      <h3 class="no">{{ order.orderNo }}</h3>
      <span class="age" :title="`Placed ${minutes} minutes ago`">
        <i class="pi pi-clock" aria-hidden="true" />
        {{ minutes < 1 ? 'Just now' : `${minutes} min` }}
      </span>
    </header>

    <p class="where">{{ where }}</p>

    <ul class="items">
      <li v-for="(item, i) in order.items" :key="i">
        <span class="qty">{{ item.quantity }}</span>
        <span>
          {{ item.name }}
          <mark v-if="item.notes" class="note">{{ item.notes }}</mark>
        </span>
      </li>
    </ul>

    <footer class="actions">
      <Button
        v-if="nextStep"
        :label="nextStep.label"
        :loading="busy"
        fluid
        @click="emit('advance', nextStep.status)"
      />
      <Button
        v-if="canCancel"
        label="Cancel"
        severity="danger"
        text
        size="small"
        :disabled="busy"
        @click="emit('cancel')"
      />
    </footer>
  </article>
</template>

<style scoped>
.card {
  display: grid;
  gap: 10px;
  padding: 16px;
  border-radius: var(--radius-m);
  background: var(--paper);
  border: 1px solid var(--line);
  border-top: 5px solid var(--pandan);
}

.card.late {
  border-top-color: var(--chilli);
}

.head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 8px;
}

.no {
  font-size: 1.9rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.age {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  color: var(--muted);
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.late .age {
  color: var(--chilli);
}

.where {
  font-weight: 700;
  font-size: 1.05rem;
}

.items {
  list-style: none;
  margin: 0;
  padding: 10px 0 0;
  border-top: 1px dashed var(--line);
  display: grid;
  gap: 8px;
}

.items li {
  display: grid;
  grid-template-columns: 30px 1fr;
  gap: 8px;
  font-size: 1.05rem;
}

.qty {
  display: grid;
  place-items: center;
  height: 26px;
  border-radius: 6px;
  background: var(--pandan-wash);
  color: var(--pandan-deep);
  font-family: var(--font-display);
  font-weight: 800;
}

/* Notes matter most to the cook, so they stand out */
.note {
  display: block;
  width: fit-content;
  margin-top: 4px;
  padding: 2px 8px;
  border-radius: 4px;
  background: #fbe7a8;
  color: var(--kaya-deep);
  font-weight: 600;
  font-size: 0.9rem;
}

.actions {
  display: grid;
  gap: 4px;
  margin-top: 4px;
}

@media (prefers-reduced-motion: no-preference) {
  .card.fresh {
    animation: arrive 2.4s ease-out;
  }

  @keyframes arrive {
    0%,
    40% {
      box-shadow: 0 0 0 4px var(--kaya);
    }
    100% {
      box-shadow: 0 0 0 0 transparent;
    }
  }
}
</style>
