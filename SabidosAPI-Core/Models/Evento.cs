// Models/Evento.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabidosAPI_Core.Models;

public class Evento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string TitleEvent { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DescriptionEvent { get; set; }

    [Required]
    public DateTime DataEvento { get; set; }

    [StringLength(100)]
    public string? LocalEvento { get; set; }

    public bool IsCompleted { get; set; } = false;

    // 🔗 Relacionamento com User
    [Required]
    [StringLength(160)]
    public string AuthorUid { get; set; } = string.Empty;

    public virtual User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}