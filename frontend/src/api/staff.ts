import { http } from './http'
import type { Order, OrderStatus, RestaurantTable } from '@/types/api'
import type {
  AdminMenuItem,
  Category,
  CreateUserRequest,
  CurrentUser,
  DailySales,
  SaveCategoryRequest,
  SaveMenuItemRequest,
  SaveTableRequest,
  StaffUser,
} from '@/types/staff'

// One function per API endpoint used by the staff and admin pages.

export const authApi = {
  login: (username: string, password: string) => http.post<CurrentUser>('/auth/login', { username, password }),
  logout: () => http.post<void>('/auth/logout'),
  me: () => http.get<CurrentUser>('/auth/me'),
}

export const kitchenApi = {
  activeOrders: () => http.get<Order[]>('/orders'),
  setStatus: (id: number, status: OrderStatus) => http.patch<Order>(`/orders/${id}/status`, { status }),
}

export const adminApi = {
  categories: {
    list: () => http.get<Category[]>('/admin/categories'),
    create: (body: SaveCategoryRequest) => http.post<Category>('/admin/categories', body),
    update: (id: number, body: SaveCategoryRequest) => http.put<Category>(`/admin/categories/${id}`, body),
    remove: (id: number) => http.delete(`/admin/categories/${id}`),
  },
  menuItems: {
    list: () => http.get<AdminMenuItem[]>('/admin/menu-items'),
    create: (body: SaveMenuItemRequest) => http.post<AdminMenuItem>('/admin/menu-items', body),
    update: (id: number, body: SaveMenuItemRequest) => http.put<AdminMenuItem>(`/admin/menu-items/${id}`, body),
    remove: (id: number) => http.delete(`/admin/menu-items/${id}`),
  },
  tables: {
    list: () => http.get<RestaurantTable[]>('/tables'),
    create: (body: SaveTableRequest) => http.post<RestaurantTable>('/admin/tables', body),
    update: (id: number, body: SaveTableRequest) => http.put<RestaurantTable>(`/admin/tables/${id}`, body),
    remove: (id: number) => http.delete(`/admin/tables/${id}`),
  },
  users: {
    list: () => http.get<StaffUser[]>('/admin/users'),
    create: (body: CreateUserRequest) => http.post<StaffUser>('/admin/users', body),
    resetPassword: (id: number, password: string) => http.put<void>(`/admin/users/${id}/password`, { password }),
    remove: (id: number) => http.delete(`/admin/users/${id}`),
  },
  reports: {
    daily: (date?: string) => http.get<DailySales>(`/admin/reports/daily${date ? `?date=${date}` : ''}`),
  },
}
