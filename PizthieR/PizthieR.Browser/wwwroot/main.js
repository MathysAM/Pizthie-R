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

// Supprime le splash quand Avalonia est montee
const splash = document.getElementById("splash");
if (splash) {
    splash.remove();
}

// Fix iOS standalone : force le canvas a la bonne taille exacte en pixels entiers
if (window.navigator.standalone) {
    const syncStandaloneCanvas = function () {
        const canvas = document.querySelector('#out canvas');
        if (!canvas) return;

        const vv = window.visualViewport;
        const cssWidth = Math.round(vv ? vv.width : window.innerWidth);
        const cssHeight = Math.round(vv ? vv.height : window.innerHeight);
        const dpr = window.devicePixelRatio || 1;

        const pixelWidth = Math.round(cssWidth * dpr);
        const pixelHeight = Math.round(cssHeight * dpr);

        canvas.style.width = `${cssWidth}px`;
        canvas.style.height = `${cssHeight}px`;

        if (canvas.width !== pixelWidth || canvas.height !== pixelHeight) {
            canvas.width = pixelWidth;
            canvas.height = pixelHeight;
            window.dispatchEvent(new Event('resize'));
        }
    };

    setTimeout(syncStandaloneCanvas, 300);
    window.addEventListener('resize', syncStandaloneCanvas);
    window.visualViewport?.addEventListener('resize', syncStandaloneCanvas);
}

