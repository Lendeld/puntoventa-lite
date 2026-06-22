namespace PuntoVenta.Domain.Entities.Impresion;

public enum SistemaOperativoAgente
{
    Windows = 1,
    MacOS = 2,
    Linux = 3,
}

public enum ArquitecturaAgente
{
    X64 = 1,
    Arm64 = 2,
    X86 = 3,
}

public enum CanalAgente
{
    Stable = 1,
    Beta = 2,
}
