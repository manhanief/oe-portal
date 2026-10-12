<script setup lang="ts">
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import SelectButton from 'primevue/selectbutton'
import { onMounted, reactive, ref } from 'vue'
import { adminApi } from '@/api/staff'
import { useAdminFeedback } from '@/composables/useAdminFeedback'
import { useAuthStore } from '@/stores/auth'
import type { CreateUserRequest, StaffUser } from '@/types/staff'

const feedback = useAdminFeedback()
const auth = useAuthStore()

const users = ref<StaffUser[]>([])
const loading = ref(true)
const loadError = ref<string | null>(null)

const roleOptions = [
  { label: 'Kitchen staff', value: 'Staff' },
  { label: 'Admin', value: 'Admin' },
]

async function load() {
  loading.value = true
  loadError.value = null
  try {
    users.value = await adminApi.users.list()
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'Accounts could not be loaded.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// ---------- New account ----------
const createOpen = ref(false)
const creating = ref(false)
const createErrors = ref<Record<string, string>>({})
const createForm = reactive<CreateUserRequest>({ username: '', password: '', role: 'Staff' })

function openCreate() {
  Object.assign(createForm, { username: '', password: '', role: 'Staff' })
  createErrors.value = {}
  createOpen.value = true
}

async function create() {
  creating.value = true
  createErrors.value = {}
  try {
    const created = await adminApi.users.create({ ...createForm })
    users.value = [...users.value, created].sort((a, b) => a.username.localeCompare(b.username))
    feedback.saved(`Account ${created.username} created`)
    createOpen.value = false
  } catch (e) {
    createErrors.value = feedback.fieldErrors(e)
    if (Object.keys(createErrors.value).length === 0) feedback.failed('Could not create the account', e)
  } finally {
    creating.value = false
  }
}

// ---------- Reset password ----------
const resetFor = ref<StaffUser | null>(null)
const resetPassword = ref('')
const resetError = ref<string | null>(null)
const resetting = ref(false)

function openReset(user: StaffUser) {
  resetFor.value = user
  resetPassword.value = ''
  resetError.value = null
}

async function reset() {
  if (!resetFor.value) return
  resetting.value = true
  resetError.value = null
  try {
    await adminApi.users.resetPassword(resetFor.value.id, resetPassword.value)
    feedback.saved(`Password changed for ${resetFor.value.username}`)
    resetFor.value = null
  } catch (e) {
    resetError.value = Object.values(feedback.fieldErrors(e))[0] ?? null
    if (!resetError.value) feedback.failed('Could not change the password', e)
  } finally {
    resetting.value = false
  }
}

function remove(user: StaffUser) {
  feedback.confirmDelete(`the account ${user.username}`, async () => {
    try {
      await adminApi.users.remove(user.id)
      users.value = users.value.filter((u) => u.id !== user.id)
      feedback.saved(`Account ${user.username} deleted`)
    } catch (e) {
      feedback.failed(`Could not delete ${user.username}`, e)
    }
  })
}
</script>

<template>
  <div class="admin-page narrow">
    <div class="admin-head">
      <h1>Staff accounts</h1>
      <Button label="Add account" icon="pi pi-user-plus" @click="openCreate" />
    </div>

    <p class="muted">Kitchen staff can use the kitchen board. Admins can also change the menu, tables, accounts and see sales.</p>

    <Message v-if="loadError" severity="error">{{ loadError }}</Message>

    <DataTable :value="users" :loading="loading" data-key="id" size="small" class="panel-table">
      <Column field="username" header="Username">
        <template #body="{ data }">
          <strong>{{ data.username }}</strong>
          <span v-if="data.id === auth.user?.id" class="muted"> (you)</span>
        </template>
      </Column>
      <Column header="Role">
        <template #body="{ data }">
          <span class="pill" :class="{ off: data.role === 'Staff' }">
            {{ data.role === 'Admin' ? 'Admin' : 'Kitchen staff' }}
          </span>
        </template>
      </Column>
      <Column header="">
        <template #body="{ data }">
          <div class="row-actions">
            <Button
              icon="pi pi-key"
              text
              rounded
              :aria-label="`Change password for ${data.username}`"
              title="Change password"
              @click="openReset(data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              :disabled="data.id === auth.user?.id"
              :aria-label="`Delete ${data.username}`"
              @click="remove(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <!-- New account -->
    <Dialog v-model:visible="createOpen" modal header="Add account" :style="{ width: 'min(420px, 94vw)' }">
      <form id="user-form" class="form-grid" novalidate @submit.prevent="create">
        <div class="form-field">
          <label for="user-name">Username</label>
          <InputText
            id="user-name"
            v-model="createForm.username"
            autocomplete="off"
            maxlength="50"
            :invalid="!!createErrors.Username"
            fluid
          />
          <small v-if="createErrors.Username" class="field-error">{{ createErrors.Username }}</small>
        </div>

        <div class="form-field">
          <label for="user-password">Password</label>
          <Password
            v-model="createForm.password"
            input-id="user-password"
            :feedback="false"
            toggle-mask
            :input-props="{ autocomplete: 'new-password' }"
            :invalid="!!createErrors.Password"
            fluid
          />
          <small v-if="createErrors.Password" class="field-error">{{ createErrors.Password }}</small>
          <small v-else class="muted">At least 8 characters.</small>
        </div>

        <div class="form-field">
          <span class="label" id="user-role-label">Role</span>
          <SelectButton
            v-model="createForm.role"
            :options="roleOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-labelledby="user-role-label"
            fluid
          />
        </div>
      </form>

      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="createOpen = false" />
        <Button type="submit" form="user-form" label="Add account" :loading="creating" />
      </template>
    </Dialog>

    <!-- Reset password -->
    <Dialog
      :visible="resetFor !== null"
      modal
      :header="`Change password for ${resetFor?.username ?? ''}`"
      :style="{ width: 'min(420px, 94vw)' }"
      @update:visible="(v: boolean) => { if (!v) resetFor = null }"
    >
      <form id="reset-form" class="form-grid" novalidate @submit.prevent="reset">
        <div class="form-field">
          <label for="reset-password">New password</label>
          <Password
            v-model="resetPassword"
            input-id="reset-password"
            :feedback="false"
            toggle-mask
            :input-props="{ autocomplete: 'new-password' }"
            :invalid="!!resetError"
            fluid
          />
          <small v-if="resetError" class="field-error">{{ resetError }}</small>
          <small v-else class="muted">At least 8 characters.</small>
        </div>
      </form>

      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="resetFor = null" />
        <Button type="submit" form="reset-form" label="Change password" :loading="resetting" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.narrow {
  max-width: 720px;
}

.label {
  font-weight: 600;
}
</style>
