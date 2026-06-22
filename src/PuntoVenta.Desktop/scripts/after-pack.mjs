// electron-builder afterPack hook. Renombra _modules -> node_modules
// dentro de la .app/bundle ANTES de firmar y empaquetar .dmg.
//
// Workaround: electron-builder filtra `node_modules` de extraResources
// incluso con filter explicito. Solucion conocida: stageamos como
// "_modules", electron-builder lo copia sin filtrar, y aqui restauramos
// el nombre original para que `require()` lo encuentre.
import fs from "node:fs";
import path from "node:path";

export default async function afterPack(context) {
    const resourcesDir = context.electronPlatformName === "darwin"
        ? path.join(context.appOutDir, `${context.packager.appInfo.productFilename}.app`, "Contents", "Resources")
        : path.join(context.appOutDir, "resources");

    const stagedName = path.join(resourcesDir, "standalone", "_modules");
    const finalName = path.join(resourcesDir, "standalone", "node_modules");

    if (fs.existsSync(stagedName)) {
        if (fs.existsSync(finalName)) fs.rmSync(finalName, { recursive: true, force: true });
        fs.renameSync(stagedName, finalName);
        console.log(`afterPack: ${stagedName} -> ${finalName}`);
    } else {
        console.warn(`afterPack: no encontre ${stagedName}, no hago nada`);
    }
}
