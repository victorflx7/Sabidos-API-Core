using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class FlashcardCreateUpdateDto
    {
        [Required]
        [MaxLength(160)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string Frente { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string Verso { get; set; } = string.Empty;
        
        // AuthorUid e AuthorName removidos: Serão definidos pelo Controller/Service 
        // com base no usuário autenticado (melhor prática de segurança).
    }
}