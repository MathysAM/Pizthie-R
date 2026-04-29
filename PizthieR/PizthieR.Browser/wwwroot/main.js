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
// remesure le viewport (sans manipuler directement le canvas).
function triggerResize() {
    window.dispatchEvent(new Event('resize'));
}
triggerResize();
requestAnimationFrame(triggerResize);
setTimeout(triggerResize, 100);
setTimeout(triggerResize, 500);
window.addEventListener('orientationchange', triggerResize);

// =====================================================================
// ?? OVERLAY DIAGNOSTIC (TEMPORAIRE)
// Affiche les dimensions du viewport directement à l'écran.
// À retirer une fois la bande noire diagnostiquée.
// =====================================================================
(function showDiagnosticOverlay() {
    const box = document.createElement('div');
    box.id = 'diag-overlay';
    box.style.cssText = [
        'position:fixed',
        'top:env(safe-area-inset-top, 8px)',
        'right:8px',
        'z-index:2147483647',
        'background:rgba(255,255,0,0.95)',
        'color:#000',
        'font:11px/1.3 monospace',
        'padding:6px 8px',
        'border:1px solid #000',
        'border-radius:6px',
        'max-width:55vw',
        'pointer-events:auto',
        'white-space:pre'
    ].join(';');
    box.textContent = 'diag…';
    box.addEventListener('click', () => box.remove()); // tap pour fermer

    function update() {
        const out = document.getElementById('out');
        const cv  = document.querySelector('canvas');
        const cvR = cv ? cv.getBoundingClientRect() : null;
        const outR = out ? out.getBoundingClientRect() : null;
        box.textContent =
            'inner: ' + innerWidth + '×' + innerHeight + '\n' +
            'docCli: ' + document.documentElement.clientWidth + '×' + document.documentElement.clientHeight + '\n' +
            'screen: ' + screen.width + '×' + screen.height + '\n' +
            'dpr:    ' + devicePixelRatio + '\n' +
            'standalone: ' + (window.navigator.standalone === true) + '\n' +
            '#out:   ' + (outR ? Math.round(outR.width) + '×' + Math.round(outR.height) : 'n/a') + '\n' +
            'canvas#: ' + document.querySelectorAll('canvas').length + '\n' +
            (cv ? 'cvAttr: ' + cv.width + '×' + cv.height + '\n' : '') +
            (cvR ? 'cvRect: ' + Math.round(cvR.width) + '×' + Math.round(cvR.height) +
                   ' @' + Math.round(cvR.left) + ',' + Math.round(cvR.top) : '');
    }
    document.body.appendChild(box);
    update();
    setInterval(update, 500);
    window.addEventListener('resize', update);
    window.addEventListener('orientationchange', update);
})();
