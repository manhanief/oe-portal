<script setup lang="ts">
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import { computed, onMounted, ref, watch } from 'vue'
import { errorMessage } from '@/api/http'
import { adminApi } from '@/api/staff'
import type { DailySales } from '@/types/staff'
import { formatMoney } from '@/utils/money'

const day = ref<Date>(new Date())
const sales = ref<DailySales | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

/** Date -> "2026-10-12" in local time (not UTC) */
function toIsoDate(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

const isToday = computed(() => toIsoDate(day.value) === toIsoDate(new Date()))

const heading = computed(() =>
  isToday.value
    ? 'Today'
    : day.value.toLocaleDateString('en-MY', { weekday: 'long', day: 'numeric', month: 'long' }),
)

const maxQuantity = computed(() => Math.max(1, ...(sales.value?.topItems.map((t) => t.quantity) ?? [1])))

async function load() {
  loading.value = true
  error.value = null
  try {
    sales.value = await adminApi.reports.daily(toIsoDate(day.value))
  } catch (e) {
    error.value = errorMessage(e, 'Sales could not be loaded.')
  } finally {
    loading.value = false
  }
}

onMounted(load)
watch(day, load)
</script>

<template>
  <div class="admin-page">
    <div class="admin-head">
      <h1>Sales: {{ heading }}</h1>
      <div class="admin-tools">
        <DatePicker
          v-model="day"
          :max-date="new Date()"
          date-format="dd M yy"
          show-icon
          aria-label="Choose a day"
        />
        <Button icon="pi pi-refresh" outlined aria-label="Refresh" :loading="loading" @click="load" />
      </div>
    </div>

    <Message v-if="error" severity="error">{{ error }}</Message>

    <div v-if="loading && !sales" class="stats">
      <Skeleton v-for="n in 4" :key="n" height="6rem" border-radius="14px" />
    </div>

    <template v-else-if="sales">
      <!-- Revenue leads; the rest support it -->
      <div class="stats">
        <div class="stat lead">
          <span class="stat-label">Revenue</span>
          <span class="stat-value">{{ formatMoney(sales.revenue) }}</span>
        </div>
        <div class="stat">
          <span class="stat-label">Orders</span>
          <span class="stat-value">{{ sales.orderCount }}</span>
        </div>
        <div class="stat">
          <span class="stat-label">Average order</span>
          <span class="stat-value">{{ formatMoney(sales.averageOrder) }}</span>
        </div>
        <div class="stat">
          <span class="stat-label">Cancelled</span>
          <span class="stat-value">{{ sales.cancelledCount }}</span>
        </div>
      </div>

      <section class="panel" aria-labelledby="top-title">
        <h2 id="top-title">Best sellers</h2>
        <p v-if="sales.topItems.length === 0" class="muted">No orders on this day yet.</p>
        <ol v-else class="top">
          <li v-for="item in sales.topItems" :key="item.name">
            <div class="top-row">
              <span class="top-name">{{ item.name }}</span>
              <span class="muted">{{ item.quantity }} sold</span>
              <span class="price">{{ formatMoney(item.revenue) }}</span>
            </div>
            <div class="bar" aria-hidden="true">
              <span :style="{ width: `${(item.quantity / maxQuantity) * 100}%` }" />
            </div>
          </li>
        </ol>
      </section>

      <p class="muted small">Revenue includes SST and leaves out cancelled orders.</p>
    </template>
  </div>
</template>

<style scoped>
.stats {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.stat {
  display: grid;
  gap: 4px;
  padding: 16px 18px;
  border-radius: var(--radius-m);
  background: var(--paper);
  border: 1px solid var(--line);
}

.stat.lead {
  grid-column: 1 / -1;
  background: var(--pandan);
  border-color: var(--pandan);
  color: #fff;
}

.stat-label {
  font-weight: 600;
  color: var(--muted);
}

.lead .stat-label {
  color: rgb(255 255 255 / 0.8);
}

.stat-value {
  font-family: var(--font-display);
  font-size: 1.75rem;
  font-weight: 750;
  font-variant-numeric: tabular-nums;
}

.lead .stat-value {
  font-size: 2.6rem;
}

h2 {
  font-size: 1.25rem;
  margin-bottom: 12px;
}

.top {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 14px;
}

.top-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto 100px;
  gap: 12px;
  align-items: baseline;
}

.top-name {
  font-weight: 600;
}

.top-row .price {
  text-align: right;
}

.bar {
  height: 8px;
  margin-top: 6px;
  border-radius: 999px;
  background: var(--pandan-wash);
  overflow: hidden;
}

.bar span {
  display: block;
  height: 100%;
  border-radius: 999px;
  background: var(--pandan);
}

.small {
  font-size: 0.875rem;
}

@media (min-width: 900px) {
  .stats {
    grid-template-columns: 1.6fr repeat(3, minmax(0, 1fr));
  }

  .stat.lead {
    grid-column: auto;
  }
}
</style>
