<script setup lang="ts">
import Button from 'primevue/button'
import Message from 'primevue/message'
import Skeleton from 'primevue/skeleton'
import { onMounted, ref } from 'vue'
import { ApiError } from '@/api/http'
import { restaurantApi } from '@/api/restaurant'
import CartSummary from '@/components/CartSummary.vue'
import MenuItemRow from '@/components/MenuItemRow.vue'
import MobileCartBar from '@/components/MobileCartBar.vue'
import type { MenuCategory } from '@/types/api'

const menu = ref<MenuCategory[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

async function loadMenu() {
  loading.value = true
  error.value = null
  try {
    menu.value = await restaurantApi.getMenu()
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'The menu could not be loaded.'
  } finally {
    loading.value = false
  }
}

function jumpTo(categoryId: number) {
  document.getElementById(`category-${categoryId}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

onMounted(loadMenu)
</script>

<template>
  <div class="page layout">
    <div class="main">
      <div class="intro">
        <h1>What would you like today?</h1>
        <p class="muted">Dine in or take away. Pay at the counter when you're done.</p>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="panel" aria-busy="true" aria-label="Loading menu">
        <div v-for="n in 5" :key="n" class="skeleton-row">
          <div>
            <Skeleton width="12rem" height="1.25rem" />
            <Skeleton width="18rem" height="0.9rem" class="mt" />
          </div>
          <Skeleton shape="circle" size="2.5rem" />
        </div>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="error">
        <Message severity="error">{{ error }}</Message>
        <Button label="Try again" icon="pi pi-refresh" outlined @click="loadMenu" />
      </div>

      <!-- Menu -->
      <template v-else>
        <nav class="chips" aria-label="Menu categories">
          <button v-for="cat in menu" :key="cat.id" type="button" class="chip" @click="jumpTo(cat.id)">
            {{ cat.name }}
          </button>
        </nav>

        <section
          v-for="cat in menu"
          :id="`category-${cat.id}`"
          :key="cat.id"
          class="category"
          :aria-labelledby="`category-title-${cat.id}`"
        >
          <h2 :id="`category-title-${cat.id}`">{{ cat.name }}</h2>
          <ul class="panel items">
            <MenuItemRow v-for="item in cat.items" :key="item.id" :item="item" />
          </ul>
        </section>

        <p v-if="menu.length === 0" class="muted">The menu is empty right now. Please ask our staff.</p>
      </template>
    </div>

    <div class="side">
      <CartSummary />
    </div>
  </div>

  <MobileCartBar />
</template>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr); /* lets the chip row scroll instead of widening the page */
  gap: 32px;
}

.intro {
  display: grid;
  gap: 6px;
  margin-bottom: 20px;
}

.intro h1 {
  font-size: clamp(1.9rem, 5vw, 2.75rem);
  font-weight: 750;
  max-width: 18ch;
}

.chips {
  position: sticky;
  top: var(--header-h);
  z-index: 10;
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding: 10px 0;
  margin-bottom: 8px;
  background: var(--tile);
  scrollbar-width: none;
}

.chip {
  flex-shrink: 0;
  padding: 8px 16px;
  border: 1.5px solid var(--line);
  border-radius: 999px;
  background: var(--paper);
  color: var(--ink);
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}

.chip:hover {
  border-color: var(--pandan);
  color: var(--pandan);
}

.category {
  scroll-margin-top: calc(var(--header-h) + 64px);
  margin-top: 24px;
}

.category h2 {
  font-size: 1.5rem;
  margin-bottom: 10px;
}

.items {
  list-style: none;
  margin: 0;
  padding: 0 20px;
}

.skeleton-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
}

.mt {
  margin-top: 8px;
}

.error {
  display: grid;
  gap: 12px;
  justify-items: start;
}

.side {
  display: none;
}

@media (min-width: 960px) {
  .layout {
    grid-template-columns: minmax(0, 1fr) 340px;
    align-items: start;
  }

  .side {
    display: block;
    position: sticky;
    top: calc(var(--header-h) + 24px);
  }
}
</style>
