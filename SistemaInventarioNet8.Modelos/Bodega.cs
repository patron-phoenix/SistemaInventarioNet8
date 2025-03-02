using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioNet8.Modelos
{
    public class Bodega
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Nombre es requerido")]
        [MaxLength(60,ErrorMessage ="Nombre debe tener máximo 60 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Descripción es requerido")]
        [MaxLength(100, ErrorMessage = "Descripción  debe tener máximo 100 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Estado es requerido")]
        public bool Estado { get; set; }
    }
}
