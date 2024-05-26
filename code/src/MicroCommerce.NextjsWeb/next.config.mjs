/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  transpilePackages: ["@refinedev/antd"],
  logging: {
    fetches: { 
      fullUrl: true,
    },
  },
};

export default nextConfig;
