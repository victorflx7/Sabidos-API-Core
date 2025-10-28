using SabidosAPI_Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SabidosAPI_Core.Services
{
    public interface IEventoService
    {
        Task<List<EventoResponseDto>> GetAllEventosAsync(string? authorUid = null);
        Task<EventoResponseDto?> GetEventosByIdAsync(int id);
        Task<int> GetEventosCountByUserAsync(string authorUid);
        Task<EventoResponseDto> CreateEventoAsync(EventoResponseDto eventoDto, string authorUid);
        Task<EventoResponseDto?> UpdateEventoAsync(int id, EventoResponseDto eventoDto);
        Task<bool> DeleteEventoAsync(int id);
    }
}