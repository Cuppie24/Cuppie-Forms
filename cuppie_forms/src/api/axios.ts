import axios from "axios";

const apiAxios = axios.create({
  baseURL: "https://localhost:5001", // <- твой сервер
  withCredentials: true, // если ты используешь cookies (например, для JWT)
});

export default apiAxios;