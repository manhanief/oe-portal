import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { ApiError, errorMessage } from '@/api/http'

/** Toasts and confirm dialogs shared by the admin pages, so they all behave the same. */
export function useAdminFeedback() {
  const toast = useToast()
  const confirm = useConfirm()

  return {
    saved(summary: string) {
      toast.add({ severity: 'success', summary, life: 2500 })
    },

    failed(summary: string, error: unknown) {
      toast.add({ severity: 'error', summary, detail: errorMessage(error), life: 6000 })
    },

    /** Ask before deleting; runs onConfirm only if the user agrees. */
    confirmDelete(what: string, onConfirm: () => void) {
      confirm.require({
        header: `Delete ${what}?`,
        message: 'This cannot be undone.',
        icon: 'pi pi-exclamation-triangle',
        rejectProps: { label: 'Keep', severity: 'secondary', outlined: true },
        acceptProps: { label: 'Delete', severity: 'danger' },
        accept: onConfirm,
      })
    },

    /** Field errors from a 400 response, keyed by property name (e.g. "Price"). */
    fieldErrors(error: unknown): Record<string, string> {
      if (!(error instanceof ApiError)) return {}
      return Object.fromEntries(
        Object.entries(error.fieldErrors).map(([key, messages]) => [key, messages[0] ?? '']),
      )
    },
  }
}
