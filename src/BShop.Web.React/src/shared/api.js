import axios from "axios";

const apiClient = axios.create({ baseURL: "http://localhost" });

apiClient.interceptors.request.use(
  config => {
    return {
      ...config,
      headers: {
        Authorization: "Bearer"
      }
    };
  },
  error => Promise.reject(error)
);

apiClient.interceptors.response.use(
  response => response,
  async error => {
    return Promise.reject(error);
  }
);

export default apiClient;
