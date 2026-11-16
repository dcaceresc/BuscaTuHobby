// start-dev.js
// Levanta el servidor de desarrollo de Angular con SSL usando el certificado de desarrollo de ASP.NET Core.

'use strict';

const path      = require('path');
const { spawn } = require('child_process');
const { name: APP_NAME } = require('./package.json');

// ── Resuelve las rutas del certificado ─────────────────────────────────────

const CERT_BASE_FOLDER =
  process.env.APPDATA
    ? path.join(process.env.APPDATA, 'ASP.NET', 'https')
    : path.join(process.env.HOME,    '.aspnet',  'https');

const CERT_FILE = path.join(CERT_BASE_FOLDER, `${APP_NAME}.pem`);
const KEY_FILE  = path.join(CERT_BASE_FOLDER, `${APP_NAME}.key`);

// ── Resuelve el punto de entrada de @angular/cli directamente ───────────────
//   Evita todos los envoltorios .cmd/.ps1: llama a node con el archivo JS directamente.
//   Funciona en todas las plataformas y en todas las versiones de Node sin shell:true.

const NG_JS = path.resolve(
  __dirname,
  'node_modules',
  '@angular',
  'cli',
  'bin',
  'ng.js'
);

// ── Lanza ng serve ─────────────────────────────────────────────────────────

const child = spawn(
  process.execPath,           // el binario de node exacto que ejecuta este script
  [
    NG_JS,
    'serve',
    '--ssl',
    '--ssl-cert', CERT_FILE,
    '--ssl-key',  KEY_FILE,
    '--host',     '127.0.0.1',
  ],
  {
    stdio: 'inherit',
  }
);

child.on('error', err => {
  console.error('[start-dev] Failed to start ng serve:', err.message);
  console.error('[start-dev] Tried to run:', NG_JS);
  process.exit(1);
});

child.on('exit', code => process.exit(code ?? 1));

