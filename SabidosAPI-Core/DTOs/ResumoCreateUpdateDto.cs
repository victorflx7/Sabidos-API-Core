
using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class ResumoCreateUpdateDto
    {
        [Required]
        [MaxLength(160)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string Conteudo { get; set; } = string.Empty;

    
    }
}       