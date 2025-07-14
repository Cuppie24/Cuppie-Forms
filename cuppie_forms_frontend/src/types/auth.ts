export interface LoginData {
  username: string
  password: string
}

export interface RegisterData {
  username: string
  email: string
  password: string
}

export interface User {
  id: number
  username: string
  email: string
}

export interface AuthResponse {
  user?: User
  message?: string
  // Добавляем поля для случая, когда данные пользователя приходят напрямую
  id: number
  username?: string
  email?: string
}