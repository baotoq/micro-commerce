"use client";

import { useSession } from "next-auth/react";
import { useState, useEffect } from "react";

interface UserInfo {
  isAuthenticated: boolean;
  name: string | null;
  email: string | null;
  subject: string | null;
  claims: { type: string; value: string }[];
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string | null;
}

export function ApiTest() {
  const { data: session } = useSession();
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [weather, setWeather] = useState<WeatherForecast[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [apiBaseUrl, setApiBaseUrl] = useState<string>("http://localhost:5182");

  // Fetch API URL from server config (handles Aspire-injected env vars)
  useEffect(() => {
    fetch("/api/config")
      .then((res) => res.json())
      .then((data) => setApiBaseUrl(data.apiBaseUrl))
      .catch(() => {
        // Fallback already set
      });
  }, []);

  const fetchWithAuth = async (endpoint: string) => {
    if (!session?.accessToken) {
      setError("No access token available. Please sign in.");
      return null;
    }

    const response = await fetch(`${apiBaseUrl}${endpoint}`, {
      headers: {
        Authorization: `Bearer ${session.accessToken}`,
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    return response.json();
  };

  const handleFetchMe = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchWithAuth("/me");
      setUserInfo(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch user info");
    } finally {
      setLoading(false);
    }
  };

  const handleFetchWeather = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchWithAuth("/weatherforecast");
      setWeather(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch weather");
    } finally {
      setLoading(false);
    }
  };

  if (!session) {
    return (
      <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-6 dark:border-zinc-800 dark:bg-zinc-900">
        <p className="text-zinc-600 dark:text-zinc-400">
          Sign in to test the API endpoints
        </p>
      </div>
    );
  }

  return (
    <div className="w-full space-y-4">
      <div className="flex gap-3">
        <button
          onClick={handleFetchMe}
          disabled={loading}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:opacity-50"
        >
          {loading ? "Loading..." : "GET /me"}
        </button>
        <button
          onClick={handleFetchWeather}
          disabled={loading}
          className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-green-700 disabled:opacity-50"
        >
          {loading ? "Loading..." : "GET /weatherforecast"}
        </button>
      </div>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-400">
          <strong>Error:</strong> {error}
        </div>
      )}

      {userInfo && (
        <div className="rounded-lg border border-zinc-200 bg-white p-4 dark:border-zinc-800 dark:bg-zinc-900">
          <h3 className="mb-3 font-semibold text-zinc-900 dark:text-zinc-100">
            User Info from API
          </h3>
          <dl className="space-y-2 text-sm">
            <div className="flex gap-2">
              <dt className="font-medium text-zinc-600 dark:text-zinc-400">Authenticated:</dt>
              <dd className="text-zinc-900 dark:text-zinc-100">
                {userInfo.isAuthenticated ? "✅ Yes" : "❌ No"}
              </dd>
            </div>
            <div className="flex gap-2">
              <dt className="font-medium text-zinc-600 dark:text-zinc-400">Name:</dt>
              <dd className="text-zinc-900 dark:text-zinc-100">{userInfo.name || "N/A"}</dd>
            </div>
            <div className="flex gap-2">
              <dt className="font-medium text-zinc-600 dark:text-zinc-400">Email:</dt>
              <dd className="text-zinc-900 dark:text-zinc-100">{userInfo.email || "N/A"}</dd>
            </div>
            <div className="flex gap-2">
              <dt className="font-medium text-zinc-600 dark:text-zinc-400">Subject:</dt>
              <dd className="font-mono text-xs text-zinc-900 dark:text-zinc-100">
                {userInfo.subject || "N/A"}
              </dd>
            </div>
          </dl>
          <details className="mt-4">
            <summary className="cursor-pointer text-sm font-medium text-zinc-600 hover:text-zinc-900 dark:text-zinc-400 dark:hover:text-zinc-100">
              View all claims ({userInfo.claims.length})
            </summary>
            <pre className="mt-2 max-h-48 overflow-auto rounded bg-zinc-100 p-2 text-xs dark:bg-zinc-800">
              {JSON.stringify(userInfo.claims, null, 2)}
            </pre>
          </details>
        </div>
      )}

      {weather && (
        <div className="rounded-lg border border-zinc-200 bg-white p-4 dark:border-zinc-800 dark:bg-zinc-900">
          <h3 className="mb-3 font-semibold text-zinc-900 dark:text-zinc-100">
            Weather Forecast
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-zinc-200 dark:border-zinc-700">
                  <th className="py-2 text-left font-medium text-zinc-600 dark:text-zinc-400">Date</th>
                  <th className="py-2 text-left font-medium text-zinc-600 dark:text-zinc-400">Temp (°C)</th>
                  <th className="py-2 text-left font-medium text-zinc-600 dark:text-zinc-400">Temp (°F)</th>
                  <th className="py-2 text-left font-medium text-zinc-600 dark:text-zinc-400">Summary</th>
                </tr>
              </thead>
              <tbody>
                {weather.map((w) => (
                  <tr key={w.date} className="border-b border-zinc-100 dark:border-zinc-800">
                    <td className="py-2 text-zinc-900 dark:text-zinc-100">{w.date}</td>
                    <td className="py-2 text-zinc-900 dark:text-zinc-100">{w.temperatureC}°</td>
                    <td className="py-2 text-zinc-900 dark:text-zinc-100">{w.temperatureF}°</td>
                    <td className="py-2 text-zinc-900 dark:text-zinc-100">{w.summary}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-4 dark:border-zinc-800 dark:bg-zinc-900">
        <h4 className="mb-2 text-sm font-medium text-zinc-600 dark:text-zinc-400">
          Access Token (for debugging)
        </h4>
        <pre className="max-h-24 overflow-auto rounded bg-zinc-100 p-2 text-xs dark:bg-zinc-800">
          {session.accessToken?.substring(0, 50)}...
        </pre>
      </div>
    </div>
  );
}

