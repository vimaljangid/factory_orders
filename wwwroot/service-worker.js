// Lightweight Service Worker for Factory Orders PWA
const CACHE_NAME = 'factory-orders-v1';

self.addEventListener('install', (event) => {
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', (event) => {
    // Let Blazor WebSockets and dynamic requests pass directly through
    if (event.request.url.includes('/_blazor') || event.request.method !== 'GET') {
        return;
    }
    event.respondWith(
        fetch(event.request).catch(() => caches.match(event.request))
    );
});
