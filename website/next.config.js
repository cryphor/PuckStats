/** @type {import('next').Config} */
const nextConfig = {
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: 'avatars.steamstatic.com' },
    ],
  },
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${process.env.BACKEND_URL || 'http://localhost:5000'}/api/:path*`,
      },
      {
        source: '/hubs/:path*',
        destination: `${process.env.BACKEND_URL || 'http://localhost:5000'}/hubs/:path*`,
      },
    ];
  },
};

module.exports = nextConfig;
