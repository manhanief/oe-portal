import { http } from './http'
import type { CreateOrderRequest, MenuCategory, Order, RestaurantTable } from '@/types/api'

// One function per API endpoint the customer site uses.

export const restaurantApi = {
  getMenu: () => http.get<MenuCategory[]>('/menu'),

  getTables: () => http.get<RestaurantTable[]>('/tables'),

  createOrder: (request: CreateOrderRequest) => http.post<Order>('/orders', request),

  getOrder: (id: number) => http.get<Order>(`/orders/${id}`),
}
