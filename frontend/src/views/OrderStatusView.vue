<script setup lang="ts">
import Button from 'primevue/button'
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import { useToast } from 'primevue/usetoast'
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '@/api/http'
import { restaurantApi } from '@/api/restaurant'
import OrderProgress from '@/components/OrderProgress.vue'
import TotalsList from '@/components/TotalsList.vue'
import { ORDER_REFRESH_MS } from '@/config'
import type { Order, OrderStatus } from '@/types/api'
import { formatMoney } from '@/utils/money'

const props = defineProps<{ id: number }>()

const router = useRouter()
const toast = useToast()

const order = ref<Order | null>(null)
const loading = ref(true)
const notFound = ref(false)
const connectionError = ref<string | null>(null)
let timer: number | undefined

const isFinished = (status: OrderStatus) => status === 'Completed' || status === 'Cancelled'

/** Load the order, then schedule the next check until the order is finished. */
async function refresh() {
  try {
    const latest = await restaurantApi.getOrder(props.id)
    const previous = order.value?.status
    order.value = latest
    connectionError.value = null

    if (previous && previous !== 'Ready' && latest.status === 'Ready') {
      toast.add({ severity: 'success', summary: `Order ${latest.orderNo} is ready`, life: 6000 })
    }
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) {
      notFound.value = true
    } else {
      // Keep showing the last known status; just say we couldn't update it
      connectionError.value = 'Could not check for updates. Retrying…'
    }
  } finally {
    loading.value = false
    scheduleNext()
  }
}

function scheduleNext() {
  window.clearTimeout(timer)
  if (notFound.value || (order.value && isFinished(order.value.status))) return
  timer = window.setTimeout(refresh, ORDER_REFRESH_MS)
}

onMounted(refresh)
onUnmounted(() => window.clearTimeout(timer))

watch(
  () => props.id,
  () => {
    order.value = null
    loading.value = true
    notFound.value = false
    refresh()
  },
)

const where = computed(() => {
  if (!order.value) return ''
  if (order.value.orderType === 'DineIn') return `Table ${order.value.tableNo}`
  return order.value.customerName ? `Take away for ${order.value.customerName}` : 'Take away'
})

const placedAt = computed(() =>
  order.value
    ? new Date(order.value.createdAt).toLocaleTimeString('en-MY', { hour: 'numeric', minute: '2-digit' })
    : '',
)

const headline = computed(() => {
  if (!order.value) return { title: '', text: '' }
  const dineIn = order.value.orderType === 'DineIn'
  switch (order.value.status) {
    case 'Pending':
      return { title: 'Order received', text: 'The kitchen will start on it shortly.' }
    case 'Preparing':
      return { title: 'Being prepared', text: 'Your food is being cooked now.' }
    case 'Ready':
      return dineIn
        ? { title: 'Ready', text: 'We are bringing it to your table.' }
        : { title: 'Ready for pickup', text: 'Show this number at the counter.' }
    case 'Completed':
      return { title: 'Enjoy your meal', text: 'Thanks for ordering with us.' }
    case 'Cancelled':
      return { title: 'Order cancelled', text: 'Please ask our staff if this is unexpected.' }
  }
})
</script>

<template>
  <div class="page status-page">
    <!-- Loading -->
    <div v-if="loading" class="stack" aria-busy="true" aria-label="Loading order">
      <Skeleton height="14rem" border-radius="18px" />
      <Skeleton height="6rem" />
    </div>

    <!-- Not found -->
    <div v-else-if="notFound" class="panel stack">
      <h1>We can't find that order</h1>
      <p class="muted">Check the link, or ask our staff for help.</p>
      <Button label="Go to menu" @click="router.push('/')" />
    </div>

    <!-- Could not load at all -->
    <div v-else-if="!order" class="panel stack">
      <Message severity="error">{{ connectionError }}</Message>
    </div>

    <!-- Order -->
    <template v-else>
      <section class="ticket" :class="{ cancelled: order.status === 'Cancelled' }" aria-label="Order number">
        <p class="ticket-label">Order number</p>
        <p class="ticket-no">{{ order.orderNo }}</p>
        <p class="ticket-where">{{ where }}</p>
      </section>

      <section class="status" aria-live="polite">
        <h1>{{ headline.title }}</h1>
        <p class="muted">{{ headline.text }}</p>
      </section>

      <OrderProgress v-if="order.status !== 'Cancelled'" :status="order.status" />

      <Message v-if="connectionError" severity="warn" size="small">{{ connectionError }}</Message>

      <section class="panel details" aria-labelledby="details-title">
        <div class="details-head">
          <h2 id="details-title">Your items</h2>
          <span class="muted">Placed at {{ placedAt }}</span>
        </div>
        <ul class="lines">
          <li v-for="(item, i) in order.items" :key="i">
            <span class="qty">{{ item.quantity }}×</span>
            <span>
              {{ item.name }}
              <small v-if="item.notes" class="muted">{{ item.notes }}</small>
            </span>
            <span class="price">{{ formatMoney(item.lineTotal) }}</span>
          </li>
        </ul>
        <TotalsList :subtotal="order.subtotal" :tax="order.tax" :total="order.total" />
      </section>

      <p v-if="!isFinished(order.status)" class="muted small center">This page updates by itself.</p>

      <Button label="Order more" icon="pi pi-plus" outlined @click="router.push('/')" />
    </template>
  </div>
</template>

<style scoped>
.status-page {
  max-width: 560px;
  display: grid;
  gap: 24px;
}

.stack {
  display: grid;
  gap: 12px;
  justify-items: start;
}

/* The queue ticket: the one bold element of the site */
.ticket {
  position: relative;
  padding: 28px 24px 24px;
  border-radius: 18px;
  background: var(--kaya);
  color: var(--ink);
  text-align: center;
  /* punched notches on both sides, like a paper ticket */
  -webkit-mask:
    radial-gradient(circle 14px at 0 62%, #0000 98%, #000) left / 51% 100% no-repeat,
    radial-gradient(circle 14px at 100% 62%, #0000 98%, #000) right / 51% 100% no-repeat;
  mask:
    radial-gradient(circle 14px at 0 62%, #0000 98%, #000) left / 51% 100% no-repeat,
    radial-gradient(circle 14px at 100% 62%, #0000 98%, #000) right / 51% 100% no-repeat;
}

.ticket.cancelled {
  background: #e4e7e5;
  color: var(--muted);
}

.ticket-label {
  font-weight: 600;
  color: var(--kaya-deep);
}

.cancelled .ticket-label {
  color: inherit;
}

.ticket-no {
  font-family: var(--font-display);
  font-size: clamp(4.5rem, 22vw, 7.5rem);
  font-weight: 800;
  line-height: 1;
  letter-spacing: -0.03em;
  font-variant-numeric: tabular-nums;
  padding-bottom: 14px;
  border-bottom: 2px dashed rgb(23 36 29 / 0.35);
}

.cancelled .ticket-no {
  text-decoration: line-through;
}

.ticket-where {
  margin-top: 12px;
  font-size: 1.15rem;
  font-weight: 700;
}

.status {
  display: grid;
  gap: 4px;
  text-align: center;
}

.status h1 {
  font-size: 1.9rem;
  font-weight: 750;
}

.details {
  display: grid;
  gap: 14px;
}

.details-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12px;
}

.details h2 {
  font-size: 1.2rem;
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
}

.lines small {
  display: block;
}

.qty {
  font-weight: 700;
  color: var(--pandan);
}

.small {
  font-size: 0.875rem;
}

.center {
  text-align: center;
}

@media (prefers-reduced-motion: no-preference) {
  .ticket {
    animation: ticket-in 0.5s cubic-bezier(0.2, 0.9, 0.3, 1.2) both;
  }

  @keyframes ticket-in {
    from {
      transform: translateY(-16px) scale(0.96);
      opacity: 0;
    }
  }
}
</style>
