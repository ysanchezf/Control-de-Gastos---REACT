using Control_de_Gastos.Models;
namespace Control_de_Gastos.Services

{
    public class GastoService : IGastoService
    {
        public decimal CalcularTotal(IEnumerable<Gasto> gastos)
        {
            return gastos.Sum(g => g.Monto);
        }
    }
}