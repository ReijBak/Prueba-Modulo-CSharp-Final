import api from './api'

export const empleadosService = {
  async getAll() {
    const response = await api.get('/empleados')
    return response.data
  },

  async getById(documento) {
    const response = await api.get(`/empleados/${documento}`)
    return response.data
  },

  async create(empleado) {
    const response = await api.post('/empleados', empleado)
    return response.data
  },

  async update(documento, empleado) {
    const response = await api.put(`/empleados/${documento}`, empleado)
    return response.data
  },

  async delete(documento) {
    await api.delete(`/empleados/${documento}`)
  },

  async importExcel(file) {
    const formData = new FormData()
    formData.append('file', file)
    const response = await api.post('/empleados/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data
  },

  async downloadResume(documento) {
    const response = await api.get(`/empleados/${documento}/resume`, {
      responseType: 'blob',
    })
    return response.data
  },
}

