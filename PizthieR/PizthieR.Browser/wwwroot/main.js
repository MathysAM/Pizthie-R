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
    setTimeout(function () {
        const canvas = document.querySelector('#out canvas');
        if (canvas) {
            const w = Math.round(window.innerWidth  * window.devicePixelRatio);
            const h = Math.round(window.innerHeight * window.devicePixelRatio);
            if (canvas.width !== w || canvas.height !== h) {
                canvas.width  = w;
                canvas.height = h;
                window.dispatchEvent(new Event('resize'));
            }
        }
    }, 300);
}

