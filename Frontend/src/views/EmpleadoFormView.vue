<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">{{ isEditing ? 'Editar Empleado' : 'Nuevo Empleado' }}</h1>
        <p class="text-gray-600">{{ isEditing ? 'Actualiza la información del empleado' : 'Registra un nuevo empleado' }}</p>
      </div>
      <router-link to="/empleados" class="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-md hover:bg-gray-300">← Volver</router-link>
    </div>

    <div v-if="loading" class="text-center py-8">
      <p class="text-gray-500">Cargando...</p>
    </div>

    <div v-if="error" class="bg-red-50 border border-red-500 text-red-600 rounded-lg p-4">{{ error }}</div>

    <form v-if="!loading" @submit.prevent="handleSubmit" class="bg-white rounded-lg shadow-md p-6">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label for="documento" class="block text-sm font-medium text-gray-700 mb-2">Número de Documento *</label>
          <input id="documento" v-model.number="form.documento" type="number" required :disabled="isEditing"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100" />
        </div>

        <div>
          <label for="nombres" class="block text-sm font-medium text-gray-700 mb-2">Nombres *</label>
          <input id="nombres" v-model="form.nombres" type="text" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="apellidos" class="block text-sm font-medium text-gray-700 mb-2">Apellidos *</label>
          <input id="apellidos" v-model="form.apellidos" type="text" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="fechaNacimiento" class="block text-sm font-medium text-gray-700 mb-2">Fecha de Nacimiento *</label>
          <input id="fechaNacimiento" v-model="form.fechaNacimiento" type="date" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">Email</label>
          <input id="email" v-model="form.email" type="email"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="telefono" class="block text-sm font-medium text-gray-700 mb-2">Teléfono</label>
          <input id="telefono" v-model="form.telefono" type="tel"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div class="md:col-span-2">
          <label for="direccion" class="block text-sm font-medium text-gray-700 mb-2">Dirección</label>
          <input id="direccion" v-model="form.direccion" type="text"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="salario" class="block text-sm font-medium text-gray-700 mb-2">Salario</label>
          <input id="salario" v-model.number="form.salario" type="number" step="0.01"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="fechaIngreso" class="block text-sm font-medium text-gray-700 mb-2">Fecha de Ingreso *</label>
          <input id="fechaIngreso" v-model="form.fechaIngreso" type="date" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500" />
        </div>

        <div>
          <label for="estadoId" class="block text-sm font-medium text-gray-700 mb-2">Estado *</label>
          <select id="estadoId" v-model.number="form.estadoId" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500">
            <option value="">Seleccionar...</option>
            <option v-for="estado in catalogs.estados" :key="estado.id" :value="estado.id">{{ estado.nombre }}</option>
          </select>
        </div>

        <div>
          <label for="departamentoId" class="block text-sm font-medium text-gray-700 mb-2">Departamento *</label>
          <select id="departamentoId" v-model.number="form.departamentoId" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500">
            <option value="">Seleccionar...</option>
            <option v-for="dep in catalogs.departamentos" :key="dep.id" :value="dep.id">{{ dep.nombre }}</option>
          </select>
        </div>

        <div>
          <label for="cargoId" class="block text-sm font-medium text-gray-700 mb-2">Cargo *</label>
          <select id="cargoId" v-model.number="form.cargoId" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500">
            <option value="">Seleccionar...</option>
            <option v-for="cargo in catalogs.cargos" :key="cargo.id" :value="cargo.id">{{ cargo.nombre }}</option>
          </select>
        </div>

        <div>
          <label for="nivelEducativoId" class="block text-sm font-medium text-gray-700 mb-2">Nivel Educativo *</label>
          <select id="nivelEducativoId" v-model.number="form.nivelEducativoId" required
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500">
            <option value="">Seleccionar...</option>
            <option v-for="nivel in catalogs.nivelesEducativos" :key="nivel.id" :value="nivel.id">{{ nivel.nombre }}</option>
          </select>
        </div>

        <div v-if="!isEditing">
          <label for="password" class="block text-sm font-medium text-gray-700 mb-2">Contraseña</label>
          <input id="password" v-model="form.password" type="password"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"
            placeholder="Para acceso al sistema" />
        </div>

        <div class="md:col-span-2">
          <label for="perfilProfesional" class="block text-sm font-medium text-gray-700 mb-2">Perfil Profesional</label>
          <textarea id="perfilProfesional" v-model="form.perfilProfesional" rows="4"
            class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500"></textarea>
        </div>
      </div>

      <div class="mt-6 flex justify-end space-x-4">
        <router-link to="/empleados" class="px-6 py-2 bg-gray-200 text-gray-700 font-medium rounded-md hover:bg-gray-300">Cancelar</router-link>
        <button type="submit" :disabled="submitting"
          class="px-6 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 disabled:opacity-50">
          {{ submitting ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Crear') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { empleadosService } from '@/services/empleadosService'
import { catalogsService } from '@/services/catalogsService'

const route = useRoute()
const router = useRouter()

const isEditing = computed(() => !!route.params.documento)
const loading = ref(true)
const submitting = ref(false)
const error = ref('')

const catalogs = reactive({
  estados: [],
  departamentos: [],
  cargos: [],
  nivelesEducativos: [],
})

const form = reactive({
  documento: '',
  nombres: '',
  apellidos: '',
  fechaNacimiento: '',
  direccion: '',
  telefono: '',
  email: '',
  salario: null,
  fechaIngreso: '',
  perfilProfesional: '',
  password: '',
  estadoId: '',
  nivelEducativoId: '',
  departamentoId: '',
  cargoId: '',
})

onMounted(async () => {
  try {
    const catalogData = await catalogsService.getAllCatalogs()
    Object.assign(catalogs, catalogData)

    if (isEditing.value) {
      const empleado = await empleadosService.getById(route.params.documento)
      Object.assign(form, {
        documento: empleado.documento,
        nombres: empleado.nombres,
        apellidos: empleado.apellidos,
        fechaNacimiento: empleado.fechaNacimiento?.split('T')[0] || '',
        direccion: empleado.direccion || '',
        telefono: empleado.telefono || '',
        email: empleado.email || '',
        salario: empleado.salario,
        fechaIngreso: empleado.fechaIngreso?.split('T')[0] || '',
        perfilProfesional: empleado.perfilProfesional || '',
        estadoId: empleado.estadoId,
        nivelEducativoId: empleado.nivelEducativoId,
        departamentoId: empleado.departamentoId,
        cargoId: empleado.cargoId,
      })
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al cargar datos'
  } finally {
    loading.value = false
  }
})

const handleSubmit = async () => {
  submitting.value = true
  error.value = ''

  try {
    const data = {
      ...form,
      fechaNacimiento: new Date(form.fechaNacimiento).toISOString(),
      fechaIngreso: new Date(form.fechaIngreso).toISOString(),
    }

    if (isEditing.value) {
      await empleadosService.update(form.documento, data)
    } else {
      await empleadosService.create(data)
    }

    router.push('/empleados')
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al guardar empleado'
  } finally {
    submitting.value = false
  }
}
</script>

