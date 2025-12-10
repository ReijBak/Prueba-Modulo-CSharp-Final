<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Detalle del Empleado</h1>
      </div>
      <div class="flex space-x-4">
        <button
          @click="handleDownloadResume"
          class="px-4 py-2 bg-green-600 text-white font-medium rounded-md hover:bg-green-700"
        >
          📄 Descargar PDF
        </button>
        <router-link
          to="/empleados"
          class="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-md hover:bg-gray-300"
        >
          ← Volver
        </router-link>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-8">
      <p class="text-gray-500">Cargando...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="bg-red-50 border border-red-500 text-red-600 rounded-lg p-4">
      {{ error }}
    </div>

    <!-- Employee Detail -->
    <div v-else-if="empleado" class="bg-white rounded-lg shadow-md">
      <!-- Header with status -->
      <div class="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <div>
          <h2 class="text-xl font-bold text-gray-900">
            {{ empleado.nombres }} {{ empleado.apellidos }}
          </h2>
          <p class="text-gray-500">Documento: {{ empleado.documento }}</p>
        </div>
        <span
          :class="[
            'px-3 py-1 text-sm rounded-full font-medium',
            empleado.nombreEstado === 'Activo'
              ? 'bg-green-100 text-green-800'
              : 'bg-gray-100 text-gray-800'
          ]"
        >
          {{ empleado.nombreEstado }}
        </span>
      </div>

      <!-- Details -->
      <div class="p-6">
        <div class="grid grid-cols-1 gap-6" style="grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));">
          <!-- Personal Info Section -->
          <div class="space-y-4">
            <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-200 pb-2">
              Información Personal
            </h3>
            <div class="space-y-3">
              <div>
                <p class="text-sm text-gray-500">Fecha de Nacimiento</p>
                <p class="font-medium">{{ formatDate(empleado.fechaNacimiento) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Email</p>
                <p class="font-medium">{{ empleado.email || '-' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Teléfono</p>
                <p class="font-medium">{{ empleado.telefono || '-' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Dirección</p>
                <p class="font-medium">{{ empleado.direccion || '-' }}</p>
              </div>
            </div>
          </div>

          <!-- Work Info Section -->
          <div class="space-y-4">
            <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-200 pb-2">
              Información Laboral
            </h3>
            <div class="space-y-3">
              <div>
                <p class="text-sm text-gray-500">Departamento</p>
                <p class="font-medium">{{ empleado.nombreDepartamento }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Cargo</p>
                <p class="font-medium">{{ empleado.nombreCargo }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Fecha de Ingreso</p>
                <p class="font-medium">{{ formatDate(empleado.fechaIngreso) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-500">Salario</p>
                <p class="font-medium">
                  {{ empleado.salario ? formatCurrency(empleado.salario) : '-' }}
                </p>
              </div>
            </div>
          </div>

          <!-- Education Section -->
          <div class="space-y-4">
            <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-200 pb-2">
              Formación
            </h3>
            <div class="space-y-3">
              <div>
                <p class="text-sm text-gray-500">Nivel Educativo</p>
                <p class="font-medium">{{ empleado.nombreNivelEducativo }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Professional Profile -->
        <div v-if="empleado.perfilProfesional" class="mt-6">
          <h3 class="text-lg font-semibold text-gray-900 border-b border-gray-200 pb-2 mb-4">
            Perfil Profesional
          </h3>
          <p class="text-gray-700 whitespace-pre-wrap">{{ empleado.perfilProfesional }}</p>
        </div>
      </div>

      <!-- Actions -->
      <div v-if="isAdmin" class="px-6 py-4 border-t border-gray-200 flex justify-end space-x-4">
        <router-link
          :to="`/empleados/${empleado.documento}/editar`"
          class="px-4 py-2 bg-yellow-500 text-white font-medium rounded-md hover:bg-yellow-600"
        >
          Editar
        </router-link>
        <button
          @click="handleDelete"
          class="px-4 py-2 bg-red-500 text-white font-medium rounded-md hover:bg-red-600"
        >
          Eliminar
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { empleadosService } from '../services/empleadosService';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const isAdmin = computed(() => authStore.isAdmin);
const empleado = ref(null);
const loading = ref(true);
const error = ref('');

onMounted(async () => {
  try {
    empleado.value = await empleadosService.getById(route.params.documento);
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al cargar empleado';
  } finally {
    loading.value = false;
  }
});

const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
};

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
  }).format(amount);
};

const handleDelete = async () => {
  if (!confirm('¿Estás seguro de que deseas eliminar este empleado?')) return;
  
  try {
    await empleadosService.delete(empleado.value.documento);
    router.push('/empleados');
  } catch (err) {
    alert(err.response?.data?.message || 'Error al eliminar empleado');
  }
};

const handleDownloadResume = async () => {
  try {
    const blob = await empleadosService.downloadResume(empleado.value.documento);
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `HojaDeVida_${empleado.value.nombres}_${empleado.value.documento}.pdf`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  } catch (err) {
    alert(err.response?.data?.message || 'Error al descargar PDF');
  }
};
</script>

