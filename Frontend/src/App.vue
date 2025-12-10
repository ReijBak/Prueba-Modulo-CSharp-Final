<template>
  <div class="min-h-screen bg-gray-100">
    <nav v-if="isAuthenticated" class="bg-blue-600 text-white shadow-lg">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <div class="flex items-center">
            <h1 class="text-xl font-bold">TalentoPlus</h1>
            <div class="ml-10 flex space-x-4">
              <router-link
                v-if="isAdmin"
                to="/dashboard"
                class="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-700"
              >
                Dashboard IA
              </router-link>
              <router-link
                to="/empleados"
                class="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-700"
              >
                Empleados
              </router-link>
              <router-link
                v-if="isAdmin"
                to="/importar"
                class="px-3 py-2 rounded-md text-sm font-medium hover:bg-blue-700"
              >
                Importar Excel
              </router-link>
            </div>
          </div>
          <div class="flex items-center space-x-4">
            <span class="text-sm">{{ user?.fullName }}</span>
            <span class="text-xs bg-blue-800 px-2 py-1 rounded">{{ user?.userType }}</span>
            <button
              @click="logout"
              class="px-3 py-2 rounded-md text-sm font-medium bg-red-500 hover:bg-red-600"
            >
              Cerrar Sesión
            </button>
          </div>
        </div>
      </div>
    </nav>

    <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isAuthenticated = computed(() => authStore.isAuthenticated)
const isAdmin = computed(() => authStore.isAdmin)
const user = computed(() => authStore.user)

const logout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

