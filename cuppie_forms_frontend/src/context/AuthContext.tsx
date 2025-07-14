import React, { createContext, useContext, useEffect, useState } from 'react'
import { User } from '../types/auth'
import { authApi } from '../lib/api'

interface AuthContextType {
  user: User | null
  isLoading: boolean
  login: (username: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => Promise<void>
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    checkAuthStatus()
  }, [])

  const checkAuthStatus = async () => {
    try {
      const response = await authApi.getMe()
      console.log('Auth check response:', response.data)
      
      // Проверяем разные возможные структуры ответа
      if (response.data.user) {
        setUser(response.data.user)
      } else if (response.data.id && response.data.username) {
        // Если данные пользователя приходят напрямую и содержат обязательные поля
        setUser(response.data as User)
      } else {
        console.warn('Invalid user data received:', response.data)
        setUser(null)
      }
    } catch (error) {
      console.error('Auth check error:', error)
      setUser(null)
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (username: string, password: string) => {
    try {
      const response = await authApi.login({ username, password })
      console.log('Login response:', response.data)
      
      // Проверяем разные возможные структуры ответа
      if (response.data.user) {
        setUser(response.data.user)
      } else if (response.data.id && response.data.username) {
        // Если данные пользователя приходят напрямую и содержат обязательные поля
        setUser(response.data as User)
      } else {
        // Если логин успешен, но данные пользователя не возвращены, получаем их отдельно
        await checkAuthStatus()
      }
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  }

  const register = async (username: string, email: string, password: string) => {
    try {
      const response = await authApi.register({ username, email, password })
      console.log('Register response:', response.data)
      
      // Проверяем разные возможные структуры ответа
      if (response.data.user) {
        setUser(response.data.user)
      } else if (response.data.id && response.data.username) {
        // Если данные пользователя приходят напрямую и содержат обязательные поля
        setUser(response.data as User)
      } else {
        // Если регистрация успешна, но данные пользователя не возвращены, получаем их отдельно
        await checkAuthStatus()
      }
    } catch (error) {
      console.error('Register error:', error)
      throw error
    }
  }

  const logout = async () => {
    try {
      await authApi.logout()
    } catch (error) {
      console.error('Logout error:', error)
      // Даже если запрос не удался, очищаем локальное состояние
    } finally {
      setUser(null)
    }
  }

  const value: AuthContextType = {
    user,
    isLoading,
    login,
    register,
    logout,
    isAuthenticated: !!user,
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}