  import { useForm } from "react-hook-form";
  import { z } from "zod";
  import { zodResolver } from "@hookform/resolvers/zod";
  import { useState } from "react";
  import { useAuth } from "@/context/AuthContext";
  import { Link, useNavigate } from "react-router-dom";

  const loginSchema = z.object({
    username: z.string().nonempty("Введите имя пользователя"),
    password: z.string().min(6, "Минимум 6 символов").nonempty("Введите пароль"),
  });

  type LoginDto = z.infer<typeof loginSchema>;

  export default function LoginForm({
    onSuccess,
    onSwitch,
  }: {
    onSuccess?: () => void;
    onSwitch: () => void;
  }) {
    const { login } = useAuth();
    const [serverError, setServerError] = useState("");
    const [isSuccess, setIsSuccess] = useState(false);
    const navigate = useNavigate();

    const {
      register,
      handleSubmit,
      formState: { errors, isSubmitting, isSubmitted },
    } = useForm<LoginDto>({
      resolver: zodResolver(loginSchema),
      mode: "onSubmit",
    });

    const onSubmit = async (data: LoginDto) => {
      setServerError("");

      try {
        await login(data.username, data.password);
        setIsSuccess(true);
        onSuccess?.(); // коллбэк при успехе`
        // Добавляем паузу перед редиректом
        setTimeout(() => {
          navigate("/"); // редирект на главную страницу
        }, 1500); // пауза 1.5 секунды
      } catch (error: any) {
        setIsSuccess(false);
        const status = error.response?.status;

        if (status === 401 || status === 400) {
          setServerError("Неверные данные. Проверьте логин и пароль.");
        } else if (status === 403) {
          setServerError("Доступ запрещён. Обратитесь к администратору.");
        } else {
          setServerError("Что-то пошло не так. Попробуйте позже.");
        }
      }
    };

    return (
      <form onSubmit={handleSubmit(onSubmit)} className="bg-gray-900 backdrop-blur-xl p-6 rounded-3xl border border-gray-700 space-y-4" noValidate>
        <div>
          <input
            {...register("username")}
            type="text"
            placeholder="Имя пользователя"
            className={`w-full p-4 rounded-xl bg-gray-800 text-gray-100 placeholder-gray-400 border transition-all focus:ring-2 focus:outline-none ${errors.username && isSubmitted
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
            {...register("password")}
            type="password"
            placeholder="Пароль"
            className={`w-full p-4 rounded-xl bg-gray-800 text-gray-100 placeholder-gray-400 border transition-all focus:ring-2 focus:outline-none ${errors.password && isSubmitted
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
          {isSubmitting ? "Вход..." : "Войти"}
        </button>

        {(serverError || isSuccess) && (
          <div
            className={`mt-4 p-3 rounded-lg text-center text-sm ${isSuccess
                ? "bg-green-600/20 text-green-300"
                : "bg-red-600/20 text-red-300"
              }`}
          >
            {isSuccess ? "✅ Успешный вход" : serverError}
          </div>
        )}

        <div className="mt-6 text-sm text-gray-300 text-center space-y-4">
            <div>
              Нет аккаунта?{" "}
              <button
                type="button"
                onClick={onSwitch}
                className="text-blue-400 hover:underline hover:text-blue-300 transition"
              >
                Зарегистрироваться
              </button>
            </div>
            <div>
              <Link
                to="/forgot-password"
                className="text-blue-400 hover:underline hover:text-blue-300 transition"
              >
                Забыли пароль?
              </Link>
            </div>
          </div>
        
      </form>
    );
  }
