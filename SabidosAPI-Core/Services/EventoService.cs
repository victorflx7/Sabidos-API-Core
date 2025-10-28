using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;

namespace SabidosAPI_Core.Services
{
    public class EventoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EventoService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EventoResponseDto>> GetAllEventosAsync(string? authorUid = null)
        {
            var query = _context.Eventos.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(authorUid))
                query = query.Where(e => e.AuthorUid == authorUid);

            var eventos = await query.OrderByDescending(p => p.DataEvento).ToListAsync();
            return _mapper.Map<List<EventoResponseDto>>(eventos);
        }

        public async Task<EventoResponseDto?> GetEventosByIdAsync(int id)
        {
            var evento = await _context.Eventos
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (evento == null) return null;
            return _mapper.Map<EventoResponseDto>(evento);
        }

        public async Task<int> GetEventosCountByUserAsync(string authorUid)
        {
            return await _context.Eventos.CountAsync(e => e.AuthorUid == authorUid);
        }
        public async Task<EventoResponseDto> CreateEventoAsync(EventoResponseDto eventoDto, string authorUid)
        {
            var evento = _mapper.Map<Models.Evento>(eventoDto);
            evento.AuthorUid = authorUid;

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();
            return _mapper.Map<EventoResponseDto>(evento);
        }


        public async Task<EventoResponseDto?> UpdateEventoAsync(int id, EventoResponseDto eventoDto)
        {
            var existingEvento = await _context.Eventos.FindAsync(id);
            if (existingEvento == null) return null;
            _mapper.Map(eventoDto, existingEvento);
            await _context.SaveChangesAsync();
            return _mapper.Map<EventoResponseDto>(existingEvento);
        }


        public async Task<bool> DeleteEventoAsync(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return false;
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
            return true;
        }



        //public Task DeleteEventoAsync(int id)
        //{

        //    var evento = _context.Eventos.Find(id);
        //    if (evento != null)
        //    {
        //        _context.Eventos.Remove(evento);
        //        return _context.SaveChangesAsync();
        //    }
        //    return Task.CompletedTask;
        //}

        //public
        //    async Task<bool> EventoExistsAsync(int id)
        //{
        //    return await _context.Eventos.AnyAsync(e => e.Id == id);
        //}

        //public
        //    async Task<bool> UserExistsAsync(string firebaseUid)
        //{
        //    return await _context.Users.AnyAsync(u => u.FirebaseUid == firebaseUid);
        //}

        //public
        //    async Task<int> GetEventosCountAsync()
        //{
        //    return await _context.Eventos.CountAsync();
        //}



        //public async Task<List<EventoResponseDto>> GetRecentEventosAsync(int count)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .OrderByDescending(p => p.DataEvento)
        //        .Take(count)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosByDateRangeAsync(DateTime startDate, DateTime endDate)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.DataEvento >= startDate && e.DataEvento <= endDate)
        //        .OrderByDescending(p => p.DataEvento)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}

        //public async Task<List<EventoResponseDto>> SearchEventosByTitleAsync(string titleKeyword)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.TitleEvent.Contains(titleKeyword))
        //        .OrderByDescending(p => p.DataEvento)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}

        //public async Task<List<EventoResponseDto>> GetEventosByUserAsync(string authorUid)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.AuthorUid == authorUid)
        //        .OrderByDescending(p => p.DataEvento)
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
        //public
        //    async Task<DateTime?> GetEarliestEventoDateAsync()
        //{
        //    var earliestEvento = await _context.Eventos
        //        .OrderBy(e => e.DataEvento)
        //        .FirstOrDefaultAsync();
        //    return earliestEvento?.DataEvento;
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosPagedAsync(int pageNumber, int pageSize)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .OrderByDescending(p => p.DataEvento)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosByUserPagedAsync(string authorUid, int pageNumber, int pageSize)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.AuthorUid == authorUid)
        //        .OrderByDescending(p => p.DataEvento)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosByTitlePagedAsync(string titleKeyword, int pageNumber, int pageSize)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.TitleEvent.Contains(titleKeyword))
        //        .OrderByDescending(p => p.DataEvento)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}

        //public
        //    async Task<List<EventoResponseDto>> GetEventosByDateRangePagedAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.DataEvento >= startDate && e.DataEvento <= endDate)
        //        .OrderByDescending(p => p.DataEvento)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}

        //public
        //    async Task<int> GetEventosCountByTitleAsync(string titleKeyword)
        //{
        //    return await _context.Eventos.CountAsync(e => e.TitleEvent.Contains(titleKeyword));
        //}
        //public
        //    async Task<int> GetEventosCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        //{
        //    return await _context.Eventos.CountAsync(e => e.DataEvento >= startDate && e.DataEvento <= endDate);
        //}
        //public
        //    async Task<int> GetEventosCountByUserAndDateRangeAsync(string authorUid, DateTime startDate, DateTime endDate)
        //{
        //    return await _context.Eventos.CountAsync(e => e.AuthorUid == authorUid && e.DataEvento >= startDate && e.DataEvento <= endDate);
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosByUserAndDateRangeAsync(string authorUid, DateTime startDate, DateTime endDate)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.AuthorUid == authorUid && e.DataEvento >= startDate && e.DataEvento <= endDate)
        //        .OrderByDescending(p => p.DataEvento)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}
        //public
        //    async Task<List<EventoResponseDto>> GetEventosByUserAndTitleAsync(string authorUid, string titleKeyword)
        //{
        //    var eventos = await _context.Eventos
        //        .Include(p => p.User)
        //        .Where(e => e.AuthorUid == authorUid && e.TitleEvent.Contains(titleKeyword))
        //        .OrderByDescending(p => p.DataEvento)
        //        .ToListAsync();
        //    return _mapper.Map<List<EventoResponseDto>>(eventos);
        //}

        //public
        //    async Task<int> GetEventosCountByUserAndTitleAsync(string authorUid, string titleKeyword)
        //{
        //    return await _context.Eventos.CountAsync(e => e.AuthorUid == authorUid && e.TitleEvent.Contains(titleKeyword));
        //}
    }
}