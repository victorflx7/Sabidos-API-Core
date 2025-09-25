using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Models
{
    public class Flashcard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(160)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string Frente { get; set; } = string.Empty;

        [Required]
        [MaxLength(8000)]
        public string Verso { get; set; } = string.Empty;

        [Required]
        [MaxLength(160)]
        public string AuthorUid { get; set; } = string.Empty;

        [Required]
        [MaxLength(160)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}