import api from './api'

export const catalogsService = {
  async getEstados() {
    const response = await api.get('/catalogs/estados')
    return response.data
  },

  async getDepartamentos() {
    const response = await api.get('/catalogs/departamentos')
    return response.data
  },

  async getCargos() {
    const response = await api.get('/catalogs/cargos')
    return response.data
  },

  async getNivelesEducativos() {
    const response = await api.get('/catalogs/niveles-educativos')
    return response.data
  },

  async getAllCatalogs() {
    const [estados, departamentos, cargos, nivelesEducativos] = await Promise.all([
      this.getEstados(),
      this.getDepartamentos(),
      this.getCargos(),
      this.getNivelesEducativos(),
    ])
    return { estados, departamentos, cargos, nivelesEducativos }
  },
}

