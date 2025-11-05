using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class EventoCreateDto
    {

        [Required]
        [MaxLength(200)]
        public string TitleEvent { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DescriptionEvent { get; set; }

        [Required]
        public DateTime DataEvento { get; set; }

        [MaxLength(100)]
        public string? LocalEvento { get; set; }

        [Required]
        public string AuthorUid { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;
    }
}