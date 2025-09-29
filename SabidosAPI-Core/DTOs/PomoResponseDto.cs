namespace SabidosAPI_Core.Dtos
{
    public class PomoResponseDto
    {
        public int Id { get; set; }
        public int Ciclos { get; set; }
        public int Duration { get; set; }
        public int TempoTrabalho { get; set; }
        public int TempoDescanso { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Userid { get; set; }
        public string AuthorUid { get; set; } = string.Empty;
    }
}   