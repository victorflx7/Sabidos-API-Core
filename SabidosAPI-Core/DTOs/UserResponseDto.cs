using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs;

public class UserResponseDto
{
    public string FirebaseUid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
}