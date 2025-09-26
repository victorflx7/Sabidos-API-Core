using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Models
{
    public class Pomodoro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Ciclos { get; set; }

        [Required]
        public int Duration { get; set; } 

        [Required]
        public int TempoTrabalho { get; set; } 

        [Required]
        public int TempoDescanso { get; set; } 
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int Userid { get; set; }

        [Required]
        [MaxLength(160)]
        public string AuthorUid { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}