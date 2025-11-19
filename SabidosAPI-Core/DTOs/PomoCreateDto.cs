using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Dtos
{
    public class PomoCreateDto
    {
        [Required]
        public int Ciclos { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int TempoTrabalho { get; set; }

        [Required]
        public int TempoDescanso { get; set; }

        //[Required]
        //public int Userid { get; set; }

        //[Required]
        //[MaxLength(160)]
        //public string AuthorUid { get; set; } = string.Empty;
    }
}