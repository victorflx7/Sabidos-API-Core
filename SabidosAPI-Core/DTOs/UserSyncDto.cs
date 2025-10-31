using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    public class UserSyncDto
    {
        [Required]
        public string FirebaseUid { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Name { get; set; }
    }
}