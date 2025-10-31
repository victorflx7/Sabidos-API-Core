namespace SabidosAPI_Core.DTOs
{
   public class UserSyncDto
    {
        public string FirebaseUid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
}
