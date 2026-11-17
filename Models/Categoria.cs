namespace Control_de_Gastos.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int? UserId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}