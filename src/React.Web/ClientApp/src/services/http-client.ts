import axios from "axios";
import AuthService from "./auth-service";

const httpClient = axios.create({
  baseURL: process.env.REACT_APP_CATALOG_URI,
  responseType: "json",
  headers: {
    "Content-Type": "application/json",
  },
});

httpClient.interceptors.request.use(
  async (config) => {
    // perform a task before the request is sent
    console.log("Request was sent");
    const user = await AuthService.getUserAsync();
    config.headers.Authorization = `Bearer ${user?.access_token}`;
    return config;
  },
  (error) => {
    // handle the error
    return Promise.reject(error);
  }
);

// declare a response interceptor
httpClient.interceptors.response.use(
  (response) => {
    // do something with the response data
    console.log(response);
    console.log("Response was received");
    return response;
  },
  (error) => {
    // handle the response error
    return Promise.reject(error);
  }
);

export { httpClient };
