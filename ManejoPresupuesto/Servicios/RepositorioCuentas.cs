using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"insert into Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                                                        values (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                                                        select SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"select c.Id, c.Nombre, c.Balance, tc.Nombre as TipoCuenta 
                                                        from Cuentas c
                                                        inner join TiposCuentas tc on tc.Id = c.TipoCuentaId
                                                        where tc.UsuarioId = @UsuarioId
                                                        order by tc.Orden", new {usuarioId});
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"select c.Id, c.Nombre, c.Balance, c.descripcion, TipoCuentaId 
                    from Cuentas c
                    inner join TiposCuentas tc on tc.Id = c.TipoCuentaId
                    where tc.UsuarioId = @UsuarioId and c.Id = @Id", new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Update Cuentas
                                            Set Nombre = @nombre, Balance = @Balance, Descripcion = @Descripcion,
                                            TipoCuentaId = @TipoCuentaId
                                            Where Id = @Id;", cuenta);

        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Delete Cuentas Where Id = @Id", new { id });
        }

    }
}
