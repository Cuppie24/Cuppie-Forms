import axios, { AxiosError, AxiosRequestConfig } from 'axios'
import { LoginData, RegisterData, AuthResponse } from '../types/auth'

const API_BASE_URL = `${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/api`

// Расширяем конфигурацию запроса для поддержки `_retry`
interface RetryAxiosRequestConfig extends AxiosRequestConfig {
  _retry?: boolean
}

// Очередь запросов при обновлении токена
let isRefreshing = false
let failedQueue: {
  resolve: (value?: unknown) => void
  reject: (reason?: any) => void
}[] = []

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) prom.reject(error)
    else prom.resolve(token)
  })
  failedQueue = []
}

export const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Интерсептор ответа для автоматического рефреша токена
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryAxiosRequestConfig

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !originalRequest.url?.includes('/auth/refresh')
    ) {
      originalRequest._retry = true

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then(() => api(originalRequest))
          .catch((err) => Promise.reject(err))
      }

      isRefreshing = true

      try {
        await api.post('/auth/refresh')
        processQueue(null)
        return api(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  }
)

// Методы авторизации
export const authApi = {
  login: (data: LoginData): Promise<{ data: AuthResponse }> => api.post('/auth/login', data),
  register: (data: RegisterData): Promise<{ data: AuthResponse }> => api.post('/auth/register', data),
  logout: (): Promise<void> => api.post('/auth/logout'),
  refresh: (): Promise<{ data: AuthResponse }> => api.post('/auth/refresh'),
  getMe: (): Promise<{ data: AuthResponse }> => api.get('/auth/me'),
}

// Методы получения страниц
export const pagesApi = {
  getHome: (): Promise<{ data: { message: string } }> => api.get('/pages/home/get'),
}
