using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class FlashcardResponseDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(160)]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        [StringLength(8000)]
        public string  Frente { get; set; } = string.Empty;
        [StringLength(8000)]
        public string Verso { get; set; } = string.Empty;
        [Required]
        public string AuthorUid { get; set; } = string.Empty;
        [Required]
        public string AuthorName { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
