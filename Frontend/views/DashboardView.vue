<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Dashboard IA</h1>
        <p class="text-gray-600">Consulta información usando lenguaje natural</p>
      </div>
    </div>

    <!-- Query Section -->
    <div class="bg-white rounded-lg shadow-md p-6">
      <h2 class="text-lg font-semibold text-gray-900 mb-4">Pregunta a la Base de Datos</h2>
      
      <form @submit.prevent="handleQuery" class="space-y-4">
        <div>
          <label for="question" class="block text-sm font-medium text-gray-700 mb-2">
            Tu pregunta
          </label>
          <div class="flex gap-4">
            <input
              id="question"
              v-model="question"
              type="text"
              required
              class="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Ej: ¿Cuántos empleados hay en el departamento de Tecnología?"
            />
            <button
              type="submit"
              :disabled="loading || !question.trim()"
              class="px-6 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ loading ? 'Consultando...' : 'Preguntar' }}
            </button>
          </div>
        </div>
      </form>

      <!-- Example Questions -->
      <div class="mt-4">
        <p class="text-sm text-gray-500 mb-2">Ejemplos de preguntas:</p>
        <div class="flex flex-wrap gap-2">
          <button
            v-for="example in examples"
            :key="example"
            @click="question = example"
            class="px-3 py-1 text-sm bg-gray-100 text-gray-700 rounded-full hover:bg-gray-200 transition"
          >
            {{ example }}
          </button>
        </div>
      </div>
    </div>

    <!-- Error Message -->
    <div v-if="error" class="bg-red-50 border border-red-500 text-red-600 rounded-lg p-4">
      <p class="font-medium">Error:</p>
      <p>{{ error }}</p>
    </div>

    <!-- Results Section -->
    <div v-if="result" class="bg-white rounded-lg shadow-md p-6">
      <h2 class="text-lg font-semibold text-gray-900 mb-4">Resultados</h2>
      
      <!-- SQL Query Generated -->
      <div v-if="result.query" class="mb-6">
        <p class="text-sm font-medium text-gray-700 mb-2">Consulta SQL generada:</p>
        <pre class="bg-gray-900 text-green-400 p-4 rounded-lg overflow-x-auto text-sm">{{ result.query }}</pre>
      </div>

      <!-- Results Table -->
      <div v-if="result.result && result.result.length > 0" class="overflow-x-auto">
        <p class="text-sm font-medium text-gray-700 mb-2">
          Resultados ({{ result.result.length }} registros):
        </p>
        <table class="min-w-full">
          <thead>
            <tr>
              <th v-for="key in Object.keys(result.result[0])" :key="key" class="text-left">
                {{ key }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, index) in result.result" :key="index">
              <td v-for="(value, key) in row" :key="key">
                {{ formatValue(value) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- No Results -->
      <div v-else-if="result.result && result.result.length === 0" class="text-center py-8 text-gray-500">
        No se encontraron resultados para tu consulta.
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 gap-6" style="grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));">
      <div class="bg-white rounded-lg shadow-md p-6">
        <h3 class="text-sm font-medium text-gray-500">Consultas del día</h3>
        <p class="text-3xl font-bold text-blue-600 mt-2">{{ queryCount }}</p>
      </div>
      <div class="bg-white rounded-lg shadow-md p-6">
        <h3 class="text-sm font-medium text-gray-500">Última consulta</h3>
        <p class="text-sm text-gray-900 mt-2">{{ lastQuery || 'Ninguna' }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { dashboardService } from '../services/dashboardService';

const question = ref('');
const loading = ref(false);
const error = ref('');
const result = ref(null);
const queryCount = ref(0);
const lastQuery = ref('');

const examples = [
  '¿Cuántos empleados hay?',
  '¿Cuántos empleados hay en el departamento de Tecnología?',
  '¿Cuál es el salario promedio?',
  '¿Cuántos empleados están activos?',
  'Lista los 5 empleados con mayor salario',
  '¿Cuántos empleados hay por departamento?',
];

const handleQuery = async () => {
  loading.value = true;
  error.value = '';
  result.value = null;

  try {
    const response = await dashboardService.query(question.value);
    if (response.success) {
      result.value = response;
      queryCount.value++;
      lastQuery.value = question.value;
    } else {
      error.value = response.message || 'Error al procesar la consulta';
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor';
  } finally {
    loading.value = false;
  }
};

const formatValue = (value) => {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    if (Number.isInteger(value)) return value.toString();
    return value.toFixed(2);
  }
  if (typeof value === 'boolean') return value ? 'Sí' : 'No';
  return value;
};
</script>

