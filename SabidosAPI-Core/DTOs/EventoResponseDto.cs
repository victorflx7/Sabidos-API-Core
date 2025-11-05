using System.ComponentModel.DataAnnotations;


namespace SabidosAPI_Core.DTOs
{
    public class EventoResponseDto
    {
        public int Id { get; set; }
        public string TitleEvent { get; set; } = string.Empty;
        public string? DescriptionEvent { get; set; }
        public DateTime DataEvento { get; set; }
        public string? LocalEvento { get; set; }
        public bool IsCompleted { get; set; }
        public string AuthorUid { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
