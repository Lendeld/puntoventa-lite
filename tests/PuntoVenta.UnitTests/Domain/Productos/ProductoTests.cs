using PuntoVenta.Domain.Entities.Productos;

namespace PuntoVenta.UnitTests.Domain.Productos;

public class ProductoTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private const string CodigoValido = "P001";
    private const string NombreValido = "Producto de Prueba";
    private const TipoItem TipoItemValido = TipoItem.Bien;
    private const decimal PrecioValido = 1000m;

    // ──────────────────────────────────────────────
    // Casos exitosos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarProducto_CuandoDatosMinimosValidos()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido);

        Assert.False(resultado.IsError);
        var producto = resultado.Value;
        Assert.Equal(CodigoValido, producto.Codigo);
        Assert.Equal(NombreValido, producto.Nombre);
        Assert.Equal(TipoItemValido, producto.TipoItem);
        Assert.Equal(PrecioValido, producto.PrecioUnitario);
        Assert.Null(producto.CodigoBarras);
        Assert.Null(producto.Descripcion);
        Assert.Null(producto.ImagenUrl);
        Assert.Null(producto.PrecioCosto);
        Assert.Null(producto.CategoriaId);
        Assert.Null(producto.TarifaIvaImpuestoCodigo);
        Assert.False(producto.NoAplicaExistencias);
        Assert.False(producto.PermiteModificarPrecioUnitario);
        Assert.NotEqual(Guid.Empty, producto.Id);
    }

    [Fact]
    public void Crear_DebeRetornarProducto_CuandoServicioSinExistencias()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItem.Servicio, PrecioValido);

        Assert.False(resultado.IsError);
        Assert.Equal(TipoItem.Servicio, resultado.Value.TipoItem);
    }

    [Fact]
    public void Crear_DebeRetornarProducto_CuandoPrecioEsCero()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, 0m);

        Assert.False(resultado.IsError);
        Assert.Equal(0m, resultado.Value.PrecioUnitario);
    }

    // ──────────────────────────────────────────────
    // Trim de campos
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeTrimearCodigo()
    {
        var resultado = Producto.Crear("  P001  ", NombreValido, TipoItemValido, PrecioValido);

        Assert.False(resultado.IsError);
        Assert.Equal("P001", resultado.Value.Codigo);
    }

    [Fact]
    public void Crear_DebeTrimearNombre()
    {
        var resultado = Producto.Crear(CodigoValido, "  Producto  ", TipoItemValido, PrecioValido);

        Assert.False(resultado.IsError);
        Assert.Equal("Producto", resultado.Value.Nombre);
    }

    [Fact]
    public void Crear_DebeTrimearCodigoBarras()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido, codigoBarras: "  123456  ");

        Assert.False(resultado.IsError);
        Assert.Equal("123456", resultado.Value.CodigoBarras);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Codigo
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoCodigoVacioOEspacios(string codigo)
    {
        var resultado = Producto.Crear(codigo, NombreValido, TipoItemValido, PrecioValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoCodigoExcedeLongitudMaxima()
    {
        var codigoLargo = new string('A', Producto.CodigoMaxLength + 1);

        var resultado = Producto.Crear(codigoLargo, NombreValido, TipoItemValido, PrecioValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Nombre
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRetornarError_CuandoNombreVacioOEspacios(string nombre)
    {
        var resultado = Producto.Crear(CodigoValido, nombre, TipoItemValido, PrecioValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.NombreRequerido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoNombreExcedeLongitudMaxima()
    {
        var nombreLargo = new string('a', Producto.NombreMaxLength + 1);

        var resultado = Producto.Crear(CodigoValido, nombreLargo, TipoItemValido, PrecioValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.NombreExcedeLongitud.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — Precio
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoPrecioNegativo()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, -1m);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.PrecioUnitarioInvalido.Code);
    }

    [Fact]
    public void Crear_DebeRetornarError_CuandoPrecioCostoNegativo()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido, precioCosto: -1m);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.PrecioCostoInvalido.Code);
    }

    // ──────────────────────────────────────────────
    // Validaciones — regla de negocio
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarError_CuandoServicioConNoAplicaExistencias()
    {
        var resultado = Producto.Crear(CodigoValido, NombreValido, TipoItem.Servicio, PrecioValido, noAplicaExistencias: true);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.NoAplicaExistenciasSoloBien.Code);
    }

    // ──────────────────────────────────────────────
    // Acumulación de múltiples errores
    // ──────────────────────────────────────────────

    [Fact]
    public void Crear_DebeRetornarMultiplesErrores_CuandoVariosCamposInvalidos()
    {
        var resultado = Producto.Crear(string.Empty, string.Empty, TipoItemValido, -10m);

        Assert.True(resultado.IsError);
        Assert.True(resultado.Errors.Count >= 3);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.NombreRequerido.Code);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.PrecioUnitarioInvalido.Code);
    }

    // ──────────────────────────────────────────────
    // ValidarStockDisponible
    // ──────────────────────────────────────────────

    [Fact]
    public void ValidarStockDisponible_RetornaSuccess_CuandoFlagNoAplicaExistenciasActivo()
    {
        // NoAplicaExistencias=true: siempre Success aunque cantidad > existencia (existencia 0)
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItem.Bien, PrecioValido,
            noAplicaExistencias: true).Value;

        var resultado = producto.ValidarStockDisponible(999m);

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void ValidarStockDisponible_RetornaSuccess_CuandoCantidadMenorQueExistencia()
    {
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido).Value;
        producto.AplicarMovimientoStock(10m); // existencia = 10

        var resultado = producto.ValidarStockDisponible(9m);

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void ValidarStockDisponible_RetornaSuccess_CuandoCantidadIgualAExistencia()
    {
        // cantidad == existencia debe permitirse (queda en 0)
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido).Value;
        producto.AplicarMovimientoStock(5m); // existencia = 5

        var resultado = producto.ValidarStockDisponible(5m);

        Assert.False(resultado.IsError);
    }

    [Fact]
    public void ValidarStockDisponible_RetornaError_CuandoCantidadMayorQueExistencia()
    {
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido).Value;
        producto.AplicarMovimientoStock(5m); // existencia = 5

        var resultado = producto.ValidarStockDisponible(6m);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code.StartsWith("Producto_StockInsuficiente_"));
        Assert.Contains(resultado.Errors, e => e.Description.Contains(CodigoValido));
        // Stock insuficiente es condición contemplada → advertencia, no error (se muestra como warning en UI)
        Assert.Equal("warning", resultado.Errors[0].Metadata?["severity"]);
    }

    // ──────────────────────────────────────────────
    // Actualizar
    // ──────────────────────────────────────────────

    [Fact]
    public void Actualizar_DebeModificarCampos_CuandoDatosValidos()
    {
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido).Value;

        var resultado = producto.Actualizar("P002", "  Nuevo Nombre  ", TipoItem.Servicio, 2000m);

        Assert.False(resultado.IsError);
        Assert.Equal("P002", producto.Codigo);
        Assert.Equal("Nuevo Nombre", producto.Nombre);
        Assert.Equal(TipoItem.Servicio, producto.TipoItem);
        Assert.Equal(2000m, producto.PrecioUnitario);
    }

    [Fact]
    public void Actualizar_DebeRetornarError_CuandoCodigoVacio()
    {
        var producto = Producto.Crear(CodigoValido, NombreValido, TipoItemValido, PrecioValido).Value;

        var resultado = producto.Actualizar(string.Empty, NombreValido, TipoItemValido, PrecioValido);

        Assert.True(resultado.IsError);
        Assert.Contains(resultado.Errors, e => e.Code == ProductoErrors.CodigoRequerido.Code);
    }
}
