import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { ApiError } from '@/api/http'
import { authApi } from '@/api/staff'
import type { CurrentUser } from '@/types/staff'

/**
 * Who is logged in. The login itself lives in an HttpOnly cookie that
 * JavaScript can't read, so we ask the API (/auth/me) instead.
 */
export const useAuthStore = defineStore('auth', () => {
  const user = ref<CurrentUser | null>(null)
  const checked = ref(false)

  const isLoggedIn = computed(() => user.value !== null)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  /** Ask the API once per page load whether the cookie is still valid. */
  async function ensureChecked() {
    if (checked.value) return
    try {
      user.value = await authApi.me()
    } catch (e) {
      if (!(e instanceof ApiError) || e.status !== 401) console.warn('Could not check login', e)
      user.value = null
    } finally {
      checked.value = true
    }
  }

  async function login(username: string, password: string) {
    user.value = await authApi.login(username, password)
    checked.value = true
  }

  async function logout() {
    try {
      await authApi.logout()
    } finally {
      user.value = null
    }
  }

  /** Called when the API says the session has expired. */
  function forget() {
    user.value = null
  }

  return { user, isLoggedIn, isAdmin, ensureChecked, login, logout, forget }
})
