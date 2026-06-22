using PuntoVenta.Domain.Common.Events;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Domain.Entities;

public abstract class BaseAuditableEntity : Entity
{
    private readonly List<IDomainEvent> _eventosDominio = [];

    protected BaseAuditableEntity() { }

    protected BaseAuditableEntity(Guid id) : base(id) { }

    public bool Activo { get; private set; } = true;

    public Guid? UsuarioCreacionId { get; internal set; }

    public Usuario? UsuarioCreacion { get; private set; }

    public DateTime FechaCreacion { get; internal set; }

    public Guid? UsuarioModificacionId { get; internal set; }

    public Usuario? UsuarioModificacion { get; private set; }

    public DateTime? FechaModificacion { get; internal set; }

    internal IReadOnlyList<IDomainEvent> EventosDominio => _eventosDominio;

    protected void RegistrarEvento(IDomainEvent evento) => _eventosDominio.Add(evento);

    internal void LimpiarEventos() => _eventosDominio.Clear();

    public void Desactivar() => Activo = false;

    public void Activar() => Activo = true;
}
