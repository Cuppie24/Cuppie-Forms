import { useForm } from "react-hook-form";
import { z } from "zod";
import apiAxios from "@/api/axios.ts";
import axios from "axios";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";

const schema = z.object({
  username: z.string().min(1, "Имя пользователя обязательно").max(30, "Максимум 30 символов"),
  email: z.string().email("Некорректный Email"),
  password: z.string().min(6, "Минимум 6 символов").max(100, "Максимум 100 символов"),
});

type RegisterFormData = z.infer<typeof schema>;

export default function RegisterForm({
  onSuccess,
  onSwitch,
}: {
  onSuccess?: () => void;
  onSwitch: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitted },
  } = useForm<RegisterFormData>({ resolver: zodResolver(schema) });

  const [responseMessage, setResponseMessage] = useState<string>("");
  const [isSuccess, setIsSuccess] = useState<boolean>(false);

  const onSubmit = async (data: RegisterFormData) => {
    try {
      const response = await apiAxios.post("auth/register", data);

      if (response.status === 200 || response.status === 201) {
        setResponseMessage("✅ Пользователь успешно создан!");
        setIsSuccess(true);
        onSuccess?.(); // коллбэк при успехе
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
    <form onSubmit={handleSubmit(onSubmit)} className="bg-gray-900 backdrop-blur-xl p-6 rounded-3xl border border-gray-700 space-y-4" noValidate>
      <div>
        <input
          {...register("username")}
          placeholder="Имя пользователя"
          className={`w-full p-4 rounded-xl bg-gray-800 text-gray-100 placeholder-gray-400 border transition-all focus:ring-2 focus:outline-none ${
            errors.username && isSubmitted
              ? "border-red-500 focus:ring-red-500"
              : "border-blue-500 focus:ring-blue-400"
          }`}
        />
        {errors.username && isSubmitted && (
          <p className="text-red-400 text-sm mt-1">{errors.username.message}</p>
        )}
      </div>

      <div>
        <input
          {...register("email")}
          placeholder="Email"
          className={`w-full p-4 rounded-xl bg-gray-800 text-gray-100 placeholder-gray-400 border transition-all focus:ring-2 focus:outline-none ${
            errors.email && isSubmitted
              ? "border-red-500 focus:ring-red-500"
              : "border-blue-500 focus:ring-blue-400"
          }`}
        />
        {errors.email && isSubmitted && (
          <p className="text-red-400 text-sm mt-1">{errors.email.message}</p>
        )}
      </div>

      <div>
        <input
          {...register("password")}
          placeholder="Пароль"
          type="password"
          className={`w-full p-4 rounded-xl bg-gray-800 text-gray-100 placeholder-gray-400 border transition-all focus:ring-2 focus:outline-none ${
            errors.password && isSubmitted
              ? "border-red-500 focus:ring-red-500"
              : "border-blue-500 focus:ring-blue-400"
          }`}
        />
        {errors.password && isSubmitted && (
          <p className="text-red-400 text-sm mt-1">{errors.password.message}</p>
        )}
      </div>

      <button
        type="submit"
        disabled={isSubmitting}
        className="w-full p-4 bg-blue-600 text-white rounded-xl hover:bg-blue-500 hover:shadow-lg transition-all duration-300 font-medium disabled:opacity-60"
      >
        {isSubmitting ? "Регистрация..." : "Зарегистрироваться"}
      </button>

      {responseMessage && (
        <div
          className={`mt-4 p-3 rounded-lg text-center text-sm ${
            isSuccess
              ? "bg-green-600/20 text-green-300"
              : "bg-red-600/20 text-red-300"
          }`}
        >
          {responseMessage}
        </div>
      )}

      <div className="flex items-center justify-center mt-6 text-sm text-gray-300">
        <span>Уже есть аккаунт?</span>
        <button
          type="button"
          onClick={onSwitch}
          className="ml-2 text-blue-400 hover:underline hover:text-blue-300 transition"
        >
          Войти
        </button>
      </div>

    </form>
  );
}
