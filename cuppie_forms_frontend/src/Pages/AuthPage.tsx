import React, { useState, useEffect } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { loginSchema, registerSchema, type LoginFormData, type RegisterFormData } from '../lib/validation'
import { z } from 'zod'
import { AxiosError } from 'axios'

const AuthPage: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true)
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: ''
  })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  const { login, register, isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      if (successMessage) {
        const timer = setTimeout(() => {
          navigate('/home', { replace: true })
        }, 1500)
        return () => clearTimeout(timer)
      } else {
        navigate('/home', { replace: true })
      }
    }
  }, [isLoading, isAuthenticated, navigate, successMessage])

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50">
        <div className="relative">
          <div className="animate-spin rounded-full h-12 w-12 border-4 border-slate-200 border-t-blue-600"></div>
          <div className="absolute inset-0 animate-pulse rounded-full h-12 w-12 border-4 border-transparent border-t-blue-400 opacity-40"></div>
        </div>
      </div>
    )
  }

  if (isAuthenticated && !successMessage) {
    return <Navigate to="/home" replace />
  }

  const getErrorMessage = (error: any): string => {
    if (error instanceof AxiosError) {
      const status = error.response?.status
      const message = error.response?.data?.message || error.message

      switch (status) {        
        case 401:
          return 'Неверное имя пользователя или пароль'
        case 409:
          return 'Пользователь с таким именем уже существует'
        case 500:
          return 'Внутренняя ошибка сервера'
        case 503:
          return 'Сервис временно недоступен'
        default:
          return message || 'Произошла неизвестная ошибка'
      }
    }
    return 'Произошла неизвестная ошибка'
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    
    // Очищаем ошибку для данного поля
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrors({})
    setSubmitError('')
    setSuccessMessage('')

    try {
      if (isLogin) {
        const loginData: LoginFormData = {
          username: formData.username,
          password: formData.password
        }
        loginSchema.parse(loginData)
        setIsSubmitting(true)
        await login(loginData.username, loginData.password)
        setSuccessMessage('Вход выполнен успешно! Перенаправление...')
      } else {
        const registerData: RegisterFormData = {
          username: formData.username,
          email: formData.email,
          password: formData.password
        }
        registerSchema.parse(registerData)
        setIsSubmitting(true)
        await register(registerData.username, registerData.email, registerData.password)
        setSuccessMessage('Регистрация прошла успешно! Перенаправление...')
      }
    } catch (error) {
      if (error instanceof z.ZodError) {
        const fieldErrors: Record<string, string> = {}
        error.errors.forEach(err => {
          if (err.path[0]) {
            fieldErrors[err.path[0] as string] = err.message
          }
        })
        setErrors(fieldErrors)
      } else {
        setSubmitError(getErrorMessage(error))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const switchTab = (loginMode: boolean) => {
    setIsLogin(loginMode)
    setFormData({ username: '', email: '', password: '' })
    setErrors({})
    setSubmitError('')
    setSuccessMessage('')
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-r from-blue-600 to-indigo-600 rounded-2xl mb-4 shadow-lg">
            <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2 bg-gradient-to-r from-slate-900 to-slate-700 bg-clip-text text-transparent">
            {isLogin ? 'Добро пожаловать' : 'Создать аккаунт'}
          </h1>
          <p className="text-slate-600 text-base">
            {isLogin ? 'Войдите в свой аккаунт' : 'Присоединяйтесь к нам сегодня'}
          </p>
        </div>

        {/* Main Card */}
        <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-xl border border-white/20 p-8">
          {/* Tab Switcher */}
          <div className="flex bg-slate-100 rounded-xl p-1 mb-8">
            <button
              type="button"
              onClick={() => switchTab(true)}
              className={`flex-1 py-3 px-4 rounded-lg text-sm font-semibold transition-all duration-300 outline-none relative overflow-hidden ${
                isLogin
                  ? 'bg-white text-slate-900 shadow-md transform scale-105'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'
              }`}
            >
              <span className="relative z-10">Вход</span>
              {isLogin && (
                <div className="absolute inset-0 bg-gradient-to-r from-blue-500/10 to-indigo-500/10 rounded-lg" />
              )}
            </button>
            <button
              type="button"
              onClick={() => switchTab(false)}
              className={`flex-1 py-3 px-4 rounded-lg text-sm font-semibold transition-all duration-300 outline-none relative overflow-hidden ${
                !isLogin
                  ? 'bg-white text-slate-900 shadow-md transform scale-105'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'
              }`}
            >
              <span className="relative z-10">Регистрация</span>
              {!isLogin && (
                <div className="absolute inset-0 bg-gradient-to-r from-blue-500/10 to-indigo-500/10 rounded-lg" />
              )}
            </button>
          </div>

          {/* Success Message */}
          {successMessage && (
            <div className="bg-gradient-to-r from-green-50 to-emerald-50 border border-green-200 rounded-xl p-4 mb-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="w-6 h-6 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-green-800 font-semibold">{successMessage}</p>
                </div>
              </div>
            </div>
          )}

          {/* Error Message */}
          {submitError && (
            <div className="bg-gradient-to-r from-red-50 to-pink-50 border border-red-200 rounded-xl p-4 mb-6">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg className="w-6 h-6 text-red-600" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-red-800 font-semibold">{submitError}</p>
                </div>
              </div>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Username Field */}
            <div>
              <label htmlFor="username" className="block text-sm font-semibold text-slate-700 mb-2">
                Имя пользователя
              </label>
              <input
                type="text"
                id="username"
                name="username"
                value={formData.username}
                onChange={handleInputChange}
                disabled={isSubmitting}
                placeholder={isLogin ? "Введите имя пользователя" : "Выберите имя пользователя"}
                className={`w-full px-4 py-3.5 bg-white border-0 rounded-xl shadow-sm transition-all duration-300 outline-none placeholder-slate-400 ${
                  errors.username 
                    ? 'ring-2 ring-red-400 bg-red-50 shadow-red-100' 
                    : 'ring-1 ring-slate-200 hover:ring-slate-300 focus:ring-2 focus:ring-blue-500 focus:bg-blue-50/30 hover:shadow-md'
                } ${isSubmitting ? 'bg-slate-50 ring-slate-100 cursor-not-allowed' : ''}`}
              />
              {errors.username && (
                <div className="mt-2 flex items-center text-red-600 bg-red-50 px-3 py-2 rounded-lg border border-red-200">
                  <svg className="w-4 h-4 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                  <span className="text-sm font-medium">{errors.username}</span>
                </div>
              )}
            </div>

            {/* Email Field (only for registration) */}
            {!isLogin && (
              <div>
                <label htmlFor="email" className="block text-sm font-semibold text-slate-700 mb-2">
                  Email
                </label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  disabled={isSubmitting}
                  placeholder="Введите email"
                  className={`w-full px-4 py-3.5 bg-white border-0 rounded-xl shadow-sm transition-all duration-300 outline-none placeholder-slate-400 ${
                    errors.email 
                      ? 'ring-2 ring-red-400 bg-red-50 shadow-red-100' 
                      : 'ring-1 ring-slate-200 hover:ring-slate-300 focus:ring-2 focus:ring-blue-500 focus:bg-blue-50/30 hover:shadow-md'
                  } ${isSubmitting ? 'bg-slate-50 ring-slate-100 cursor-not-allowed' : ''}`}
                />
                {errors.email && (
                  <div className="mt-2 flex items-center text-red-600 bg-red-50 px-3 py-2 rounded-lg border border-red-200">
                    <svg className="w-4 h-4 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    <span className="text-sm font-medium">{errors.email}</span>
                  </div>
                )}
              </div>
            )}

            {/* Password Field */}
            <div>
              <label htmlFor="password" className="block text-sm font-semibold text-slate-700 mb-2">
                Пароль
              </label>
              <input
                type="password"
                id="password"
                name="password"
                value={formData.password}
                onChange={handleInputChange}
                disabled={isSubmitting}
                placeholder={isLogin ? "Введите пароль" : "Создайте пароль"}
                className={`w-full px-4 py-3.5 bg-white border-0 rounded-xl shadow-sm transition-all duration-300 outline-none placeholder-slate-400 ${
                  errors.password 
                    ? 'ring-2 ring-red-400 bg-red-50 shadow-red-100' 
                    : 'ring-1 ring-slate-200 hover:ring-slate-300 focus:ring-2 focus:ring-blue-500 focus:bg-blue-50/30 hover:shadow-md'
                } ${isSubmitting ? 'bg-slate-50 ring-slate-100 cursor-not-allowed' : ''}`}
              />
              {errors.password && (
                <div className="mt-2 flex items-center text-red-600 bg-red-50 px-3 py-2 rounded-lg border border-red-200">
                  <svg className="w-4 h-4 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                  <span className="text-sm font-medium">{errors.password}</span>
                </div>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-3.5 px-6 rounded-xl hover:from-blue-700 hover:to-indigo-700 active:from-blue-800 active:to-indigo-800 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed font-semibold outline-none shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 active:translate-y-0"
            >
              {isSubmitting ? (
                <div className="flex items-center justify-center">
                  <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent mr-3"></div>
                  <span>{isLogin ? 'Вход...' : 'Регистрация...'}</span>
                </div>
              ) : (
                <span>{isLogin ? 'Войти' : 'Зарегистрироваться'}</span>
              )}
            </button>
          </form>
        </div>

        {/* Footer */}
      </div>
    </div>
  )
}

export default AuthPage