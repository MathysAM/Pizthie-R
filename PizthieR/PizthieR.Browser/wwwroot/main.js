import { dotnet } from './_framework/dotnet.js';

const is_browser = typeof window !== 'undefined';
if (!is_browser) throw new Error('Expected to be running in a browser');

const runtime = await dotnet
    .withDiagnosticTracing(true)
    .withApplicationArgumentsFromQuery()
    .create();

const config = runtime.getConfig();

// Lance Avalonia
await runtime.runMain(config.mainAssemblyName, [globalThis.location.href]);

// Supprime le splash personnalisé
const splash = document.getElementById('splash');
if (splash) splash.remove();

// Supprime au cas où le splash Avalonia par défaut serait injecté
const injected = document.querySelector('.avalonia-splash');
if (injected) injected.remove();
