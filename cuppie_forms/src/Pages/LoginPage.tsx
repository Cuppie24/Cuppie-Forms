import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import apiAxios from "../api/axios.ts";

// Схема Zod для валидации
const loginSchema = z.object({
  username: z.string().nonempty("Имя пользователя обязательно"),
  password: z
    .string()
    .min(6, "Пароль должен содержать минимум 6 символов")
    .nonempty("Пароль обязателен"),
});

type LoginDto = z.infer<typeof loginSchema>;

export default function Login() {
  const navigate = useNavigate();
  const [responseMessage, setResponseMessage] = useState<string>("");
  const [isSuccess, setIsSuccess] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginDto>({
    resolver: zodResolver(loginSchema),
  });

 

  const onSubmit = async (data: LoginDto) => {
    try {
      await apiAxios.post("/api/auth/login", data);
  
      setIsSuccess(true);
      setResponseMessage("✅ Успешный вход");
      navigate("/");
    } catch (error: any) {
      setIsSuccess(false);
  
      const status = error.response?.status;
  
      switch (status) {
        case 400:
          setResponseMessage("❌ Ошибка: некорректные данные");
          break;
        case 401:
          setResponseMessage("❌ Неверное имя пользователя или пароль");
          break;
        case 403:
          setResponseMessage("❌ Доступ запрещен. Обратитесь к администратору.");
          break;
        case 404:
          setResponseMessage("❌ Сервер авторизации недоступен");
          break;
        case 500:
          setResponseMessage("❌ Внутренняя ошибка сервера. Попробуйте позже.");
          break;
        default:
          setResponseMessage(`❌ Ошибка: код ${status || "неизвестно"}`);
          break;
      }
    }
  };
  

  return (
    <div className="flex items-center justify-center min-h-screen bg-[#121212]">
      <div className="relative bg-[#1E1E1E] p-8 rounded-3xl shadow-2xl max-w-md w-full">
        <h2 className="text-3xl font-bold text-center text-[#FF6B6B] mb-6">Вход</h2>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <input
              {...register("username")}
              placeholder="Имя пользователя"
              className="w-full p-4 border border-[#FF6B6B] rounded-xl bg-[#292929] text-white focus:ring-2 focus:ring-[#FF6B6B] placeholder-gray-400 transition-all"
            />
            {errors.username && (
              <p className="text-red-500 text-sm mt-1">{errors.username.message}</p>
            )}
          </div>

          <div>
            <input
              {...register("password")}
              type="password"
              placeholder="Пароль"
              className="w-full p-4 border border-[#FF6B6B] rounded-xl bg-[#292929] text-white focus:ring-2 focus:ring-[#FF6B6B] placeholder-gray-400 transition-all"
            />
            {errors.password && (
              <p className="text-red-500 text-sm mt-1">{errors.password.message}</p>
            )}
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full p-4 bg-[#FF6B6B] text-white rounded-xl hover:bg-[#E04E4E] hover:shadow-lg transition-all duration-300 font-medium disabled:opacity-60"
          >
            {isSubmitting ? "Вход..." : "Войти"}
          </button>
        </form>

        {responseMessage && (
          <div
            className={`mt-4 p-3 rounded-lg text-center ${
              isSuccess ? "bg-green-100/10" : "bg-red-100/10"
            }`}
          >
            <p className="text-sm" style={{ color: isSuccess ? "#4CAF50" : "#FF6B6B" }}>
              {responseMessage}
            </p>
          </div>
        )}

        <div className="flex items-center justify-center mt-6">
          <span className="text-[#A0A0A0]">Нет аккаунта?</span>
          <a href="/register" className="text-[#FF6B6B] hover:underline transition-all ml-2">
            Зарегистрироваться
          </a>
        </div>
      </div>
    </div>
  );
}
