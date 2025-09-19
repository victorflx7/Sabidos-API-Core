namespace SabidosAPI_Core.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
