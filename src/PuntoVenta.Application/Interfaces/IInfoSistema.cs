namespace PuntoVenta.Application.Interfaces;

public interface IInfoSistema
{
    string BackendVersion { get; }
    string? BackendCommitSha { get; }
    string Ambiente { get; }
}
