using Control_de_Gastos.Models;
namespace Control_de_Gastos.Services

{
    public interface IGastoService
    {
        decimal CalcularTotal(IEnumerable<Gasto> gastos);
    }
}