import axiosFetch from "@/api/axios";

export const login = async (username: string, password: string) => {
  try {
    await axiosFetch.post("api/auth/login", { username, password });
  } catch (error) {
    throw error;
  }
};

export const logout = async () => {
  try {
    await axiosFetch.post("api/auth/logout");
  } catch {
    // Можно не обрабатывать, если логаут — это просто очистка
  }
};

export const fetchCurrentUser = async () => {
  try {
    const res = await axiosFetch.get("api/auth/me");
    return res.data;
  } catch (error) {
    throw error;
  }
};
