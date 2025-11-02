// Services/EventoService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;

namespace SabidosAPI_Core.Services
{
    public class EventoService : IEventoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EventoService> _logger;

        public EventoService(AppDbContext context, IMapper mapper, ILogger<EventoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<EventoResponseDto>> GetAllEventosAsync(string? authorUid = null)
        {
            try
            {
                var query = _context.Eventos
                    .Include(p => p.User)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(authorUid))
                    query = query.Where(e => e.AuthorUid == authorUid);

                var eventos = await query
                    .OrderByDescending(p => p.DataEvento)
                    .ToListAsync();

                return _mapper.Map<List<EventoResponseDto>>(eventos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos");
                throw;
            }
        }

        public async Task<EventoResponseDto?> GetEventoByIdAsync(int id)
        {
            try
            {
                var evento = await _context.Eventos
                    .Include(p => p.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                return evento == null ? null : _mapper.Map<EventoResponseDto>(evento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar evento por ID: {EventoId}", id);
                throw;
            }
        }

        public async Task<int> GetEventosCountByUserAsync(string authorUid)
        {
            try
            {
                return await _context.Eventos
                    .CountAsync(e => e.AuthorUid == authorUid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar eventos do usuário: {FirebaseUid}", authorUid);
                throw;
            }
        }

        public async Task<EventoResponseDto> CreateEventoAsync(EventoCreateDto eventoDto, string authorUid)
        {
            if (string.IsNullOrEmpty(authorUid))
                throw new ArgumentException("Firebase UID do autor é obrigatório");

            try
            {
                var evento = _mapper.Map<Evento>(eventoDto);
                evento.AuthorUid = authorUid;
                evento.CreatedAt = DateTime.UtcNow;
                evento.IsCompleted = false;

                _context.Eventos.Add(evento);
                await _context.SaveChangesAsync();

                // Recarrega com dados do autor
                await _context.Entry(evento)
                    .Reference(p => p.User)
                    .LoadAsync();

                _logger.LogInformation("Evento criado com sucesso: {EventoId} para usuário: {FirebaseUid}", evento.Id, authorUid);
                return _mapper.Map<EventoResponseDto>(evento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar evento para usuário: {FirebaseUid}", authorUid);
                throw;
            }
        }

        public async Task<EventoResponseDto?> UpdateEventoAsync(int id, EventoUpdateDto eventoDto, string userFirebaseUid)
        {
            try
            {
                var existingEvento = await _context.Eventos
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingEvento == null) return null;

                // 🔐 Verifica se o usuário é o autor
                if (existingEvento.AuthorUid != userFirebaseUid)
                {
                    _logger.LogWarning("Tentativa de atualização não autorizada. Evento: {EventoId}, Usuário: {FirebaseUid}", id, userFirebaseUid);
                    throw new UnauthorizedAccessException("Você não tem permissão para editar este evento");
                }

                // Aplica as atualizações
                _mapper.Map(eventoDto, existingEvento);
                existingEvento.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento atualizado com sucesso: {EventoId}", id);
                return _mapper.Map<EventoResponseDto>(existingEvento);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar evento: {EventoId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteEventoAsync(int id, string userFirebaseUid)
        {
            try
            {
                var evento = await _context.Eventos
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (evento == null) return false;

                // 🔐 Verifica se o usuário é o autor
                if (evento.AuthorUid != userFirebaseUid)
                {
                    _logger.LogWarning("Tentativa de exclusão não autorizada. Evento: {EventoId}, Usuário: {FirebaseUid}", id, userFirebaseUid);
                    throw new UnauthorizedAccessException("Você não tem permissão para excluir este evento");
                }

                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento excluído com sucesso: {EventoId}", id);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar evento: {EventoId}", id);
                throw;
            }
        }

        public async Task<bool> EventoBelongsToUserAsync(int eventoId, string userFirebaseUid)
        {
            try
            {
                return await _context.Eventos
                    .AnyAsync(p => p.Id == eventoId && p.AuthorUid == userFirebaseUid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar propriedade do evento: {EventoId}", eventoId);
                throw;
            }
        }

        // Métodos adicionais úteis
        public async Task<List<EventoResponseDto>> GetEventosByDateRangeAsync(DateTime startDate, DateTime endDate, string? authorUid = null)
        {
            try
            {
                var query = _context.Eventos
                    .Include(p => p.User)
                    .Where(e => e.DataEvento >= startDate && e.DataEvento <= endDate);

                if (!string.IsNullOrEmpty(authorUid))
                    query = query.Where(e => e.AuthorUid == authorUid);

                var eventos = await query
                    .OrderBy(e => e.DataEvento)
                    .ToListAsync();

                return _mapper.Map<List<EventoResponseDto>>(eventos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos por range de data");
                throw;
            }
        }

        public async Task<List<EventoResponseDto>> GetUpcomingEventosAsync(int days = 7, string? authorUid = null)
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(days);

            return await GetEventosByDateRangeAsync(startDate, endDate, authorUid);
        }
    }
}
//public async Task<List<EventoResponseDto>> GetRecentEventosAsync(int count)
//{
//    var eventos = await _context.Eventos
//        .Include(p => p.User)
//        .OrderByDescending(p => p.DataEvento)
//        .Take(count)
//        .ToListAsync();
//    return _mapper.Map<List<EventoResponseDto>>(eventos);
//}
//public async Task<DateTime?> GetLatestEventoDateAsync()
//{
//    var latestEvento = await _context.Eventos
//        .OrderByDescending(e => e.DataEvento)
//        .FirstOrDefaultAsync();
//    return latestEvento?.DataEvento;
//}