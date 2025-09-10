using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(128)]
    public string FirebaseUid { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(160)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
