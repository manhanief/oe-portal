<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { RESTAURANT_NAME } from '@/config'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

const links = computed(() => [
  { to: { name: 'kitchen' }, label: 'Kitchen', icon: 'pi pi-th-large', show: true },
  { to: { name: 'admin-menu' }, label: 'Menu', icon: 'pi pi-book', show: auth.isAdmin },
  { to: { name: 'admin-categories' }, label: 'Categories', icon: 'pi pi-tags', show: auth.isAdmin },
  { to: { name: 'admin-tables' }, label: 'Tables', icon: 'pi pi-table', show: auth.isAdmin },
  { to: { name: 'admin-users' }, label: 'Staff', icon: 'pi pi-users', show: auth.isAdmin },
  { to: { name: 'admin-sales' }, label: 'Sales', icon: 'pi pi-chart-bar', show: auth.isAdmin },
])

async function logOut() {
  await auth.logout()
  await router.push({ name: 'staff-login' })
}
</script>

<template>
  <div class="staff">
    <header class="bar">
      <div class="inner">
        <RouterLink :to="{ name: 'kitchen' }" class="brand">
          <img src="/favicon.svg" alt="" width="28" height="28" />
          <span>{{ RESTAURANT_NAME }} <span class="role">staff</span></span>
        </RouterLink>

        <nav aria-label="Staff pages">
          <template v-for="link in links" :key="link.label">
            <RouterLink v-if="link.show" :to="link.to" class="nav-link">
              <i :class="link.icon" aria-hidden="true" />
              <span>{{ link.label }}</span>
            </RouterLink>
          </template>
        </nav>

        <div class="who">
          <span class="user"><i class="pi pi-user" aria-hidden="true" /> {{ auth.user?.username }}</span>
          <button type="button" class="logout" @click="logOut">Log out</button>
        </div>
      </div>
    </header>

    <RouterView />
  </div>
</template>

<style scoped>
.bar {
  position: sticky;
  top: 0;
  z-index: 20;
  background: var(--ink);
  color: #fff;
}

.inner {
  max-width: 1400px;
  margin: 0 auto;
  padding: 10px var(--gutter);
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px 20px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  color: inherit;
  text-decoration: none;
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.1rem;
}

.role {
  font-family: var(--font-body);
  font-weight: 500;
  font-size: 0.85rem;
  color: var(--kaya);
  margin-left: 4px;
}

nav {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  order: 3;
  width: 100%;
  scrollbar-width: none;
}

.nav-link {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
  padding: 8px 12px;
  border-radius: var(--radius-s);
  color: rgb(255 255 255 / 0.75);
  text-decoration: none;
  font-weight: 600;
}

.nav-link:hover {
  color: #fff;
  background: rgb(255 255 255 / 0.08);
}

.nav-link.router-link-active {
  color: var(--ink);
  background: var(--kaya);
}

.who {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.9rem;
}

.user {
  color: rgb(255 255 255 / 0.75);
}

.logout {
  padding: 6px 12px;
  border: 1px solid rgb(255 255 255 / 0.35);
  border-radius: var(--radius-s);
  background: transparent;
  color: #fff;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}

.logout:hover {
  background: rgb(255 255 255 / 0.1);
}

@media (min-width: 1100px) {
  nav {
    order: 0;
    width: auto;
  }
}
</style>
