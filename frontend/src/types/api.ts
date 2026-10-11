// Shapes returned by the .NET API (must match the C# DTOs in backend/Resto.Api/Dtos)

export type OrderType = 'DineIn' | 'Takeaway'

export type OrderStatus = 'Pending' | 'Preparing' | 'Ready' | 'Completed' | 'Cancelled'

export interface MenuItem {
  id: number
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  isAvailable: boolean
}

export interface MenuCategory {
  id: number
  name: string
  items: MenuItem[]
}

export interface RestaurantTable {
  id: number
  tableNo: string
  seats: number
}

export interface CreateOrderItemRequest {
  itemId: number
  quantity: number
  notes: string | null
}

export interface CreateOrderRequest {
  orderType: OrderType
  tableId: number | null
  customerName: string | null
  items: CreateOrderItemRequest[]
}

export interface OrderItem {
  itemId: number
  name: string
  quantity: number
  unitPrice: number
  lineTotal: number
  notes: string | null
}

export interface Order {
  id: number
  orderNo: string
  orderType: OrderType
  tableNo: string | null
  customerName: string | null
  status: OrderStatus
  subtotal: number
  tax: number
  total: number
  createdAt: string
  items: OrderItem[]
}
