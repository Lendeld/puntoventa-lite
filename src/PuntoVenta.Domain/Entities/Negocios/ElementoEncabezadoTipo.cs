namespace PuntoVenta.Domain.Entities.Negocios;

// Tipos de elemento del encabezado ordenable del ticket térmico.
// Los 7 primeros son fijos (siempre presentes, solo se ocultan/reordenan).
// Texto es el único agregable/borrable y lleva TextoLibre.
public enum ElementoEncabezadoTipo
{
    Nombre = 0,
    NombreComercial = 1,
    Correo = 2,
    Telefono = 3,
    Direccion = 4,
    IdentificacionFiscal = 5,
    Fecha = 6,
    Texto = 7
}
