"use client";

import type { AuthProvider } from "@refinedev/core";

const API_URL = "https://localhost:7477";

export const authProvider: AuthProvider = {
  login: async ({ email, username, password, remember }) => {
    try {
      const response = await fetch(`${API_URL}/login`, {
        method: "POST",
        body: JSON.stringify({ email, password }),
        headers: {
          "Content-Type": "application/json",
        },
      });

      const { accessToken } = await response.json();

      localStorage.setItem("token", accessToken);

      return {
        success: true,
        redirectTo: "/",
      };
    } catch {
      return {
        success: false,
        error: {
          name: "LoginError",
          message: "Invalid username or password",
        },
      };
    }
  },
  logout: async () => {
    localStorage.removeItem("token");
    return {
      success: true,
      redirectTo: "/login",
    };
  },
  check: async () => {
    const auth = localStorage.getItem("token");
    if (auth) {
      return {
        authenticated: true,
      };
    }

    return {
      authenticated: false,
      logout: true,
      redirectTo: "/login",
    };
  },
  getPermissions: async () => {
    const auth = localStorage.getItem("token");
    if (auth) {
      const parsedUser = JSON.parse(auth);
      return parsedUser.roles;
    }
    return null;
  },
  getIdentity: async () => {
    const auth = localStorage.getItem("token");
    if (auth) {
      const parsedUser = JSON.parse(auth);
      return parsedUser;
    }
    return null;
  },
  onError: async (error) => {
    if (error.response?.status === 401) {
      return {
        logout: true,
      };
    }

    return { error };
  },
};
