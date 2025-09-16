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

        public async Task<List<EventoResponseDto>> GetAllEventosAsync(string? authorUid  = null)
        {
            var query = _context.Eventos.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(authorUid ))
                query = query.Where(e => e.AuthorUid == authorUid );

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
    }
}
