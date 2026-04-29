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

// Correction iOS standalone : window.innerWidth peut être incorrect au démarrage.
// On force plusieurs resize pour que le canvas Avalonia se recalcule correctement.
function forceResize() {
    window.dispatchEvent(new Event('resize'));
}
forceResize();
setTimeout(forceResize, 100);
setTimeout(forceResize, 500);
setTimeout(forceResize, 1000);

// Sur iOS, réajuster quand l'orientation change
window.addEventListener('orientationchange', function () {
    setTimeout(forceResize, 300);
});
