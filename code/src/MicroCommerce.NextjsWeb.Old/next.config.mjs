/** @type {import('next').NextConfig} */
const nextConfig = {
  logging: {
    fetches: {
      fullUrl: false,
    },
  },
  experimental: {
    instrumentationHook: true,
  },
  output: "standalone",
};

export default nextConfig;
