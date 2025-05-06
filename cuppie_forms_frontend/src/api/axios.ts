import axios from "axios";

const apiAxios = axios.create({
  baseURL: "http://localhost:5000", // <- твой сервер
  withCredentials: true, // если ты используешь cookies (например, для JWT)
});

export default apiAxios;