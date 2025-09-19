using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Models;

public class User
{
    [Key]
    public int Id { get; set; }  

    [Required]
    [MaxLength(160)]
    public string FirebaseUid { get; set; } = string.Empty; 

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(160)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
