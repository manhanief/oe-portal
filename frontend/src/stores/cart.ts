import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import { TAX_RATE } from '@/config'
import type { CreateOrderRequest, MenuItem, OrderType } from '@/types/api'
import { round2 } from '@/utils/money'

/**
 * The cart, shared by every page (Pinia store = like a global state container).
 * It is saved in the browser (localStorage) so a page refresh doesn't empty it.
 */

export interface CartLine {
  key: string // unique per line: the same dish can appear twice with different notes
  itemId: number
  name: string
  price: number // preview only; the server uses the real database price
  quantity: number
  notes: string
}

interface SavedCart {
  lines: CartLine[]
  orderType: OrderType
  tableId: number | null
  customerName: string
  lastOrderId: number | null
}

const STORAGE_KEY = 'resto-cart-v1'
export const MAX_QUANTITY = 99

function load(): SavedCart | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as SavedCart) : null
  } catch {
    return null
  }
}

function newKey(): string {
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`
}

export const useCartStore = defineStore('cart', () => {
  const saved = load()

  const lines = ref<CartLine[]>(saved?.lines ?? [])
  const orderType = ref<OrderType>(saved?.orderType ?? 'DineIn')
  const tableId = ref<number | null>(saved?.tableId ?? null)
  const customerName = ref(saved?.customerName ?? '')
  const lastOrderId = ref<number | null>(saved?.lastOrderId ?? null)

  // ---------- totals ----------
  const itemCount = computed(() => lines.value.reduce((sum, l) => sum + l.quantity, 0))
  const subtotal = computed(() => round2(lines.value.reduce((sum, l) => sum + l.price * l.quantity, 0)))
  const tax = computed(() => round2(subtotal.value * TAX_RATE))
  const total = computed(() => round2(subtotal.value + tax.value))
  const isEmpty = computed(() => lines.value.length === 0)

  /** How many of this dish are in the cart (all lines together), for the menu badges. */
  function quantityOf(itemId: number): number {
    return lines.value.filter((l) => l.itemId === itemId).reduce((sum, l) => sum + l.quantity, 0)
  }

  // ---------- actions ----------
  function add(item: MenuItem) {
    // Merge into an existing line without notes; otherwise start a new line
    const plain = lines.value.find((l) => l.itemId === item.id && l.notes === '')
    if (plain) {
      plain.quantity = Math.min(MAX_QUANTITY, plain.quantity + 1)
    } else {
      lines.value.push({ key: newKey(), itemId: item.id, name: item.name, price: item.price, quantity: 1, notes: '' })
    }
  }

  /** Remove one of this dish (from the most recently added line). */
  function removeOne(itemId: number) {
    const line = [...lines.value].reverse().find((l) => l.itemId === itemId)
    if (line) setQuantity(line.key, line.quantity - 1)
  }

  function setQuantity(key: string, quantity: number) {
    const line = lines.value.find((l) => l.key === key)
    if (!line) return
    if (quantity <= 0) {
      lines.value = lines.value.filter((l) => l.key !== key)
    } else {
      line.quantity = Math.min(MAX_QUANTITY, quantity)
    }
  }

  function setNotes(key: string, notes: string) {
    const line = lines.value.find((l) => l.key === key)
    if (line) line.notes = notes.slice(0, 200)
  }

  /** Builds the request body for POST /api/orders. Note: no prices are sent. */
  function toOrderRequest(): CreateOrderRequest {
    const isDineIn = orderType.value === 'DineIn'
    return {
      orderType: orderType.value,
      tableId: isDineIn ? tableId.value : null,
      customerName: customerName.value.trim() || null,
      items: lines.value.map((l) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        notes: l.notes.trim() || null,
      })),
    }
  }

  function clearAfterOrder(orderId: number) {
    lines.value = []
    lastOrderId.value = orderId
  }

  // ---------- save to the browser on every change ----------
  watch(
    [lines, orderType, tableId, customerName, lastOrderId],
    () => {
      const data: SavedCart = {
        lines: lines.value,
        orderType: orderType.value,
        tableId: tableId.value,
        customerName: customerName.value,
        lastOrderId: lastOrderId.value,
      }
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
      } catch {
        // storage full or blocked (private mode): the cart still works for this visit
      }
    },
    { deep: true },
  )

  return {
    lines,
    orderType,
    tableId,
    customerName,
    lastOrderId,
    itemCount,
    subtotal,
    tax,
    total,
    isEmpty,
    quantityOf,
    add,
    removeOne,
    setQuantity,
    setNotes,
    toOrderRequest,
    clearAfterOrder,
  }
})
