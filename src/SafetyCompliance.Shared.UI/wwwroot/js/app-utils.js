// ═══════════════════════════════════════════════════════════════════
// SafeCheck — Client utilities (photo compression, offline, network)
// ═══════════════════════════════════════════════════════════════════

// ── Photo Compression ────────────────────────────────────────────

/**
 * Compress an image file using canvas.
 * Returns a base64 data URI of the compressed JPEG.
 * @param {string} dotNetStreamRef — not used, we read from inputId
 * @param {string} inputId — the file input element ID
 * @param {number} maxDim — max width or height (default 1920)
 * @param {number} quality — JPEG quality 0–1 (default 0.80)
 * @returns {Promise<{base64:string, fileName:string, contentType:string, size:number}>}
 */
/**
 * Draw a timestamp watermark on the bottom-right of a canvas context.
 */
function _drawTimestamp(ctx, w, h) {
    const now = new Date();
    const pad = n => String(n).padStart(2, '0');
    const stamp = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}  ${pad(now.getHours())}:${pad(now.getMinutes())}`;

    // Scale font size relative to image width (min 14px, max 36px)
    const fontSize = Math.max(14, Math.min(36, Math.round(w * 0.025)));
    ctx.font = `bold ${fontSize}px sans-serif`;

    const metrics = ctx.measureText(stamp);
    const textW = metrics.width;
    const textH = fontSize;
    const padX = fontSize * 0.6;
    const padY = fontSize * 0.4;
    const margin = fontSize * 0.5;

    // Semi-transparent dark background
    const bgX = w - textW - padX * 2 - margin;
    const bgY = h - textH - padY * 2 - margin;
    ctx.fillStyle = 'rgba(0, 0, 0, 0.55)';
    ctx.beginPath();
    const r = fontSize * 0.25;
    ctx.roundRect(bgX, bgY, textW + padX * 2, textH + padY * 2, r);
    ctx.fill();

    // White text
    ctx.fillStyle = '#ffffff';
    ctx.textBaseline = 'top';
    ctx.fillText(stamp, bgX + padX, bgY + padY);
}

window.compressPhoto = async function (inputId, maxDim, quality) {
    maxDim = maxDim || 1920;
    quality = quality || 0.80;

    const input = document.getElementById(inputId);
    if (!input || !input.files || !input.files[0]) throw new Error('No file selected');
    const file = input.files[0];

    // For small files we still need to stamp, so don't skip — just don't resize
    const needsResize = file.size >= 500 * 1024;

    const bitmap = await createImageBitmap(file);
    let w = bitmap.width, h = bitmap.height;

    // Scale down if larger than maxDim
    if (needsResize && (w > maxDim || h > maxDim)) {
        const ratio = Math.min(maxDim / w, maxDim / h);
        w = Math.round(w * ratio);
        h = Math.round(h * ratio);
    }

    const canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(bitmap, 0, 0, w, h);
    bitmap.close();

    // Always add timestamp watermark
    _drawTimestamp(ctx, w, h);

    const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/jpeg', quality));
    const arrayBuffer = await blob.arrayBuffer();
    const bytes = new Uint8Array(arrayBuffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i]);
    const base64 = btoa(binary);

    return {
        base64: base64,
        fileName: file.name.replace(/\.[^.]+$/, '.jpg'),
        contentType: 'image/jpeg',
        size: blob.size
    };
};

// ── Offline Inspection Queue (IndexedDB) ─────────────────────────

const DB_NAME = 'SafeCheckOffline';
const DB_VERSION = 1;
const STORE_QUEUE = 'syncQueue';
const STORE_CACHE = 'inspectionCache';

function openDb() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = () => {
            const db = req.result;
            if (!db.objectStoreNames.contains(STORE_QUEUE))
                db.createObjectStore(STORE_QUEUE, { keyPath: 'id', autoIncrement: true });
            if (!db.objectStoreNames.contains(STORE_CACHE))
                db.createObjectStore(STORE_CACHE, { keyPath: 'key' });
        };
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
    });
}

/** Queue an operation for later sync */
window.offlineQueueAdd = async function (action, payload) {
    const db = await openDb();
    const tx = db.transaction(STORE_QUEUE, 'readwrite');
    tx.objectStore(STORE_QUEUE).add({
        action: action,
        payload: JSON.parse(payload),
        timestamp: Date.now()
    });
    await new Promise((res, rej) => { tx.oncomplete = res; tx.onerror = rej; });
    db.close();
    return true;
};

/** Get count of pending sync items */
window.offlineQueueCount = async function () {
    const db = await openDb();
    const tx = db.transaction(STORE_QUEUE, 'readonly');
    const count = await new Promise((res, rej) => {
        const req = tx.objectStore(STORE_QUEUE).count();
        req.onsuccess = () => res(req.result);
        req.onerror = rej;
    });
    db.close();
    return count;
};

/** Get all pending items */
window.offlineQueueGetAll = async function () {
    const db = await openDb();
    const tx = db.transaction(STORE_QUEUE, 'readonly');
    const items = await new Promise((res, rej) => {
        const req = tx.objectStore(STORE_QUEUE).getAll();
        req.onsuccess = () => res(req.result);
        req.onerror = rej;
    });
    db.close();
    return JSON.stringify(items);
};

/** Remove an item by id after successful sync */
window.offlineQueueRemove = async function (id) {
    const db = await openDb();
    const tx = db.transaction(STORE_QUEUE, 'readwrite');
    tx.objectStore(STORE_QUEUE).delete(id);
    await new Promise((res, rej) => { tx.oncomplete = res; tx.onerror = rej; });
    db.close();
};

/** Clear entire queue */
window.offlineQueueClear = async function () {
    const db = await openDb();
    const tx = db.transaction(STORE_QUEUE, 'readwrite');
    tx.objectStore(STORE_QUEUE).clear();
    await new Promise((res, rej) => { tx.oncomplete = res; tx.onerror = rej; });
    db.close();
};

/** Cache inspection data for offline use */
window.offlineCacheSet = async function (key, data) {
    const db = await openDb();
    const tx = db.transaction(STORE_CACHE, 'readwrite');
    tx.objectStore(STORE_CACHE).put({ key: key, data: data, cachedAt: Date.now() });
    await new Promise((res, rej) => { tx.oncomplete = res; tx.onerror = rej; });
    db.close();
};

/** Get cached inspection data */
window.offlineCacheGet = async function (key) {
    const db = await openDb();
    const tx = db.transaction(STORE_CACHE, 'readonly');
    const result = await new Promise((res, rej) => {
        const req = tx.objectStore(STORE_CACHE).get(key);
        req.onsuccess = () => res(req.result);
        req.onerror = rej;
    });
    db.close();
    return result ? result.data : null;
};

// ── Network Status ───────────────────────────────────────────────

window.getNetworkStatus = function () {
    return navigator.onLine;
};

/** Register callbacks for online/offline events */
window.registerNetworkCallbacks = function (dotNetRef) {
    window._networkDotNetRef = dotNetRef;

    window.addEventListener('online', function () {
        if (window._networkDotNetRef) {
            window._networkDotNetRef.invokeMethodAsync('OnNetworkStatusChanged', true);
        }
    });
    window.addEventListener('offline', function () {
        if (window._networkDotNetRef) {
            window._networkDotNetRef.invokeMethodAsync('OnNetworkStatusChanged', false);
        }
    });
};

// ── Haptic Feedback (mobile vibration API) ───────────────────────

window.hapticTap = function () {
    if (navigator.vibrate) navigator.vibrate(10);
};

window.hapticSuccess = function () {
    if (navigator.vibrate) navigator.vibrate([10, 50, 10]);
};

// ── Scroll helpers ───────────────────────────────────────────────

window.scrollToElement = function (selector) {
    const el = document.querySelector(selector);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
};

// ── Install prompt (PWA) ─────────────────────────────────────────

let deferredPrompt = null;
window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
});

window.canInstallPwa = function () {
    return deferredPrompt !== null;
};

window.installPwa = async function () {
    if (!deferredPrompt) return false;
    deferredPrompt.prompt();
    const { outcome } = await deferredPrompt.userChoice;
    deferredPrompt = null;
    return outcome === 'accepted';
};
