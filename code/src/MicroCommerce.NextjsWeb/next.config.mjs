/** @type {import('next').NextConfig} */
const nextConfig = {
  transpilePackages: ["@refinedev/antd"],
  logging: {
    fetches: {
      fullUrl: true,
    },
  },
};

export default nextConfig;
