/** 12.9 -> "RM 12.90" */
export function formatMoney(value: number): string {
  return `RM ${value.toFixed(2)}`
}

/** Round to 2 decimals the same way the server does (half away from zero). */
export function round2(value: number): number {
  return (Math.sign(value) * Math.round(Math.abs(value) * 100 + Number.EPSILON)) / 100
}
