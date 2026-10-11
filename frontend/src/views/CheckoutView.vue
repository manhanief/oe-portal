<script setup lang="ts">
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import SelectButton from 'primevue/selectbutton'
import { useToast } from 'primevue/usetoast'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '@/api/http'
import { restaurantApi } from '@/api/restaurant'
import QuantityStepper from '@/components/QuantityStepper.vue'
import TotalsList from '@/components/TotalsList.vue'
import { useCartStore } from '@/stores/cart'
import type { RestaurantTable } from '@/types/api'
import { formatMoney } from '@/utils/money'

const cart = useCartStore()
const router = useRouter()
const toast = useToast()

const tables = ref<RestaurantTable[]>([])
const tablesError = ref<string | null>(null)
const submitting = ref(false)
const triedToSubmit = ref(false)
const serverError = ref<string | null>(null)

const orderTypeOptions = [
  { label: 'Dine in', value: 'DineIn' },
  { label: 'Take away', value: 'Takeaway' },
]

const tableOptions = computed(() =>
  tables.value.map((t) => ({ label: `${t.tableNo} (${t.seats} seats)`, value: t.id })),
)

const isDineIn = computed(() => cart.orderType === 'DineIn')

// Same rules as the server's validator, checked here first for instant feedback
const tableError = computed(() => (isDineIn.value && cart.tableId === null ? 'Choose your table.' : null))
const nameError = computed(() =>
  !isDineIn.value && cart.customerName.trim() === '' ? 'Enter a name so we can call you.' : null,
)

onMounted(async () => {
  try {
    tables.value = await restaurantApi.getTables()
    // Forget a saved table that no longer exists
    if (cart.tableId !== null && !tables.value.some((t) => t.id === cart.tableId)) cart.tableId = null
  } catch (e) {
    tablesError.value = e instanceof ApiError ? e.message : 'Tables could not be loaded.'
  }
})

async function placeOrder() {
  triedToSubmit.value = true
  serverError.value = null
  if (tableError.value || nameError.value) return

  submitting.value = true
  try {
    const order = await restaurantApi.createOrder(cart.toOrderRequest())
    cart.clearAfterOrder(order.id)
    toast.add({ severity: 'success', summary: `Order ${order.orderNo} placed`, life: 4000 })
    await router.push({ name: 'order', params: { id: order.id } })
  } catch (e) {
    serverError.value = e instanceof ApiError ? e.message : 'Your order could not be placed. Please try again.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="page checkout">
    <RouterLink to="/" class="back"><i class="pi pi-arrow-left" aria-hidden="true" /> Back to menu</RouterLink>

    <h1>Review your order</h1>

    <!-- Empty cart -->
    <div v-if="cart.isEmpty" class="panel empty">
      <p>Your order is empty.</p>
      <Button label="Browse the menu" @click="router.push('/')" />
    </div>

    <form v-else class="grid" novalidate @submit.prevent="placeOrder">
      <!-- Items -->
      <section class="panel" aria-labelledby="items-title">
        <h2 id="items-title">Items</h2>
        <ul class="lines">
          <li v-for="line in cart.lines" :key="line.key" class="line">
            <div class="line-top">
              <div>
                <p class="line-name">{{ line.name }}</p>
                <p class="muted small">{{ formatMoney(line.price) }} each</p>
              </div>
              <QuantityStepper
                :quantity="line.quantity"
                :item-name="line.name"
                @increase="cart.setQuantity(line.key, line.quantity + 1)"
                @decrease="cart.setQuantity(line.key, line.quantity - 1)"
              />
              <p class="price line-total">{{ formatMoney(line.price * line.quantity) }}</p>
            </div>
            <label :for="`note-${line.key}`" class="visually-hidden">Note for {{ line.name }}</label>
            <InputText
              :id="`note-${line.key}`"
              :model-value="line.notes"
              placeholder="Add a note, e.g. less spicy"
              maxlength="200"
              size="small"
              fluid
              @update:model-value="(v) => cart.setNotes(line.key, v ?? '')"
            />
          </li>
        </ul>
      </section>

      <div class="side">
        <!-- Dine in / take away -->
        <section class="panel fields" aria-labelledby="how-title">
          <h2 id="how-title">How are you eating?</h2>

          <SelectButton
            v-model="cart.orderType"
            :options="orderTypeOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-labelledby="how-title"
            fluid
          />

          <div v-if="isDineIn" class="field">
            <label for="table">Your table</label>
            <Select
              v-model="cart.tableId"
              input-id="table"
              :options="tableOptions"
              option-label="label"
              option-value="value"
              placeholder="Choose a table"
              :invalid="triedToSubmit && !!tableError"
              fluid
            />
            <small v-if="tablesError" class="error-text">{{ tablesError }}</small>
            <small v-else-if="triedToSubmit && tableError" class="error-text">{{ tableError }}</small>
            <small v-else class="muted">The number is on the sticker at your table.</small>
          </div>

          <div class="field">
            <label for="name">{{ isDineIn ? 'Your name (optional)' : 'Name for pickup' }}</label>
            <InputText
              id="name"
              v-model="cart.customerName"
              maxlength="100"
              autocomplete="given-name"
              :invalid="triedToSubmit && !!nameError"
              fluid
            />
            <small v-if="triedToSubmit && nameError" class="error-text">{{ nameError }}</small>
          </div>
        </section>

        <!-- Totals and submit -->
        <section class="panel fields" aria-label="Total">
          <TotalsList :subtotal="cart.subtotal" :tax="cart.tax" :total="cart.total" />

          <Message v-if="serverError" severity="error">{{ serverError }}</Message>

          <Button type="submit" label="Place order" size="large" :loading="submitting" fluid />
          <p class="muted small center">You pay at the counter. The final total is confirmed by the restaurant.</p>
        </section>
      </div>
    </form>
  </div>
</template>

<style scoped>
.checkout {
  display: grid;
  gap: 16px;
}

.back {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  text-decoration: none;
  width: fit-content;
}

h1 {
  font-size: clamp(1.75rem, 4.5vw, 2.4rem);
  font-weight: 750;
}

h2 {
  font-size: 1.2rem;
  margin-bottom: 4px;
}

.empty {
  display: grid;
  gap: 12px;
  justify-items: start;
}

.grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 16px;
}

.lines {
  list-style: none;
  margin: 0;
  padding: 0;
}

.line {
  display: grid;
  gap: 10px;
  padding: 16px 0;
  border-bottom: 1px solid var(--line);
}

.line:last-child {
  border-bottom: 0;
  padding-bottom: 0;
}

.line-top {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  grid-template-areas:
    'name total'
    'stepper stepper';
  gap: 10px 16px;
  align-items: center;
}

.line-top > div {
  grid-area: name;
}

.line-top > .stepper {
  grid-area: stepper;
  justify-self: start;
}

.line-total {
  grid-area: total;
  text-align: right;
}

.line-name {
  font-weight: 650;
}

.small {
  font-size: 0.875rem;
}

.center {
  text-align: center;
}

.side {
  display: grid;
  gap: 16px;
  align-content: start;
}

.fields {
  display: grid;
  gap: 14px;
}

.field {
  display: grid;
  gap: 6px;
}

.field label {
  font-weight: 600;
}

.error-text {
  color: var(--chilli);
  font-weight: 500;
}

@media (min-width: 600px) {
  .line-top {
    grid-template-columns: minmax(0, 1fr) auto 90px;
    grid-template-areas: 'name stepper total';
  }
}

@media (min-width: 960px) {
  .grid {
    grid-template-columns: minmax(0, 1fr) 380px;
    align-items: start;
  }

  .side {
    position: sticky;
    top: calc(var(--header-h) + 24px);
  }
}
</style>
