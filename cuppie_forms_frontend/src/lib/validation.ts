import { z } from 'zod'

export const loginSchema = z.object({
  username: z.string()
    .min(1, 'Имя пользователя обязательно')
    .max(30, 'Имя пользователя не может быть длиннее 30 символов'),
  password: z.string()
    .min(1, 'Пароль обязателен'),
})

export const registerSchema = z.object({
  username: z.string()
    .min(1, 'Имя пользователя обязательно')
    .max(30, 'Имя пользователя не может быть длиннее 30 символов')
    .regex(/^[a-zA-Z0-9_-]+$/, 'Имя пользователя может содержать только буквы, цифры, дефисы и подчеркивания'),
  email: z.string()
    .email('Неверный формат email')
    .min(6, 'Email должен содержать минимум 6 символов')
    .max(100, 'Email не может быть длиннее 100 символов'),
  password: z.string()
    .min(6, 'Пароль должен содержать минимум 6 символов')
    .max(128, 'Пароль не может быть длиннее 128 символов')
    .regex(/^.{6,}$/, 'Пароль должен содержать минимум 6 символов')
})

export type LoginFormData = z.infer<typeof loginSchema>
export type RegisterFormData = z.infer<typeof registerSchema>