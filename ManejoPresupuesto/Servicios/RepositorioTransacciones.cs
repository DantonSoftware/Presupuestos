using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"Select t.Id, t.Monto, t.FechaTransaccion, c.nombre as Categoria,
                    cu.Nombre as Cuenta, c.TipoOperacionId
                    From Transacciones t
                    Inner Join Categorias c
                    On c.Id = t.CategoriaId
                    Inner Join Cuentas cu
                    On cu.Id = t.CuentaId
                    Where t.CuentaId = @CuentaId And t.UsuarioId = @UsuarioId
                    And FechaTransaccion Between @FechaInicio And @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"Select t.Id, t.Monto, t.FechaTransaccion, c.nombre as Categoria,
                    cu.Nombre as Cuenta, c.TipoOperacionId
                    From Transacciones t
                    Inner Join Categorias c
                    On c.Id = t.CategoriaId
                    Inner Join Cuentas cu
                    On cu.Id = t.CuentaId
                    Where t.UsuarioId = @UsuarioId
                    And FechaTransaccion Between @FechaInicio And @FechaFin
                    Order by t.FechaTransaccion Desc", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("TransaccionesActualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"Select Transacciones.*, cat.TipoOperacionId
                From Transacciones
                Inner join Categorias cat
                On cat.Id = Transacciones.CategoriaId
                Where Transacciones.Id = @Id And Transacciones.UsuarioId = @UsuarioId",
                new { id, usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                                                            Select datediff(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana, Sum(Monto) as Monto, cat.TipoOperacionId
                                                            from Transacciones
                                                            Inner join Categorias cat on cat.Id = Transacciones.CategoriaId
                                                            Where Transacciones.UsuarioId = @usuarioId And FechaTransaccion Between @fechaInicio and @fechafin
                                                            group by datediff(d, @fechaInicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
                                                                    }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"select MONTH(FechaTransaccion) as Mes,
                        Sum(Monto) as Monto, cat.TipoOperacionId
                        from Transacciones
                        inner join Categorias cat
                        on cat.Id = Transacciones.CategoriaId
                        Where Transacciones.UsuarioId = @usuarioId and Year(FechaTransaccion) = @Año
                        group by Month(FechaTransaccion), cat.TipoOperacionId", new { usuarioId, año });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }

    }
}
