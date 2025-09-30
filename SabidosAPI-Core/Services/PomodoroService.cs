using SabidosAPI_Core.Models;
using SabidosAPI_Core.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SabidosAPI_Core.Data;

namespace SabidosAPI_Core.Services
{
    public class PomodoroService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PomodoroService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PomoResponseDto>> GetAllAsync(string? userUId = null)
        {
            var query = _context.Pomodoros.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(userUId))
                query = query.Where(p => p.AuthorUid == userUId);

            var pomodoros = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return _mapper.Map<List<PomoResponseDto>>(pomodoros);
        }

        public async Task<int> CountTimeAsync(string authorUid)
        {
            return await _context.Pomodoros
                .Where(p => p.AuthorUid == authorUid)
                .SumAsync(p => p.Duration);
        }

        public async Task<PomoResponseDto> CreateAsync(PomoCreateDto dto, string authorUid)
        {
            var pomodoro = _mapper.Map<Pomodoro>(dto);
            _context.Pomodoros.Add(pomodoro);
            pomodoro.AuthorUid = authorUid;
            pomodoro.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<PomoResponseDto>(pomodoro);
        }
    }
}