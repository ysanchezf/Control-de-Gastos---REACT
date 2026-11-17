namespace Control_de_Gastos.Models
{
    public class Gasto
    {
        public int Id { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int? UserId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public int CategoriaId { get; set; }
    }
}