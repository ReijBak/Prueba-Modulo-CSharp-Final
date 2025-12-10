<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">{{ isAdmin ? 'Empleados' : 'Mi Información' }}</h1>
        <p class="text-gray-600">{{ isAdmin ? 'Gestión de empleados del sistema' : 'Información de mi perfil como empleado' }}</p>
      </div>
      <router-link
        v-if="isAdmin"
        to="/empleados/nuevo"
        class="px-4 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700"
      >
        + Nuevo Empleado
      </router-link>
    </div>

    <div v-if="isAdmin" class="bg-white rounded-lg shadow-md p-4">
      <input
        v-model="searchTerm"
        type="text"
        placeholder="Buscar por nombre, documento o email..."
        class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
      />
    </div>

    <div v-if="loading" class="text-center py-8">
      <p class="text-gray-500">Cargando {{ isAdmin ? 'empleados' : 'información' }}...</p>
    </div>

    <div v-else-if="error" class="bg-red-50 border border-red-500 text-red-600 rounded-lg p-4">
      {{ error }}
    </div>

    <div v-else class="bg-white rounded-lg shadow-md overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Documento</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Nombre</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Departamento</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Cargo</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr
              v-for="empleado in filteredEmpleados"
              :key="empleado.documento"
              :class="getRowClass(empleado.nombreEstado)"
            >
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ empleado.documento }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ empleado.nombres }} {{ empleado.apellidos }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ empleado.email || '-' }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ empleado.nombreDepartamento }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ empleado.nombreCargo }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="getStatusBadgeClass(empleado.nombreEstado)">
                  {{ empleado.nombreEstado }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <div class="flex space-x-2">
                  <router-link :to="`/empleados/${empleado.documento}`" class="px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200">Ver</router-link>
                  <router-link v-if="isAdmin" :to="`/empleados/${empleado.documento}/editar`" class="px-3 py-1 bg-yellow-100 text-yellow-700 rounded hover:bg-yellow-200">Editar</router-link>
                  <button v-if="isAdmin" @click="handleDelete(empleado.documento)" class="px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200">Eliminar</button>
                  <button @click="handleDownloadResume(empleado.documento, empleado.nombres)" class="px-3 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200">PDF</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="filteredEmpleados.length === 0" class="text-center py-8 text-gray-500">
        No se encontraron empleados.
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { empleadosService } from '@/services/empleadosService'

const authStore = useAuthStore()
const isAdmin = computed(() => authStore.isAdmin)

const empleados = ref([])
const loading = ref(true)
const error = ref('')
const searchTerm = ref('')

const filteredEmpleados = computed(() => {
  if (!searchTerm.value) return empleados.value

  const term = searchTerm.value.toLowerCase()
  return empleados.value.filter(emp =>
    emp.documento.toString().includes(term) ||
    emp.nombres.toLowerCase().includes(term) ||
    emp.apellidos.toLowerCase().includes(term) ||
    (emp.email && emp.email.toLowerCase().includes(term))
  )
})

// Helper function to get row background class based on employee status
const getRowClass = (estado) => {
  const estadoLower = estado?.toLowerCase() || ''
  if (estadoLower === 'activo') {
    return 'bg-green-50 hover:bg-green-100'
  } else if (estadoLower === 'inactivo') {
    return 'bg-red-50 hover:bg-red-100'
  } else {
    return 'bg-yellow-50 hover:bg-yellow-100'
  }
}

// Helper function to get status badge class
const getStatusBadgeClass = (estado) => {
  const estadoLower = estado?.toLowerCase() || ''
  const baseClass = 'px-2 py-1 text-xs rounded-full font-medium'
  if (estadoLower === 'activo') {
    return `${baseClass} bg-green-200 text-green-800`
  } else if (estadoLower === 'inactivo') {
    return `${baseClass} bg-red-200 text-red-800`
  } else {
    return `${baseClass} bg-yellow-200 text-yellow-800`
  }
}

onMounted(async () => {
  await loadEmpleados()
})

const loadEmpleados = async () => {
  loading.value = true
  error.value = ''

  try {
    empleados.value = await empleadosService.getAll()
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al cargar empleados'
  } finally {
    loading.value = false
  }
}

const handleDelete = async (documento) => {
  if (!confirm('¿Estás seguro de que deseas eliminar este empleado?')) return

  try {
    await empleadosService.delete(documento)
    empleados.value = empleados.value.filter(e => e.documento !== documento)
  } catch (err) {
    alert(err.response?.data?.message || 'Error al eliminar empleado')
  }
}

const handleDownloadResume = async (documento, nombre) => {
  try {
    const blob = await empleadosService.downloadResume(documento)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `HojaDeVida_${nombre}_${documento}.pdf`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
  } catch (err) {
    alert(err.response?.data?.message || 'Error al descargar PDF')
  }
}
</script>

