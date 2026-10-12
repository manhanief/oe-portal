import { createRouter, createWebHistory } from 'vue-router'
import { RESTAURANT_NAME } from '@/config'
import { useAuthStore } from '@/stores/auth'
import MenuView from '@/views/MenuView.vue'

declare module 'vue-router' {
  interface RouteMeta {
    /** Part of the staff area (own header, login required unless public) */
    staff?: boolean
    /** Can be opened without logging in */
    public?: boolean
    /** Only admins can open it */
    adminOnly?: boolean
    title?: string
  }
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    // ---------- Customer ----------
    { path: '/', name: 'menu', component: MenuView },
    {
      path: '/checkout',
      name: 'checkout',
      component: () => import('@/views/CheckoutView.vue'),
    },
    {
      path: '/orders/:id(\\d+)',
      name: 'order',
      component: () => import('@/views/OrderStatusView.vue'),
      props: (route) => ({ id: Number(route.params.id) }),
    },

    // ---------- Staff ----------
    {
      path: '/staff/login',
      name: 'staff-login',
      component: () => import('@/views/staff/StaffLoginView.vue'),
      meta: { staff: true, public: true, title: 'Staff login' },
    },
    {
      path: '/staff',
      component: () => import('@/layouts/StaffLayout.vue'),
      meta: { staff: true },
      children: [
        { path: '', redirect: { name: 'kitchen' } },
        {
          path: 'kitchen',
          name: 'kitchen',
          component: () => import('@/views/staff/KitchenBoardView.vue'),
          meta: { title: 'Kitchen' },
        },
        {
          path: 'menu',
          name: 'admin-menu',
          component: () => import('@/views/staff/AdminMenuView.vue'),
          meta: { adminOnly: true, title: 'Menu' },
        },
        {
          path: 'categories',
          name: 'admin-categories',
          component: () => import('@/views/staff/AdminCategoriesView.vue'),
          meta: { adminOnly: true, title: 'Categories' },
        },
        {
          path: 'tables',
          name: 'admin-tables',
          component: () => import('@/views/staff/AdminTablesView.vue'),
          meta: { adminOnly: true, title: 'Tables' },
        },
        {
          path: 'accounts',
          name: 'admin-users',
          component: () => import('@/views/staff/AdminUsersView.vue'),
          meta: { adminOnly: true, title: 'Staff accounts' },
        },
        {
          path: 'sales',
          name: 'admin-sales',
          component: () => import('@/views/staff/AdminSalesView.vue'),
          meta: { adminOnly: true, title: 'Sales' },
        },
      ],
    },

    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
    },
  ],
  scrollBehavior: () => ({ top: 0 }),
})

// Route guard: staff pages need a login; admin pages need an admin.
// (This only controls the screens. The API checks permissions again on every request.)
router.beforeEach(async (to) => {
  if (!to.meta.staff) return true

  const auth = useAuthStore()
  await auth.ensureChecked()

  if (to.meta.public) {
    // Already logged in? Skip the login page.
    return auth.isLoggedIn ? { name: 'kitchen' } : true
  }

  if (!auth.isLoggedIn) {
    return { name: 'staff-login', query: { redirect: to.fullPath } }
  }

  if (to.meta.adminOnly && !auth.isAdmin) {
    return { name: 'kitchen' }
  }

  return true
})

router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · ${RESTAURANT_NAME}` : `${RESTAURANT_NAME} · Order`
})
