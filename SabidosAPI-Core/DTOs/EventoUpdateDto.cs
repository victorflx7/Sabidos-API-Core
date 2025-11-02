using System.ComponentModel.DataAnnotations;

public class EventoUpdateDto
{
    [MaxLength(200)]
    public string? TitleEvent { get; set; }

    [MaxLength(500)]
    public string? DescriptionEvent { get; set; }

    public DateTime? DataEvento { get; set; }

    [MaxLength(100)]
    public string? LocalEvento { get; set; }

    public bool? IsCompleted { get; set; }
}