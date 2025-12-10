import { createRouter, createWebHistory } from 'vue-router'
import { authService } from '@/services/authService'

const routes = [
  {
    path: '/',
    redirect: '/dashboard',
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { guest: true },
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/RegisterView.vue'),
    meta: { guest: true },
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/DashboardView.vue'),
    meta: { requiresAuth: true, adminOnly: true },
  },
  {
    path: '/empleados',
    name: 'Empleados',
    component: () => import('@/views/EmpleadosView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/empleados/nuevo',
    name: 'NuevoEmpleado',
    component: () => import('@/views/EmpleadoFormView.vue'),
    meta: { requiresAuth: true, adminOnly: true },
  },
  {
    path: '/empleados/:documento/editar',
    name: 'EditarEmpleado',
    component: () => import('@/views/EmpleadoFormView.vue'),
    meta: { requiresAuth: true, adminOnly: true },
  },
  {
    path: '/empleados/:documento',
    name: 'VerEmpleado',
    component: () => import('@/views/EmpleadoDetailView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/importar',
    name: 'Importar',
    component: () => import('@/views/ImportView.vue'),
    meta: { requiresAuth: true, adminOnly: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const isAuthenticated = authService.isAuthenticated()
  const isAdmin = authService.isAdmin()

  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (to.meta.guest && isAuthenticated) {
    next('/dashboard')
  } else if (to.meta.adminOnly && !isAdmin) {
    next('/empleados')
  } else {
    next()
  }
})

export default router

