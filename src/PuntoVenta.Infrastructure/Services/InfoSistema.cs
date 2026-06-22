using System.Reflection;
using Microsoft.Extensions.Hosting;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Services;

public sealed class InfoSistema : IInfoSistema, ISingletonService
{
    public InfoSistema(IHostEnvironment env)
    {
        Ambiente = env.EnvironmentName;

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        // AssemblyInformationalVersion la inyecta el release con la versión del
        // tag CalVer (dotnet publish -p:Version=AAAA.MM.DD); en dev es "0.0.0-dev"
        // (default en Directory.Build.props). AssemblyVersion queda fija — no mostrar.
        BackendVersion = ExtraerVersion(info)
            ?? assembly.GetName().Version?.ToString(3)
            ?? "desconocida";
        BackendCommitSha = ExtraerCommitSha(info);
    }

    public string BackendVersion { get; }
    public string? BackendCommitSha { get; }
    public string Ambiente { get; }

    private static string? ExtraerVersion(string? informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion)) return null;
        var idx = informationalVersion.IndexOf('+');
        return idx < 0 ? informationalVersion : informationalVersion[..idx];
    }

    private static string? ExtraerCommitSha(string? informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion)) return null;
        var idx = informationalVersion.IndexOf('+');
        if (idx < 0 || idx == informationalVersion.Length - 1) return null;
        return informationalVersion[(idx + 1)..];
    }
}
