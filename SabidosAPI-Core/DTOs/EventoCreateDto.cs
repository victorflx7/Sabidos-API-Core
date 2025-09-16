using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class EventoCreateDto
    {

        [Required]
        [StringLength(160)]
        public string? TitleEvent { get; set; } = string.Empty;

        [Required]
        public DateTime? DataEvento { get; set; }

        [Required]
        [MaxLength(160)]
        public string AuthorUid { get; set; } = string.Empty;
    }
}