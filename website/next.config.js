/** @type {import('next').Config} */
const nextConfig = {
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: 'avatars.steamstatic.com' },
    ],
  },
};

module.exports = nextConfig;
