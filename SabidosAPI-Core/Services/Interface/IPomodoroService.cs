using SabidosAPI_Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SabidosAPI_Core.Services
{
    public interface IPomodoroService
    {
        Task<List<PomoResponseDto>> GetAllAsync(string? userUId = null);
        Task<int> CountTimeAsync(string authorUid);
        Task<PomoResponseDto> CreateAsync(PomoCreateDto dto, string authorUid);
        Task<int> GetTotalDurationByUserAsync(string authorUid);
        //Task<List<PomoResponseDto>> GetRecentPomodorosAsync(string authorUid, int days = 7);
        //Task<Dictionary<DateTime, int>> GetPomodoroStatsByDateAsync(string authorUid, DateTime startDate, DateTime endDate);
    }
}
