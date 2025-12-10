<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Importar Empleados</h1>
        <p class="text-gray-600">Carga masiva de empleados desde archivo Excel</p>
      </div>
      <router-link
        to="/empleados"
        class="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-md hover:bg-gray-300"
      >
        ← Volver
      </router-link>
    </div>

    <!-- Instructions -->
    <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
      <h3 class="font-semibold text-blue-800 mb-2">Instrucciones</h3>
      <ul class="text-sm text-blue-700 space-y-1">
        <li>• El archivo debe ser formato .xlsx (Excel)</li>
        <li>• La primera fila debe contener los encabezados</li>
        <li>• Si el empleado ya existe (mismo documento), se actualizará</li>
        <li>• Si no existe, se creará como nuevo</li>
      </ul>
    </div>

    <!-- Expected Format -->
    <div class="bg-white rounded-lg shadow-md p-6">
      <h3 class="font-semibold text-gray-900 mb-4">Formato esperado del archivo</h3>
      <div class="overflow-x-auto">
        <table class="min-w-full text-sm">
          <thead>
            <tr>
              <th>Columna</th>
              <th>Tipo</th>
              <th>Requerido</th>
              <th>Descripción</th>
            </tr>
          </thead>
          <tbody>
            <tr><td>A - Documento</td><td>Número</td><td>✓</td><td>Número de identificación</td></tr>
            <tr><td>B - Nombres</td><td>Texto</td><td>✓</td><td>Nombres del empleado</td></tr>
            <tr><td>C - Apellidos</td><td>Texto</td><td>✓</td><td>Apellidos del empleado</td></tr>
            <tr><td>D - FechaNacimiento</td><td>Fecha</td><td>✓</td><td>DD/MM/YYYY</td></tr>
            <tr><td>E - Direccion</td><td>Texto</td><td></td><td>Dirección de residencia</td></tr>
            <tr><td>F - Telefono</td><td>Texto</td><td></td><td>Número de contacto</td></tr>
            <tr><td>G - Email</td><td>Texto</td><td></td><td>Correo electrónico</td></tr>
            <tr><td>H - Salario</td><td>Número</td><td></td><td>Salario mensual</td></tr>
            <tr><td>I - FechaIngreso</td><td>Fecha</td><td>✓</td><td>DD/MM/YYYY</td></tr>
            <tr><td>J - PerfilProfesional</td><td>Texto</td><td></td><td>Descripción del perfil</td></tr>
            <tr><td>K - Estado</td><td>Texto</td><td>✓</td><td>Ej: Activo, Inactivo</td></tr>
            <tr><td>L - NivelEducativo</td><td>Texto</td><td>✓</td><td>Ej: Profesional, Bachiller</td></tr>
            <tr><td>M - Departamento</td><td>Texto</td><td>✓</td><td>Ej: Tecnología, Finanzas</td></tr>
            <tr><td>N - Cargo</td><td>Texto</td><td>✓</td><td>Ej: Desarrollador, Analista</td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Upload Section -->
    <div class="bg-white rounded-lg shadow-md p-6">
      <h3 class="font-semibold text-gray-900 mb-4">Seleccionar archivo</h3>
      
      <!-- File Input -->
      <div
        @dragover.prevent="dragOver = true"
        @dragleave="dragOver = false"
        @drop.prevent="handleDrop"
        :class="[
          'border-2 border-dashed rounded-lg p-8 text-center transition',
          dragOver ? 'border-blue-500 bg-blue-50' : 'border-gray-300',
          file ? 'bg-green-50 border-green-500' : ''
        ]"
      >
        <input
          type="file"
          ref="fileInput"
          @change="handleFileChange"
          accept=".xlsx"
          class="hidden"
        />
        
        <div v-if="!file">
          <p class="text-gray-500 mb-2">Arrastra tu archivo aquí o</p>
          <button
            @click="$refs.fileInput.click()"
            class="px-4 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700"
          >
            Seleccionar archivo
          </button>
        </div>
        
        <div v-else class="flex items-center justify-center space-x-4">
          <span class="text-green-700 font-medium">📄 {{ file.name }}</span>
          <button
            @click="clearFile"
            class="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200"
          >
            ✕ Quitar
          </button>
        </div>
      </div>

      <!-- Upload Button -->
      <div class="mt-4 flex justify-end">
        <button
          @click="handleUpload"
          :disabled="!file || uploading"
          class="px-6 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {{ uploading ? 'Importando...' : 'Importar Empleados' }}
        </button>
      </div>
    </div>

    <!-- Results -->
    <div v-if="result" :class="[
      'rounded-lg p-6',
      result.success ? 'bg-green-50 border border-green-500' : 'bg-red-50 border border-red-500'
    ]">
      <h3 :class="['font-semibold mb-4', result.success ? 'text-green-800' : 'text-red-800']">
        {{ result.success ? '✓ Importación completada' : '✕ Error en la importación' }}
      </h3>
      
      <div v-if="result.success" class="grid grid-cols-4 gap-4 mb-4">
        <div class="bg-white rounded p-3 text-center">
          <p class="text-2xl font-bold text-gray-900">{{ result.totalRows }}</p>
          <p class="text-sm text-gray-500">Total filas</p>
        </div>
        <div class="bg-white rounded p-3 text-center">
          <p class="text-2xl font-bold text-green-600">{{ result.insertedCount }}</p>
          <p class="text-sm text-gray-500">Insertados</p>
        </div>
        <div class="bg-white rounded p-3 text-center">
          <p class="text-2xl font-bold text-blue-600">{{ result.updatedCount }}</p>
          <p class="text-sm text-gray-500">Actualizados</p>
        </div>
        <div class="bg-white rounded p-3 text-center">
          <p class="text-2xl font-bold text-red-600">{{ result.errorCount }}</p>
          <p class="text-sm text-gray-500">Errores</p>
        </div>
      </div>

      <p class="text-gray-700">{{ result.message }}</p>

      <!-- Errors list -->
      <div v-if="result.errors && result.errors.length > 0" class="mt-4">
        <p class="font-medium text-red-700 mb-2">Detalle de errores:</p>
        <ul class="text-sm text-red-600 space-y-1 max-h-48 overflow-y-auto">
          <li v-for="(error, index) in result.errors" :key="index">• {{ error }}</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { empleadosService } from '../services/empleadosService';

const file = ref(null);
const fileInput = ref(null);
const dragOver = ref(false);
const uploading = ref(false);
const result = ref(null);

const handleFileChange = (event) => {
  const selectedFile = event.target.files[0];
  if (selectedFile && selectedFile.name.endsWith('.xlsx')) {
    file.value = selectedFile;
    result.value = null;
  } else {
    alert('Por favor, selecciona un archivo .xlsx');
  }
};

const handleDrop = (event) => {
  dragOver.value = false;
  const droppedFile = event.dataTransfer.files[0];
  if (droppedFile && droppedFile.name.endsWith('.xlsx')) {
    file.value = droppedFile;
    result.value = null;
  } else {
    alert('Por favor, arrastra un archivo .xlsx');
  }
};

const clearFile = () => {
  file.value = null;
  result.value = null;
  if (fileInput.value) {
    fileInput.value.value = '';
  }
};

const handleUpload = async () => {
  if (!file.value) return;

  uploading.value = true;
  result.value = null;

  try {
    result.value = await empleadosService.importExcel(file.value);
  } catch (err) {
    result.value = {
      success: false,
      message: err.response?.data?.message || 'Error al importar archivo',
      errors: err.response?.data?.errors || [],
    };
  } finally {
    uploading.value = false;
  }
};
</script>

