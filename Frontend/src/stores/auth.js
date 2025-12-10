import { defineStore } from 'pinia'
import { authService } from '@/services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: authService.getCurrentUser(),
    isAuthenticated: authService.isAuthenticated(),
  }),

  getters: {
    isAdmin: (state) => state.user?.userType === 'Admin',
    isEmployee: (state) => state.user?.userType === 'Employee',
    fullName: (state) => state.user?.fullName || '',
    documento: (state) => state.user?.documento || null,
  },

  actions: {
    setAuth(authResponse) {
      authService.saveAuth(authResponse)
      this.user = authService.getCurrentUser()
      this.isAuthenticated = true
    },

    logout() {
      authService.logout()
      this.user = null
      this.isAuthenticated = false
    },

    checkAuth() {
      this.isAuthenticated = authService.isAuthenticated()
      this.user = authService.getCurrentUser()
    },
  },
})

