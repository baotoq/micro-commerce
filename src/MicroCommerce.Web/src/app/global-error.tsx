"use client";

import { useEffect } from "react";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Global error:", error);
  }, [error]);

  return (
    <html lang="en">
      <body>
        <div
          style={{
            display: "flex",
            minHeight: "100vh",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            fontFamily: "system-ui, sans-serif",
          }}
        >
          <h2 style={{ marginBottom: "8px", fontSize: "20px", fontWeight: 600 }}>
            Something went wrong
          </h2>
          <p style={{ marginBottom: "24px", fontSize: "14px", color: "#71717a" }}>
            A critical error occurred. Please try again.
          </p>
          <button
            onClick={reset}
            style={{
              padding: "8px 16px",
              fontSize: "14px",
              fontWeight: 500,
              color: "#fff",
              backgroundColor: "#18181b",
              border: "none",
              borderRadius: "8px",
              cursor: "pointer",
            }}
          >
            Try again
          </button>
        </div>
      </body>
    </html>
  );
}
