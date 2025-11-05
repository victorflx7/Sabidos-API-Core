// Services/IEventoService.cs
using SabidosAPI_Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SabidosAPI_Core.Services
{
    public interface IEventoService
    {
        // ✅ CORRETO - Métodos de consulta
        Task<List<EventoResponseDto>> GetAllEventosAsync(string? authorUid = null);
        Task<EventoResponseDto?> GetEventoByIdAsync(int id); // 🔥 CORRIGIDO: GetEventoByIdAsync
        Task<int> GetEventosCountByUserAsync(string authorUid);
        
        // ✅ CORRETO - Métodos que modificam dados (com validação de autorização)
        Task<EventoResponseDto> CreateEventoAsync(EventoCreateDto eventoDto, string authorUid); // 🔥 CORRIGIDO: EventoCreateDto
        Task<EventoResponseDto?> UpdateEventoAsync(int id, EventoUpdateDto eventoDto, string userFirebaseUid); // 🔥 CORRIGIDO: Parâmetros
        Task<bool> DeleteEventoAsync(int id, string userFirebaseUid); // 🔥 CORRIGIDO: Parâmetro de segurança

        // ✅ NOVOS - Métodos adicionais úteis
        Task<bool> EventoBelongsToUserAsync(int eventoId, string userFirebaseUid);
        Task<List<EventoResponseDto>> GetEventosByDateRangeAsync(DateTime startDate, DateTime endDate, string? authorUid = null);
        Task<List<EventoResponseDto>> GetUpcomingEventosAsync(int days = 7, string? authorUid = null);
    }
}