using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.DTOs
{
    // DTOs para requests com Firebase UID
    public class EventoCreateRequestDto
    {
        [Required]
        public string FirebaseUid { get; set; } = string.Empty;

        [Required]
        public EventoCreateDto EventoData { get; set; } = new();
    }

    public class EventoUpdateRequestDto
    {
        [Required]
        public string FirebaseUid { get; set; } = string.Empty;

        [Required]
        public EventoUpdateDto EventoData { get; set; } = new();
    }

    public class UserRequestDto
    {
        [Required]
        public string FirebaseUid { get; set; } = string.Empty;
    }

    public class EventoListRequestDto
    {
        public string? FirebaseUid { get; set; }
    }

}
