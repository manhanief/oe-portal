<script setup lang="ts">
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { errorMessage } from '@/api/http'
import { RESTAURANT_NAME } from '@/config'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const username = ref('')
const password = ref('')
const error = ref<string | null>(null)
const busy = ref(false)

async function submit() {
  error.value = null
  if (!username.value.trim() || !password.value) {
    error.value = 'Enter your username and password.'
    return
  }

  busy.value = true
  try {
    await auth.login(username.value, password.value)
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/staff/kitchen'
    await router.replace(redirect.startsWith('/staff') ? redirect : '/staff/kitchen')
  } catch (e) {
    error.value = errorMessage(e)
    password.value = ''
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <form class="card" novalidate @submit.prevent="submit">
      <div class="head">
        <img src="/favicon.svg" alt="" width="44" height="44" />
        <div>
          <h1>Staff login</h1>
          <p class="muted">{{ RESTAURANT_NAME }}</p>
        </div>
      </div>

      <div class="field">
        <label for="username">Username</label>
        <InputText id="username" v-model="username" autocomplete="username" autofocus fluid />
      </div>

      <div class="field">
        <label for="password">Password</label>
        <Password
          v-model="password"
          input-id="password"
          :feedback="false"
          toggle-mask
          :input-props="{ autocomplete: 'current-password' }"
          fluid
        />
      </div>

      <Message v-if="error" severity="error">{{ error }}</Message>

      <Button type="submit" label="Log in" size="large" :loading="busy" fluid />

      <RouterLink to="/" class="customer-link">Go to the customer menu</RouterLink>
    </form>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px var(--gutter);
  background: var(--ink);
}

.card {
  width: 100%;
  max-width: 400px;
  display: grid;
  gap: 18px;
  padding: 28px;
  border-radius: var(--radius-m);
  background: var(--paper);
}

.head {
  display: flex;
  align-items: center;
  gap: 14px;
}

.head img {
  border-radius: 10px;
}

h1 {
  font-size: 1.6rem;
}

.field {
  display: grid;
  gap: 6px;
}

.field label {
  font-weight: 600;
}

.customer-link {
  justify-self: center;
  font-size: 0.9rem;
}
</style>
