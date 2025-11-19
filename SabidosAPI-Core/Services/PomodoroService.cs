// Services/PomodoroService.cs
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SabidosAPI_Core.Data;

namespace SabidosAPI_Core.Services
{
    public class PomodoroService : IPomodoroService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PomodoroService> _logger;

        public PomodoroService(AppDbContext context, IMapper mapper, ILogger<PomodoroService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PomoResponseDto>> GetAllAsync(string? userUId = null)
        {
            try
            {
                var query = _context.Pomodoros
                    .Include(p => p.User)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(userUId))
                    query = query.Where(p => p.AuthorUid == userUId);

                var pomodoros = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<PomoResponseDto>>(pomodoros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pomodoros");
                throw;
            }
        }

        public async Task<int> CountTimeAsync(string authorUid)
        {
            try
            {
                return await _context.Pomodoros
                    .Where(p => p.AuthorUid == authorUid)
                    .SumAsync(p => p.Duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar tempo do usuário: {FirebaseUid}", authorUid);
                throw;
            }
        }

        public async Task<PomoResponseDto> CreateAsync(PomoCreateDto dto, string authorUid)
        {
            if (string.IsNullOrEmpty(authorUid))
                throw new ArgumentException("Firebase UID do autor é obrigatório");

            try
            {
                var pomodoro = _mapper.Map<Pomodoro>(dto);
                pomodoro.AuthorUid = authorUid; // ✅ CORREÇÃO: Garantir que AuthorUid seja definido
                pomodoro.CreatedAt = DateTime.UtcNow;

                _context.Pomodoros.Add(pomodoro);
                await _context.SaveChangesAsync();

                // Recarrega com dados do autor
                await _context.Entry(pomodoro)
                    .Reference(p => p.User)
                    .LoadAsync();

                _logger.LogInformation("Pomodoro criado com sucesso para usuário: {FirebaseUid}", authorUid);
                return _mapper.Map<PomoResponseDto>(pomodoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pomodoro para usuário: {FirebaseUid}", authorUid);
                throw;
            }
        }

        public async Task<int> GetTotalDurationByUserAsync(string authorUid)
        {
            try
            {
                var total = await _context.Pomodoros
                    .Where(p => p.AuthorUid == authorUid)
                    .SumAsync(p => p.Duration);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular duração total do usuário: {FirebaseUid}", authorUid);
                throw;
            }
        }
    }
}
    //    // 📊 NOVO: Buscar pomodoros recentes
    //    public async Task<List<PomoResponseDto>> GetRecentPomodorosAsync(string authorUid, int days = 7)
    //    {
    //        try
    //        {
    //            var startDate = DateTime.UtcNow.AddDays(-days);

    //            var pomodoros = await _context.Pomodoros
    //                .Include(p => p.User)
    //                .Where(p => p.AuthorUid == authorUid && p.CreatedAt >= startDate)
    //                .OrderByDescending(p => p.CreatedAt)
    //                .ToListAsync();

    //            return _mapper.Map<List<PomoResponseDto>>(pomodoros);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Erro ao buscar pomodoros recentes do usuário: {FirebaseUid}", authorUid);
    //            throw;
    //        }
    //    }

    //    // 📈 NOVO: Estatísticas por data
    //    public async Task<Dictionary<DateTime, int>> GetPomodoroStatsByDateAsync(string authorUid, DateTime startDate, DateTime endDate)
    //    {
    //        try
    //        {
    //            var stats = await _context.Pomodoros
    //                .Where(p => p.AuthorUid == authorUid &&
    //                       p.CreatedAt >= startDate &&
    //                       p.CreatedAt <= endDate)
    //                .GroupBy(p => p.CreatedAt.Date)
    //                .Select(g => new { Date = g.Key, TotalDuration = g.Sum(p => p.Duration) })
    //                .ToDictionaryAsync(x => x.Date, x => x.TotalDuration);

    //            return stats;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Erro ao buscar estatísticas por data do usuário: {FirebaseUid}", authorUid);
    //            throw;
    //        }
    //    }
    