<script setup lang="ts">
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import ToggleSwitch from 'primevue/toggleswitch'
import { onMounted, reactive, ref } from 'vue'
import { adminApi } from '@/api/staff'
import { useAdminFeedback } from '@/composables/useAdminFeedback'
import type { Category, SaveCategoryRequest } from '@/types/staff'

const feedback = useAdminFeedback()

const categories = ref<Category[]>([])
const loading = ref(true)
const loadError = ref<string | null>(null)

function sortList() {
  categories.value.sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name))
}

async function load() {
  loading.value = true
  loadError.value = null
  try {
    categories.value = await adminApi.categories.list()
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'Categories could not be loaded.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// ---------- Add / edit dialog ----------
const dialogOpen = ref(false)
const editingId = ref<number | null>(null)
const saving = ref(false)
const errors = ref<Record<string, string>>({})
const form = reactive<SaveCategoryRequest>({ name: '', sortOrder: 0, isActive: true })

function openNew() {
  editingId.value = null
  const nextOrder = Math.max(0, ...categories.value.map((c) => c.sortOrder)) + 1
  Object.assign(form, { name: '', sortOrder: nextOrder, isActive: true })
  errors.value = {}
  dialogOpen.value = true
}

function openEdit(category: Category) {
  editingId.value = category.id
  Object.assign(form, { name: category.name, sortOrder: category.sortOrder, isActive: category.isActive })
  errors.value = {}
  dialogOpen.value = true
}

async function save() {
  saving.value = true
  errors.value = {}
  try {
    if (editingId.value === null) {
      const created = await adminApi.categories.create({ ...form })
      categories.value.push(created)
      feedback.saved(`${created.name} added`)
    } else {
      const updated = await adminApi.categories.update(editingId.value, { ...form })
      categories.value = categories.value.map((c) => (c.id === updated.id ? updated : c))
      feedback.saved(`${updated.name} saved`)
    }
    sortList()
    dialogOpen.value = false
  } catch (e) {
    errors.value = feedback.fieldErrors(e)
    if (Object.keys(errors.value).length === 0) feedback.failed('Could not save the category', e)
  } finally {
    saving.value = false
  }
}

async function toggleActive(category: Category, isActive: boolean) {
  try {
    const updated = await adminApi.categories.update(category.id, {
      name: category.name,
      sortOrder: category.sortOrder,
      isActive,
    })
    categories.value = categories.value.map((c) => (c.id === updated.id ? updated : c))
    feedback.saved(isActive ? `${category.name} is shown on the menu` : `${category.name} is hidden`)
  } catch (e) {
    feedback.failed(`Could not update ${category.name}`, e)
  }
}

function remove(category: Category) {
  feedback.confirmDelete(category.name, async () => {
    try {
      await adminApi.categories.remove(category.id)
      categories.value = categories.value.filter((c) => c.id !== category.id)
      feedback.saved(`${category.name} deleted`)
    } catch (e) {
      feedback.failed(`Could not delete ${category.name}`, e)
    }
  })
}
</script>

<template>
  <div class="admin-page">
    <div class="admin-head">
      <h1>Categories</h1>
      <Button label="Add category" icon="pi pi-plus" @click="openNew" />
    </div>

    <p class="muted">Categories appear on the customer menu in this order. Hidden categories are not shown.</p>

    <Message v-if="loadError" severity="error">{{ loadError }}</Message>

    <DataTable :value="categories" :loading="loading" data-key="id" size="small" class="panel-table">
      <template #empty><p class="muted">No categories yet.</p></template>

      <Column field="sortOrder" header="Order" style="width: 80px" />
      <Column field="name" header="Name">
        <template #body="{ data }">
          <strong>{{ data.name }}</strong>
        </template>
      </Column>
      <Column field="itemCount" header="Dishes" />
      <Column header="Shown on menu">
        <template #body="{ data }">
          <ToggleSwitch
            :model-value="data.isActive"
            :aria-label="`Show ${data.name} on the menu`"
            @update:model-value="(v: boolean) => toggleActive(data, v)"
          />
        </template>
      </Column>
      <Column header="">
        <template #body="{ data }">
          <div class="row-actions">
            <Button icon="pi pi-pencil" text rounded :aria-label="`Edit ${data.name}`" @click="openEdit(data)" />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              :aria-label="`Delete ${data.name}`"
              @click="remove(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogOpen"
      modal
      :header="editingId === null ? 'Add category' : 'Edit category'"
      :style="{ width: 'min(440px, 94vw)' }"
    >
      <form id="category-form" class="form-grid" novalidate @submit.prevent="save">
        <div class="form-field">
          <label for="cat-name">Name</label>
          <InputText id="cat-name" v-model="form.name" maxlength="50" :invalid="!!errors.Name" fluid />
          <small v-if="errors.Name" class="field-error">{{ errors.Name }}</small>
        </div>

        <div class="form-field">
          <label for="cat-order">Position on the menu</label>
          <InputNumber v-model="form.sortOrder" input-id="cat-order" :min="0" :max="999" show-buttons fluid />
          <small class="muted">Lower numbers appear first.</small>
        </div>

        <div class="toggle-row">
          <ToggleSwitch v-model="form.isActive" input-id="cat-active" />
          <label for="cat-active">Show on the customer menu</label>
        </div>
      </form>

      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="dialogOpen = false" />
        <Button
          type="submit"
          form="category-form"
          :label="editingId === null ? 'Add category' : 'Save'"
          :loading="saving"
        />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
}
</style>
