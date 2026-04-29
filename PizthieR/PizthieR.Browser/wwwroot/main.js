import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

// Lance l'app Avalonia
await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);

// Quand Avalonia est montée, on supprime le splash custom
const splash = document.getElementById("splash");
if (splash) {
    splash.remove();
}

// =====================================================================
// Fix bande noire à droite : forcer le canvas Avalonia à occuper 100%
// du conteneur #out, et garder la synchronisation à chaque resize.
// =====================================================================
(function fixAvaloniaCanvasSize() {
    const host = document.getElementById('out');
    if (!host) return;

    function syncCanvas() {
        const dpr = window.devicePixelRatio || 1;
        // En mode PWA standalone iOS, window.innerWidth peut renvoyer une valeur
        // tronquée par les safe-areas. On prend le MAX disponible pour couvrir l'écran complet.
        const w = Math.max(
            window.innerWidth || 0,
            document.documentElement.clientWidth || 0,
            host.clientWidth || 0,
            screen.width || 0
        );
        const h = Math.max(
            window.innerHeight || 0,
            document.documentElement.clientHeight || 0,
            host.clientHeight || 0,
            screen.height || 0
        );

        // Force aussi #out à la bonne taille (au cas où)
        host.style.width  = w + 'px';
        host.style.height = h + 'px';

        // Tous les <canvas> que Avalonia a posés dans #out
        host.querySelectorAll('canvas').forEach((c) => {
            // Taille CSS (affichage)
            c.style.width  = w + 'px';
            c.style.height = h + 'px';
            c.style.position = 'absolute';
            c.style.top  = '0';
            c.style.left = '0';
            // Taille intrinsèque (buffer de rendu, en pixels physiques)
            const targetW = Math.round(w * dpr);
            const targetH = Math.round(h * dpr);
            if (c.width  !== targetW) c.width  = targetW;
            if (c.height !== targetH) c.height = targetH;
        });
        // Notifie Avalonia pour qu'il refasse son layout
        window.dispatchEvent(new Event('resize'));
    }

    // Première synchro + plusieurs essais (le canvas peut arriver après mount)
    syncCanvas();
    requestAnimationFrame(syncCanvas);
    setTimeout(syncCanvas, 100);
    setTimeout(syncCanvas, 500);
    setTimeout(syncCanvas, 1500);

    // Observe les changements de taille du conteneur (rotation, address bar iOS…)
    if (typeof ResizeObserver !== 'undefined') {
        new ResizeObserver(syncCanvas).observe(host);
    }

    // Observe l'ajout du canvas par Avalonia
    if (typeof MutationObserver !== 'undefined') {
        new MutationObserver(syncCanvas).observe(host, { childList: true, subtree: true });
    }

    window.addEventListener('resize', syncCanvas);
    window.addEventListener('orientationchange', syncCanvas);
})();
