using System.ComponentModel.DataAnnotations;


namespace SabidosAPI_Core.DTOs
{
    public class EventoResponseDto
    {
        public string Id { get; set; }

        [Required]
        [StringLength(160)]
        public string TitleEvent { get; set; } = string.Empty;
        [Required]
        public DateTime DataEvento { get; set; }
    }
}
