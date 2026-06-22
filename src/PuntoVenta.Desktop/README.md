# PuntoVenta Desktop (Electron)

Shell de escritorio basado en Electron que empaqueta el frontend Next.js de `src/PuntoVenta.Web` y lo corre localmente. Reusa el web **tal cual** — server actions, iron-session, RSC y server components siguen funcionando porque Electron arranca un Next server (`standalone`) como child Node process.

El backend .NET corre como child process con SQLite local (`userData/puntoventa.db`). EF Core migra la DB al arrancar — sin servidor externo ni configuracion adicional.

## Estructura

```
PuntoVenta.Desktop/
├── package.json
├── tsconfig.json
├── electron-builder.yml
├── resources/             # iconos del bundle (PNG, ICO)
├── src/
│   ├── main.ts            # Electron main: BrowserWindow + arranque del API + Next server
│   ├── apiServer.ts       # spawn del .NET API con SQLite
│   └── nextServer.ts      # spawn del Next standalone + port discovery
├── scripts/
│   ├── publish-api.mjs    # publica el .NET API self-contained
│   ├── stage-next.mjs     # prepara standalone-staged/ para electron-builder
│   ├── clean-api.mjs      # borra estado local (DB, secrets, logs)
│   ├── smoke.mjs          # smoke test post-pack
│   └── after-pack.mjs     # hook electron-builder: restaura _modules -> node_modules
├── dist/                  # output del tsc (gitignored)
└── release/               # output de electron-builder (gitignored)
```

## Requisitos

- Node 22+, pnpm 11+
- .NET SDK 10 (para build:api / publish:api)
- Plataforma-especificos: ninguno (Electron incluye Chromium + Node; SQLite no requiere instalacion)

## Uso

### Modo dev

```bash
# Terminal 1 — Electron (lanza el .NET API y next dev juntos)
cd src/PuntoVenta.Desktop
pnpm install   # primera vez
pnpm dev
```

O tres terminales separadas si preferis control granular:

```bash
# Terminal 1 — backend .NET
dotnet run --project src/PuntoVenta.API

# Terminal 2 — frontend Next dev server
cd src/PuntoVenta.Web && pnpm dev

# Terminal 3 — Electron apuntando al dev server
cd src/PuntoVenta.Desktop
PV_SKIP_LOCAL_API=1 PV_SKIP_NEXT_SPAWN=1 pnpm dev
```

### Build productivo

```bash
cd src/PuntoVenta.Desktop
pnpm install
pnpm dist:mac   # → release/PuntoVentaLite-x.y.z-arm64.dmg
pnpm dist:win   # → release/PuntoVentaLite-x.y.z-x64.exe (NSIS)
```

El build hace:

1. `pnpm publish:api` — publica el .NET API self-contained en `resources/api/`
2. `pnpm build:next` con `DESKTOP_BUILD=true` → genera `.next/standalone/`
3. `tsc` del main de Electron
4. `scripts/stage-next.mjs` — copia standalone a `standalone-staged/` y renombra `node_modules` a `_modules`
5. `electron-builder` empaqueta Chromium + Node + standalone como instalador

### Instalador de Windows — dos maneras

El RID del .NET API se controla con la env var `TARGET_RID` (precedencia:
`--target-rid` > `TARGET_RID` > RID del host). `dist:win` fuerza `win-x64`.

**Manera 1 — cross-compile desde mac/Linux** (lo que usás hoy):

```bash
pnpm dist:win
```

`dotnet publish -r win-x64 --self-contained` baja el runtime pack de Windows
por NuGet (no requiere toolchain nativa), y `electron-builder --win` arma el
NSIS con su `makensis` cross-platform. Produce el `.exe` listo para Windows.

Límites del cross-build desde mac:
- **No se puede firmar** el instalador (sin signtool/cert de Windows) — el
  usuario verá el SmartScreen de "editor desconocido".
- **No se puede correr `pnpm smoke`** sobre el bundle Windows (el binario API
  es un `.exe` PE que no ejecuta en mac). Validá el `.exe` arrancándolo en
  Windows real.

**Manera 2 — build nativo en Windows** (cuando tengas la máquina):

```bash
git clone … && cd src/PuntoVenta.Desktop
pnpm install
pnpm dist:win     # TARGET_RID=win-x64 es no-op (ya es el host)
pnpm smoke        # acá SÍ corre: el .exe del API arranca y responde /health
```

En Windows podés además firmar (configurar `win.certificateFile` /
`certificatePassword` en `electron-builder.yml`).

### Smoke test

Despues de `pnpm pack` (build --dir sin instalar):

```bash
pnpm smoke
```

Lanza el binario .NET contra una DB temporal y verifica que `/health` retorna 200.

### Limpiar estado local

```bash
pnpm clean:api
```

Borra `puntoventa.db`, secrets y logs del userData de Electron. Util para forzar
un arranque limpio sin reinstalar.

## Variables de entorno (dev)

| Variable | Efecto |
|---|---|
| `PV_SKIP_LOCAL_API=1` | No spawna el .NET API (usar API externa o en otra terminal) |
| `PV_SKIP_NEXT_SPAWN=1` | No spawna Next; carga `PV_DEV_URL` directo |
| `PV_DEV_URL` | URL a cargar cuando `PV_SKIP_NEXT_SPAWN=1` (default: `http://localhost:3000`) |
| `PV_DEBUG=1` | Abre DevTools en packaged |

## Auto-update (pendiente)

Por hacer: integrar `electron-updater` con manifest firmado.
