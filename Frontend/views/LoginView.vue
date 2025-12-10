<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100 py-12 px-4">
    <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
      <div class="text-center mb-8">
        <h2 class="text-3xl font-bold text-gray-900">TalentoPlus</h2>
        <p class="mt-2 text-gray-600">Inicia sesión en tu cuenta</p>
      </div>

      <!-- Login Type Tabs -->
      <div class="flex mb-6 border-b border-gray-200">
        <button
          @click="loginType = 'admin'"
          :class="['flex-1 py-2 text-center font-medium transition', 
            loginType === 'admin' 
              ? 'text-blue-600 border-b-2 border-blue-600' 
              : 'text-gray-500 hover:text-gray-700']"
        >
          Administrador
        </button>
        <button
          @click="loginType = 'employee'"
          :class="['flex-1 py-2 text-center font-medium transition',
            loginType === 'employee' 
              ? 'text-blue-600 border-b-2 border-blue-600' 
              : 'text-gray-500 hover:text-gray-700']"
        >
          Empleado
        </button>
      </div>

      <!-- Error message -->
      <div v-if="error" class="mb-4 p-4 bg-red-50 border border-red-500 text-red-600 rounded-md text-sm">
        {{ error }}
      </div>

      <!-- Admin Login Form -->
      <form v-if="loginType === 'admin'" @submit.prevent="handleAdminLogin" class="space-y-6">
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
            Correo Electrónico
          </label>
          <input
            id="email"
            v-model="adminForm.email"
            type="email"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="admin@example.com"
          />
        </div>

        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
            Contraseña
          </label>
          <input
            id="password"
            v-model="adminForm.password"
            type="password"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="••••••••"
          />
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2 px-4 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {{ loading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
        </button>
      </form>

      <!-- Employee Login Form -->
      <form v-else @submit.prevent="handleEmployeeLogin" class="space-y-6">
        <div>
          <label for="documento" class="block text-sm font-medium text-gray-700 mb-2">
            Número de Documento
          </label>
          <input
            id="documento"
            v-model="employeeForm.documento"
            type="number"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="12345678"
          />
        </div>

        <div>
          <label for="emp-password" class="block text-sm font-medium text-gray-700 mb-2">
            Contraseña
          </label>
          <input
            id="emp-password"
            v-model="employeeForm.password"
            type="password"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="••••••••"
          />
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full py-2 px-4 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {{ loading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
        </button>
      </form>

      <!-- Register Link (Admin only) -->
      <div v-if="loginType === 'admin'" class="mt-6 text-center">
        <p class="text-sm text-gray-600">
          ¿No tienes cuenta?
          <router-link to="/register" class="text-blue-600 hover:text-blue-700 font-medium">
            Registrarse
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { authService } from '../services/authService';

const router = useRouter();
const authStore = useAuthStore();

const loginType = ref('admin');
const loading = ref(false);
const error = ref('');

const adminForm = reactive({
  email: '',
  password: '',
});

const employeeForm = reactive({
  documento: '',
  password: '',
});

const handleAdminLogin = async () => {
  loading.value = true;
  error.value = '';
  
  try {
    const response = await authService.adminLogin(adminForm.email, adminForm.password);
    if (response.success) {
      authStore.setAuth(response);
      router.push('/dashboard');
    } else {
      error.value = response.message || 'Error al iniciar sesión';
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor';
  } finally {
    loading.value = false;
  }
};

const handleEmployeeLogin = async () => {
  loading.value = true;
  error.value = '';
  
  try {
    const response = await authService.employeeLogin(
      parseInt(employeeForm.documento),
      employeeForm.password
    );
    if (response.success) {
      authStore.setAuth(response);
      router.push('/empleados');
    } else {
      error.value = response.message || 'Error al iniciar sesión';
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor';
  } finally {
    loading.value = false;
  }
};
</script>
