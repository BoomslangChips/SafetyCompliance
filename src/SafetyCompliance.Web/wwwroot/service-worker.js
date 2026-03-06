// ═══════════════════════════════════════════════════════════════
// SafeCheck Service Worker — Cache shell, fonts, icons offline
// ═══════════════════════════════════════════════════════════════

const CACHE_NAME = 'safecheck-v1';
const SHELL_ASSETS = [
    '/',
    '/_content/SafetyCompliance.Shared.UI/css/app.css',
    '/_content/SafetyCompliance.Shared.UI/js/app-utils.js',
    '/manifest.webmanifest'
];

// Install: pre-cache shell assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return cache.addAll(SHELL_ASSETS).catch(() => {
                // Some assets may not be available during first install
                console.log('[SW] Some shell assets could not be cached');
            });
        })
    );
    self.skipWaiting();
});

// Activate: clean up old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys
                .filter(k => k !== CACHE_NAME)
                .map(k => caches.delete(k))
            )
        )
    );
    self.clients.claim();
});

// Fetch: network-first for navigation/API, cache-first for static assets
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Skip non-GET requests
    if (event.request.method !== 'GET') return;

    // Skip SignalR/Blazor connections
    if (url.pathname.startsWith('/_blazor')) return;

    // Static assets (CSS, JS, fonts, images): cache-first
    if (isStaticAsset(url)) {
        event.respondWith(
            caches.match(event.request).then(cached => {
                if (cached) return cached;
                return fetch(event.request).then(response => {
                    if (response.ok) {
                        const clone = response.clone();
                        caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                    }
                    return response;
                }).catch(() => cached);
            })
        );
        return;
    }

    // Navigation: network-first with offline fallback
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() => caches.match('/'))
        );
        return;
    }
});

function isStaticAsset(url) {
    const path = url.pathname;
    return path.match(/\.(css|js|woff2?|ttf|eot|svg|png|jpg|jpeg|gif|ico|webp)$/i)
        || path.startsWith('/_content/')
        || path.startsWith('/icons/');
}
