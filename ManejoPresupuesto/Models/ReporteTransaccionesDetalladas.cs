﻿namespace ManejoPresupuesto.Models
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }
        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(x => x.BalanceDepositos);
        public decimal BalanceRetiros => TransaccionesAgrupadas.Sum(x => x.balanceRetiros);
        public decimal Total => BalanceDepositos - BalanceRetiros;

        public class TransaccionesPorFecha
        {
            public DateTime FechaTransaccion { get; set; }
            public IEnumerable<Transaccion> Transacciones { get; set; }
            public decimal BalanceDepositos => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Sum(x => x.Monto);

            public decimal balanceRetiros => Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Sum(x => x.Monto);
        }
    }
}
