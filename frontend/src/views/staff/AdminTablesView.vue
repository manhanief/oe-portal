<script setup lang="ts">
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { onMounted, reactive, ref } from 'vue'
import { adminApi } from '@/api/staff'
import { useAdminFeedback } from '@/composables/useAdminFeedback'
import type { RestaurantTable } from '@/types/api'
import type { SaveTableRequest } from '@/types/staff'

const feedback = useAdminFeedback()

const tables = ref<RestaurantTable[]>([])
const loading = ref(true)
const loadError = ref<string | null>(null)

function sortList() {
  tables.value.sort((a, b) => a.tableNo.localeCompare(b.tableNo, undefined, { numeric: true }))
}

async function load() {
  loading.value = true
  loadError.value = null
  try {
    tables.value = await adminApi.tables.list()
    sortList()
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'Tables could not be loaded.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

const dialogOpen = ref(false)
const editingId = ref<number | null>(null)
const saving = ref(false)
const errors = ref<Record<string, string>>({})
const form = reactive<SaveTableRequest>({ tableNo: '', seats: 4 })

function openNew() {
  editingId.value = null
  Object.assign(form, { tableNo: '', seats: 4 })
  errors.value = {}
  dialogOpen.value = true
}

function openEdit(table: RestaurantTable) {
  editingId.value = table.id
  Object.assign(form, { tableNo: table.tableNo, seats: table.seats })
  errors.value = {}
  dialogOpen.value = true
}

async function save() {
  saving.value = true
  errors.value = {}
  try {
    if (editingId.value === null) {
      const created = await adminApi.tables.create({ ...form })
      tables.value.push(created)
      feedback.saved(`Table ${created.tableNo} added`)
    } else {
      const updated = await adminApi.tables.update(editingId.value, { ...form })
      tables.value = tables.value.map((t) => (t.id === updated.id ? updated : t))
      feedback.saved(`Table ${updated.tableNo} saved`)
    }
    sortList()
    dialogOpen.value = false
  } catch (e) {
    errors.value = feedback.fieldErrors(e)
    if (Object.keys(errors.value).length === 0) feedback.failed('Could not save the table', e)
  } finally {
    saving.value = false
  }
}

function remove(table: RestaurantTable) {
  feedback.confirmDelete(`table ${table.tableNo}`, async () => {
    try {
      await adminApi.tables.remove(table.id)
      tables.value = tables.value.filter((t) => t.id !== table.id)
      feedback.saved(`Table ${table.tableNo} deleted`)
    } catch (e) {
      feedback.failed(`Could not delete table ${table.tableNo}`, e)
    }
  })
}
</script>

<template>
  <div class="admin-page narrow">
    <div class="admin-head">
      <h1>Tables</h1>
      <Button label="Add table" icon="pi pi-plus" @click="openNew" />
    </div>

    <p class="muted">Customers choose their table number when ordering to dine in.</p>

    <Message v-if="loadError" severity="error">{{ loadError }}</Message>

    <DataTable :value="tables" :loading="loading" data-key="id" size="small" class="panel-table">
      <template #empty><p class="muted">No tables yet.</p></template>

      <Column field="tableNo" header="Table">
        <template #body="{ data }">
          <strong>{{ data.tableNo }}</strong>
        </template>
      </Column>
      <Column field="seats" header="Seats" />
      <Column header="">
        <template #body="{ data }">
          <div class="row-actions">
            <Button icon="pi pi-pencil" text rounded :aria-label="`Edit table ${data.tableNo}`" @click="openEdit(data)" />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              :aria-label="`Delete table ${data.tableNo}`"
              @click="remove(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="dialogOpen"
      modal
      :header="editingId === null ? 'Add table' : 'Edit table'"
      :style="{ width: 'min(400px, 94vw)' }"
    >
      <form id="table-form" class="form-grid" novalidate @submit.prevent="save">
        <div class="form-field">
          <label for="table-no">Table number</label>
          <InputText
            id="table-no"
            v-model="form.tableNo"
            placeholder="e.g. T09"
            maxlength="10"
            :invalid="!!errors.TableNo"
            fluid
          />
          <small v-if="errors.TableNo" class="field-error">{{ errors.TableNo }}</small>
        </div>

        <div class="form-field">
          <label for="table-seats">Seats</label>
          <InputNumber v-model="form.seats" input-id="table-seats" :min="1" :max="99" show-buttons fluid />
          <small v-if="errors.Seats" class="field-error">{{ errors.Seats }}</small>
        </div>
      </form>

      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="dialogOpen = false" />
        <Button type="submit" form="table-form" :label="editingId === null ? 'Add table' : 'Save'" :loading="saving" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.narrow {
  max-width: 720px;
}
</style>
