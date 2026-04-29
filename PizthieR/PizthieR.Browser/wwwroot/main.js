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

// Force Avalonia à remesurer la taille du canvas après retrait du splash.
// Sans ça, le canvas peut rester à une largeur calculée pendant que le splash
// occupait l'écran, laissant une bande noire à droite.
function forceAvaloniaResize() {
    window.dispatchEvent(new Event('resize'));
}
forceAvaloniaResize();
requestAnimationFrame(forceAvaloniaResize);
setTimeout(forceAvaloniaResize, 100);
setTimeout(forceAvaloniaResize, 500);
