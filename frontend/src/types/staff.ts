// Shapes used by the staff and admin pages (match backend/Resto.Api/Dtos)

export type UserRole = 'Admin' | 'Staff'

export interface CurrentUser {
  id: number
  username: string
  role: UserRole
}

export interface Category {
  id: number
  name: string
  sortOrder: number
  isActive: boolean
  itemCount: number
}

export interface SaveCategoryRequest {
  name: string
  sortOrder: number
  isActive: boolean
}

export interface AdminMenuItem {
  id: number
  categoryId: number
  categoryName: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  isAvailable: boolean
}

export interface SaveMenuItemRequest {
  categoryId: number
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  isAvailable: boolean
}

export interface SaveTableRequest {
  tableNo: string
  seats: number
}

export interface StaffUser {
  id: number
  username: string
  role: UserRole
}

export interface CreateUserRequest {
  username: string
  password: string
  role: UserRole
}

export interface TopItem {
  name: string
  quantity: number
  revenue: number
}

export interface DailySales {
  date: string
  orderCount: number
  cancelledCount: number
  revenue: number
  averageOrder: number
  topItems: TopItem[]
}
