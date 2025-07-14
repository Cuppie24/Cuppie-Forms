import React, { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import { pagesApi } from '../lib/api'
import { LogOut, Mail, User, Lock } from 'lucide-react'

const HomePage: React.FC = () => {
  const { user, logout } = useAuth()
  const [homeContent, setHomeContent] = useState<string>('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string>('')

  useEffect(() => {
    fetchHomeContent()
  }, [])

  const fetchHomeContent = async () => {
    try {
      const response = await pagesApi.getHome()
      setHomeContent(response.data.message)
    } catch (err) {
      setError('Произошла ошибка при получении данных с сервера.')
    } finally {
      setIsLoading(false)
    }
  }

  const handleLogout = async () => {
    try {
      await logout()
    } catch (err) {
      console.error('Ошибка при выходе:', err)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-100 via-white to-blue-100 flex items-center justify-center p-6">
      <div className="w-full max-w-3xl bg-white rounded-3xl shadow-2xl overflow-hidden animate-fade-in border border-slate-200">
        <div className="bg-gradient-to-r from-indigo-600 to-blue-600 text-white px-8 py-6 flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold">Добро пожаловать, {user?.username}</h1>
            <p className="text-sm opacity-90">Мы рады видеть вас снова!</p>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 bg-white text-indigo-600 px-4 py-2 rounded-xl shadow hover:bg-slate-100 transition"
          >
            <LogOut className="w-4 h-4" />
            Выйти
          </button>
        </div>

        <div className="px-8 py-6 grid grid-cols-1 md:grid-cols-2 gap-6 bg-slate-50">
          <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col gap-3">
            <h2 className="text-lg font-semibold text-slate-800">Данные пользователя</h2>
            <div className="flex items-center gap-3 text-slate-700">
              <User className="w-5 h-5 text-indigo-500" />
              <span>{user?.username}</span>
            </div>
            <div className="flex items-center gap-3 text-slate-700">
              <Mail className="w-5 h-5 text-indigo-500" />
              <span>{user?.email}</span>
            </div>
          </div>

          <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col gap-3">
            <h2 className="text-lg font-semibold text-slate-800">Приватные данные с сервера</h2>

            {isLoading ? (
              <div className="flex justify-center items-center h-24">
                <div className="animate-spin rounded-full h-8 w-8 border-4 border-slate-200 border-t-indigo-500" />
              </div>
            ) : error ? (
              <div className="text-red-600 bg-red-100 border border-red-200 p-4 rounded-lg">
                {error}
              </div>
            ) : (
              <div className="flex justify-center items-center h-16">
                <div className="flex items-start gap-3 text-slate-700 whitespace-pre-wrap">
                  <Lock className="w-5 h-5 text-indigo-500 mt-1 shrink-0" />
                  <span>{homeContent}</span>
                </div>
              </div>

            )}
          </div>
        </div>
      </div>
    </div>
  )
}

export default HomePage
