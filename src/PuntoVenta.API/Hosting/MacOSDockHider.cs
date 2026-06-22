using System.Runtime.InteropServices;

namespace PuntoVenta.API.Hosting;

// Oculta el proceso del Dock cuando corre como child de Electron.
// Sin esto, el binario .NET aparece como "exec" brincando en el Dock
// (macOS trata cualquier ejecutable foreground como app). LSUIElement
// en Info.plist no aplica porque Electron hace spawn() directo del
// binario, no NSWorkspace.launchApplication.
//
// Trigger: env var PUNTOVENTA_DATA_ROOT (lo setea apiServer.ts del
// host Electron). Si no esta, no hace nada -> dev local sigue igual.
internal static class MacOSDockHider
{
    // Carbon ProcessManager constants.
    private const int kProcessTransformToUIElementApplication = 4;

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessSerialNumber
    {
        public uint highLongOfPSN;
        public uint lowLongOfPSN;
    }

    public static void HideIfElectronChild()
    {
        if (!OperatingSystem.IsMacOS()) return;
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PUNTOVENTA_DATA_ROOT"))) return;
        try
        {
            // TransformProcessType: PSN {0,2} = current process. Mode 4 =
            // UIElement, remueve el icono del Dock incluso si ya aparecio.
            var psn = new ProcessSerialNumber { highLongOfPSN = 0, lowLongOfPSN = 2 };
            TransformProcessType(ref psn, kProcessTransformToUIElementApplication);
        }
        catch
        {
            // Cosmetico: si falla seguimos arrancando.
        }
    }

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int TransformProcessType(ref ProcessSerialNumber psn, int transformState);
}
