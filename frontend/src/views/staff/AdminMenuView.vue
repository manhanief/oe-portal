<script setup lang="ts">
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import { computed, onMounted, reactive, ref } from 'vue'
import { adminApi } from '@/api/staff'
import { useAdminFeedback } from '@/composables/useAdminFeedback'
import type { AdminMenuItem, Category, SaveMenuItemRequest } from '@/types/staff'
import { formatMoney } from '@/utils/money'

const feedback = useAdminFeedback()

const items = ref<AdminMenuItem[]>([])
const categories = ref<Category[]>([])
const loading = ref(true)
const loadError = ref<string | null>(null)

const search = ref('')
const categoryFilter = ref<number | null>(null)

const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  return items.value.filter(
    (i) =>
      (categoryFilter.value === null || i.categoryId === categoryFilter.value) &&
      (term === '' || i.name.toLowerCase().includes(term)),
  )
})

const categoryOptions = computed(() => categories.value.map((c) => ({ label: c.name, value: c.id })))

async function load() {
  loading.value = true
  loadError.value = null
  try {
    ;[items.value, categories.value] = await Promise.all([adminApi.menuItems.list(), adminApi.categories.list()])
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The menu could not be loaded.'
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
const form = reactive<SaveMenuItemRequest>({
  categoryId: 0,
  name: '',
  description: null,
  price: 0,
  imageUrl: null,
  isAvailable: true,
})

const imagePreviewOk = ref(true)

function openNew() {
  editingId.value = null
  Object.assign(form, {
    categoryId: categoryFilter.value ?? categories.value[0]?.id ?? 0,
    name: '',
    description: null,
    price: 0,
    imageUrl: null,
    isAvailable: true,
  })
  errors.value = {}
  imagePreviewOk.value = true
  dialogOpen.value = true
}

function openEdit(item: AdminMenuItem) {
  editingId.value = item.id
  Object.assign(form, {
    categoryId: item.categoryId,
    name: item.name,
    description: item.description,
    price: item.price,
    imageUrl: item.imageUrl,
    isAvailable: item.isAvailable,
  })
  errors.value = {}
  imagePreviewOk.value = true
  dialogOpen.value = true
}

async function save() {
  saving.value = true
  errors.value = {}
  try {
    if (editingId.value === null) {
      const created = await adminApi.menuItems.create({ ...form })
      items.value.push(created)
      feedback.saved(`${created.name} added`)
    } else {
      const updated = await adminApi.menuItems.update(editingId.value, { ...form })
      items.value = items.value.map((i) => (i.id === updated.id ? updated : i))
      feedback.saved(`${updated.name} saved`)
    }
    dialogOpen.value = false
  } catch (e) {
    errors.value = feedback.fieldErrors(e)
    if (Object.keys(errors.value).length === 0) feedback.failed('Could not save the dish', e)
  } finally {
    saving.value = false
  }
}

// ---------- Quick sold-out toggle ----------
async function toggleAvailable(item: AdminMenuItem, isAvailable: boolean) {
  try {
    const updated = await adminApi.menuItems.update(item.id, {
      categoryId: item.categoryId,
      name: item.name,
      description: item.description,
      price: item.price,
      imageUrl: item.imageUrl,
      isAvailable,
    })
    items.value = items.value.map((i) => (i.id === updated.id ? updated : i))
    feedback.saved(isAvailable ? `${item.name} is available` : `${item.name} is sold out`)
  } catch (e) {
    feedback.failed(`Could not update ${item.name}`, e)
  }
}

function remove(item: AdminMenuItem) {
  feedback.confirmDelete(item.name, async () => {
    try {
      await adminApi.menuItems.remove(item.id)
      items.value = items.value.filter((i) => i.id !== item.id)
      feedback.saved(`${item.name} deleted`)
    } catch (e) {
      feedback.failed(`Could not delete ${item.name}`, e)
    }
  })
}
</script>

<template>
  <div class="admin-page">
    <div class="admin-head">
      <h1>Menu</h1>
      <Button label="Add dish" icon="pi pi-plus" :disabled="categories.length === 0" @click="openNew" />
    </div>

    <div class="admin-tools">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText v-model="search" placeholder="Search dishes" aria-label="Search dishes" />
      </IconField>
      <Select
        v-model="categoryFilter"
        :options="categoryOptions"
        option-label="label"
        option-value="value"
        placeholder="All categories"
        show-clear
        aria-label="Filter by category"
      />
    </div>

    <Message v-if="loadError" severity="error">{{ loadError }}</Message>

    <DataTable :value="filtered" :loading="loading" data-key="id" size="small" class="panel-table">
      <template #empty>
        <p class="muted">{{ search || categoryFilter ? 'No dishes match your search.' : 'No dishes yet. Add your first dish.' }}</p>
      </template>

      <Column header="Dish">
        <template #body="{ data }">
          <div class="dish">
            <img v-if="data.imageUrl" :src="data.imageUrl" alt="" class="thumb" loading="lazy" />
            <div>
              <strong>{{ data.name }}</strong>
              <p v-if="data.description" class="muted desc">{{ data.description }}</p>
            </div>
          </div>
        </template>
      </Column>
      <Column field="categoryName" header="Category" />
      <Column header="Price">
        <template #body="{ data }">
          <span class="price">{{ formatMoney(data.price) }}</span>
        </template>
      </Column>
      <Column header="Available">
        <template #body="{ data }">
          <ToggleSwitch
            :model-value="data.isAvailable"
            :aria-label="`${data.name} available`"
            @update:model-value="(v: boolean) => toggleAvailable(data, v)"
          />
        </template>
      </Column>
      <Column header="" class="actions-col">
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
      :header="editingId === null ? 'Add dish' : 'Edit dish'"
      :style="{ width: 'min(520px, 94vw)' }"
    >
      <form id="dish-form" class="form-grid" novalidate @submit.prevent="save">
        <div class="form-field">
          <label for="dish-category">Category</label>
          <Select
            v-model="form.categoryId"
            input-id="dish-category"
            :options="categoryOptions"
            option-label="label"
            option-value="value"
            :invalid="!!errors.CategoryId"
            fluid
          />
          <small v-if="errors.CategoryId" class="field-error">{{ errors.CategoryId }}</small>
        </div>

        <div class="form-field">
          <label for="dish-name">Name</label>
          <InputText id="dish-name" v-model="form.name" maxlength="100" :invalid="!!errors.Name" fluid />
          <small v-if="errors.Name" class="field-error">{{ errors.Name }}</small>
        </div>

        <div class="form-field">
          <label for="dish-desc">Description</label>
          <Textarea
            id="dish-desc"
            :model-value="form.description ?? ''"
            rows="2"
            maxlength="500"
            auto-resize
            fluid
            @update:model-value="(v: string) => (form.description = v || null)"
          />
        </div>

        <div class="form-field">
          <label for="dish-price">Price</label>
          <InputNumber
            v-model="form.price"
            input-id="dish-price"
            mode="currency"
            currency="MYR"
            locale="en-MY"
            :min="0"
            :max="999999.99"
            :invalid="!!errors.Price"
            fluid
          />
          <small v-if="errors.Price" class="field-error">{{ errors.Price }}</small>
        </div>

        <div class="form-field">
          <label for="dish-image">Photo URL (optional)</label>
          <InputText
            id="dish-image"
            :model-value="form.imageUrl ?? ''"
            placeholder="https://…"
            maxlength="500"
            :invalid="!!errors.ImageUrl"
            fluid
            @update:model-value="(v) => { form.imageUrl = v || null; imagePreviewOk = true }"
          />
          <small v-if="errors.ImageUrl" class="field-error">{{ errors.ImageUrl }}</small>
          <img
            v-if="form.imageUrl && imagePreviewOk"
            :src="form.imageUrl"
            alt="Photo preview"
            class="preview"
            @error="imagePreviewOk = false"
          />
          <small v-else-if="form.imageUrl" class="muted">The photo can't be loaded from this URL.</small>
        </div>

        <div class="toggle-row">
          <ToggleSwitch v-model="form.isAvailable" input-id="dish-available" />
          <label for="dish-available">Available to order</label>
        </div>
      </form>

      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="dialogOpen = false" />
        <Button type="submit" form="dish-form" :label="editingId === null ? 'Add dish' : 'Save'" :loading="saving" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.dish {
  display: flex;
  gap: 12px;
  align-items: center;
}

.thumb {
  width: 44px;
  height: 44px;
  border-radius: 8px;
  object-fit: cover;
  flex-shrink: 0;
}

.desc {
  font-size: 0.85rem;
  max-width: 46ch;
}

.preview {
  width: 100%;
  max-height: 180px;
  object-fit: cover;
  border-radius: var(--radius-s);
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
}
</style>
