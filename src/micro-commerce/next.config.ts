import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  output: "standalone", // required by Aspire's AddNextJsApp for publishing
  reactCompiler: true,
};

export default nextConfig;
