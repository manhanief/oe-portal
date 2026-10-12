<script setup lang="ts">
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { errorMessage } from '@/api/http'
import { kitchenApi } from '@/api/staff'
import KitchenOrderCard from '@/components/staff/KitchenOrderCard.vue'
import { ORDER_REFRESH_MS } from '@/config'
import type { Order, OrderStatus } from '@/types/api'

const toast = useToast()
const confirm = useConfirm()

const orders = ref<Order[]>([])
const loading = ref(true)
const connectionError = ref<string | null>(null)
const lastUpdated = ref<Date | null>(null)
const busyId = ref<number | null>(null)
const now = ref(Date.now())

// Highlight orders that arrived since the last refresh
const knownIds = new Set<number>()
const freshIds = ref(new Set<number>())

const columns: { status: OrderStatus; title: string; empty: string }[] = [
  { status: 'Pending', title: 'New', empty: 'No new orders' },
  { status: 'Preparing', title: 'Preparing', empty: 'Nothing cooking' },
  { status: 'Ready', title: 'Ready', empty: 'Nothing waiting' },
]

const byStatus = computed(() => {
  const groups: Record<string, Order[]> = { Pending: [], Preparing: [], Ready: [] }
  for (const order of orders.value) groups[order.status]?.push(order)
  return groups
})

let refreshTimer: number | undefined
let clockTimer: number | undefined

async function refresh() {
  try {
    const latest = await kitchenApi.activeOrders()

    const firstLoad = knownIds.size === 0 && loading.value
    const fresh = new Set<number>()
    for (const o of latest) {
      if (!knownIds.has(o.id) && !firstLoad) fresh.add(o.id)
      knownIds.add(o.id)
    }
    if (fresh.size > 0) {
      freshIds.value = fresh
      toast.add({
        severity: 'info',
        summary: fresh.size === 1 ? 'New order' : `${fresh.size} new orders`,
        life: 3000,
      })
    }

    orders.value = latest
    lastUpdated.value = new Date()
    connectionError.value = null
  } catch (e) {
    connectionError.value = `${errorMessage(e)} Retrying…`
  } finally {
    loading.value = false
    window.clearTimeout(refreshTimer)
    refreshTimer = window.setTimeout(refresh, ORDER_REFRESH_MS)
  }
}

async function changeStatus(order: Order, status: OrderStatus) {
  busyId.value = order.id
  try {
    const updated = await kitchenApi.setStatus(order.id, status)
    orders.value =
      updated.status === 'Completed' || updated.status === 'Cancelled'
        ? orders.value.filter((o) => o.id !== updated.id)
        : orders.value.map((o) => (o.id === updated.id ? updated : o))
  } catch (e) {
    toast.add({ severity: 'error', summary: `Could not update ${order.orderNo}`, detail: errorMessage(e), life: 5000 })
    refresh()
  } finally {
    busyId.value = null
  }
}

function askCancel(order: Order) {
  confirm.require({
    header: `Cancel order ${order.orderNo}?`,
    message: 'The customer will see that the order was cancelled. This cannot be undone.',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Keep order', severity: 'secondary', outlined: true },
    acceptProps: { label: 'Cancel order', severity: 'danger' },
    accept: () => changeStatus(order, 'Cancelled'),
  })
}

onMounted(() => {
  refresh()
  clockTimer = window.setInterval(() => (now.value = Date.now()), 30_000)
})

onUnmounted(() => {
  window.clearTimeout(refreshTimer)
  window.clearInterval(clockTimer)
})
</script>

<template>
  <div class="board-page">
    <div class="toolbar">
      <h1>Kitchen</h1>
      <p class="muted status-line" aria-live="polite">
        <span class="live-dot" :class="{ off: connectionError }" aria-hidden="true" />
        <template v-if="lastUpdated">Updated {{ lastUpdated.toLocaleTimeString('en-MY') }}</template>
      </p>
    </div>

    <Message v-if="connectionError" severity="warn">{{ connectionError }}</Message>

    <div class="columns">
      <section
        v-for="col in columns"
        :key="col.status"
        class="column"
        :aria-labelledby="`col-${col.status}`"
      >
        <h2 :id="`col-${col.status}`">
          {{ col.title }}
          <span class="count">{{ byStatus[col.status]?.length ?? 0 }}</span>
        </h2>

        <div v-if="loading" class="stack">
          <Skeleton height="12rem" border-radius="14px" />
        </div>

        <div v-else class="stack">
          <KitchenOrderCard
            v-for="order in byStatus[col.status]"
            :key="order.id"
            :order="order"
            :now="now"
            :busy="busyId === order.id"
            :fresh="freshIds.has(order.id)"
            @advance="(status) => changeStatus(order, status)"
            @cancel="askCancel(order)"
          />
          <p v-if="(byStatus[col.status]?.length ?? 0) === 0" class="empty muted">{{ col.empty }}</p>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.board-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px var(--gutter) 60px;
  display: grid;
  gap: 16px;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12px;
  flex-wrap: wrap;
}

h1 {
  font-size: 2rem;
  font-weight: 750;
}

.status-line {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
}

.live-dot {
  width: 9px;
  height: 9px;
  border-radius: 999px;
  background: var(--pandan);
}

.live-dot.off {
  background: var(--chilli);
}

.columns {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 20px;
  align-items: start;
}

.column h2 {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.25rem;
  margin-bottom: 12px;
}

.count {
  min-width: 28px;
  padding: 2px 8px;
  border-radius: 999px;
  background: var(--ink);
  color: #fff;
  font-size: 0.9rem;
  text-align: center;
}

.stack {
  display: grid;
  gap: 12px;
}

.empty {
  padding: 28px 12px;
  border: 2px dashed var(--line);
  border-radius: var(--radius-m);
  text-align: center;
}

@media (min-width: 900px) {
  .columns {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
</style>
