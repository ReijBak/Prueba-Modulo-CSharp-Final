import api from './api'

export const dashboardService = {
  async query(question) {
    const response = await api.post('/dashboard/query', { question })
    return response.data
  },
}

