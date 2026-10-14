import axios from "axios";
import { getToken, clearToken } from "../auth/tokenStorage";

export const API_URL = import.meta.env.VITE_API_URL;

const client = axios.create({ baseURL: `${API_URL}/api` });

client.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

client.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const isSignInCall = error.config?.url?.startsWith("/auth/");
    if (status === 401 && !isSignInCall) {
      clearToken();
      window.location.assign("/sign-in");
    }

    if (status === 403) {
      window.dispatchEvent(new Event("api:forbidden"));
    }

    return Promise.reject(error);
  },
);

export default client;
