<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100 py-12 px-4">
    <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
      <div class="text-center mb-8">
        <h2 class="text-3xl font-bold text-gray-900">TalentoPlus</h2>
        <p class="mt-2 text-gray-600">Registro de Administrador</p>
      </div>

      <!-- Error message -->
      <div v-if="error" class="mb-4 p-4 bg-red-50 border border-red-500 text-red-600 rounded-md text-sm">
        {{ error }}
      </div>

      <!-- Success message -->
      <div v-if="success" class="mb-4 p-4 bg-green-50 border border-green-500 text-green-600 rounded-md text-sm">
        {{ success }}
      </div>

      <form @submit.prevent="handleRegister" class="space-y-6">
        <div>
          <label for="fullName" class="block text-sm font-medium text-gray-700 mb-2">
            Nombre Completo
          </label>
          <input
            id="fullName"
            v-model="form.fullName"
            type="text"
            required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="Juan Pérez"
          />
        </div>

        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
            Correo Electrónico
          </label>
          <input
            id="email"
            v-model="form.email"
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
            v-model="form.password"
            type="password"
            required
            minlength="6"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="••••••••"
          />
          <p class="mt-1 text-xs text-gray-500">Mínimo 6 caracteres, incluir mayúscula y número</p>
        </div>

        <div>
          <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
            Confirmar Contraseña
          </label>
          <input
            id="confirmPassword"
            v-model="form.confirmPassword"
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
          {{ loading ? 'Registrando...' : 'Registrarse' }}
        </button>
      </form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-600">
          ¿Ya tienes cuenta?
          <router-link to="/login" class="text-blue-600 hover:text-blue-700 font-medium">
            Iniciar Sesión
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

const loading = ref(false);
const error = ref('');
const success = ref('');

const form = reactive({
  fullName: '',
  email: '',
  password: '',
  confirmPassword: '',
});

const handleRegister = async () => {
  error.value = '';
  success.value = '';

  if (form.password !== form.confirmPassword) {
    error.value = 'Las contraseñas no coinciden';
    return;
  }

  loading.value = true;

  try {
    const response = await authService.adminRegister(form.email, form.password, form.fullName);
    if (response.success) {
      authStore.setAuth(response);
      router.push('/dashboard');
    } else {
      error.value = response.message || 'Error al registrar';
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor';
  } finally {
    loading.value = false;
  }
};
</script>

