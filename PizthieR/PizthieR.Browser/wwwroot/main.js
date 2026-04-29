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

// Après retrait du splash, déclenche un 'resize' pour qu'Avalonia
// remesure le viewport (utile en PWA standalone iOS).
function triggerResize() {
    window.dispatchEvent(new Event('resize'));
}
triggerResize();
requestAnimationFrame(triggerResize);
setTimeout(triggerResize, 100);
setTimeout(triggerResize, 500);
window.addEventListener('orientationchange', triggerResize);
