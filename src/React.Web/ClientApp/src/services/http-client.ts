import axios from "axios";

const httpClient = axios.create({
  baseURL: process.env.REACT_APP_RESOURCE_URI,
  responseType: "json",
  headers: {
    "Content-Type": "application/json",
  },
});

httpClient.interceptors.request.use(
  (config) => {
    // perform a task before the request is sent
    console.log("Request was sent");

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
    console.log("Response was received");
    console.log(response);
    return response;
  },
  (error) => {
    // handle the response error
    return Promise.reject(error);
  }
);

export { httpClient };
