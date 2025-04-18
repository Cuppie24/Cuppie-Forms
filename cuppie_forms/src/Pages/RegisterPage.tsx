import { useForm } from "react-hook-form";
import { z } from "zod";
import apiAxios from "../api/axios.ts";
import axios from "axios";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { Link } from "react-router-dom";

// 💡 Zod-схема валидации
const schema = z.object({
  username: z.string().min(1, "Имя пользователя обязательно").max(30, "Максимум 30 символов"),
  email: z.string().email("Некорректный Email"),
  password: z.string().min(6, "Минимум 6 символов").max(100, "Максимум 100 символов"),
});

type RegisterFormData = z.infer<typeof schema>;

export default function RegisterForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({ resolver: zodResolver(schema) });

  const [responseMessage, setResponseMessage] = useState<string>("");
  const [isSuccess, setIsSuccess] = useState<boolean>(false);

  const onSubmit = async (data: RegisterFormData) => {
    try {
      const response = await apiAxios.post("/api/auth/register", data);

      if (response.status === 200 || response.status === 201) {
        setResponseMessage("✅ Пользователь успешно создан!");
        setIsSuccess(true);
      }
    } catch (err: any) {
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 409) {
          setResponseMessage("❌ Пользователь с таким именем уже зарегистрирован");
        } else if (err.response?.status === 400) {
          setResponseMessage("❌ Ошибка: некорректные данные");
        } else {
          setResponseMessage(`❌ Ошибка: ${err.response?.status}`);
        }
      } else {
        setResponseMessage("❌ Сервер не найден или нет соединения.");
      }
      setIsSuccess(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-[#121212] text-[#E0E0E0]">
      <div className="relative bg-[#1E1E1E] p-8 rounded-3xl shadow-xl max-w-md w-full">
        <h2 className="text-3xl font-bold text-center text-[#FF6B6B] mb-6">Регистрация</h2>

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
              {...register("email")}
              placeholder="Email"
              className="w-full p-4 border border-[#FF6B6B] rounded-xl bg-[#292929] text-white focus:ring-2 focus:ring-[#FF6B6B] placeholder-gray-400 transition-all"
            />
            {errors.email && (
              <p className="text-red-500 text-sm mt-1">{errors.email.message}</p>
            )}
          </div>

          <div>
            <input
              {...register("password")}
              placeholder="Пароль"
              type="password"
              className="w-full p-4 border border-[#FF6B6B] rounded-xl bg-[#292929] text-white focus:ring-2 focus:ring-[#FF6B6B] placeholder-gray-400 transition-all"
            />
            {errors.password && (
              <p className="text-red-500 text-sm mt-1">{errors.password.message}</p>
            )}
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full p-4 bg-[#FF6B6B] text-white rounded-xl hover:bg-[#E04E4E] hover:shadow-lg transition-all duration-300 font-medium"
          >
            Зарегистрироваться
          </button>
        </form>

        {responseMessage && (
          <div className={`mt-4 p-3 rounded-lg text-center ${isSuccess ? "bg-green-100/10" : "bg-red-100/10"}`}>
            <p className="text-sm" style={{ color: isSuccess ? "#4CAF50" : "#FF6B6B" }}>
              {responseMessage}
            </p>
          </div>
        )}

        <div className="flex items-center justify-center mt-6">
          <span className="text-gray-400">Уже есть аккаунт?</span>
          <Link to="/login" className="text-[#FF6B6B] hover:underline transition-all ml-2">Войти</Link>
        </div>
      </div>
    </div>
  );
}
