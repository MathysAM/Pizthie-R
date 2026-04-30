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

// DIAGNOSTIC iOS standalone - affiche les dimensions reelles
if (window.navigator.standalone) {
    setTimeout(function () {
        const out = document.getElementById('out');
        const rect = out ? out.getBoundingClientRect() : null;
        const canvas = document.querySelector('canvas');
        const info = [
            'innerWidth=' + window.innerWidth,
            'innerHeight=' + window.innerHeight,
            'screen.w=' + window.screen.width,
            'screen.h=' + window.screen.height,
            'dpr=' + window.devicePixelRatio,
            'out.w=' + (rect ? Math.round(rect.width) : 'null'),
            'canvas.w=' + (canvas ? canvas.width : 'null'),
            'canvas.css=' + (canvas ? canvas.style.width : 'null'),
        ].join(' | ');

        const div = document.createElement('div');
        div.style.cssText = 'position:fixed;top:0;left:0;right:0;background:rgba(0,0,0,0.85);color:lime;font-size:10px;padding:4px;z-index:99999;word-break:break-all';
        div.textContent = info;
        document.body.appendChild(div);
    }, 2000);
}

