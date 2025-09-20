using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
public class ResumoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ResumoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    
    public async Task<List<ResumoResponseDto>> GetAllResumosAsync(string? userUId = null)
    {
        var query = _context.Resumos.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrEmpty(userUId))
            query = query.Where(p => p.AuthorUid == userUId);

        var resumos = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return _mapper.Map<List<ResumoResponseDto>>(resumos);
    }

    public async Task<ResumoResponseDto?> GetResumoByIdAsync(int resumoId)
    {
        var resumo = await _context.Resumos
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == resumoId);
        if (resumo == null) return null;
        return _mapper.Map<ResumoResponseDto>(resumo);
    }

    public async Task<int> GetResumosCountByUserAsync(string authorUid)
    {
        return await _context.Resumos.CountAsync(e => e.AuthorUid == authorUid);
    }

    public async Task<ResumoResponseDto> CreateResumoAsync(ResumoCreateUpdateDto resumoDto, string authorUid, string nameAuthor)
    {
        var resumo = _mapper.Map<Resumo>(resumoDto);
        _context.Resumos.Add(resumo);
        resumo.AuthorUid = authorUid;
        resumo.AuthorName = nameAuthor;
        resumo.CreatedAt = DateTime.UtcNow;
        resumo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ResumoResponseDto>(resumo);
    }

    public async Task<ResumoResponseDto?> UpdateresumoAsync(int resumoId, ResumoCreateUpdateDto dto)
    {
        var resumo = await _context.Resumos
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == resumoId);

        if (resumo == null) return null;

        _mapper.Map(dto, resumo);
        resumo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ResumoResponseDto>(resumo);
    }

    
    public async Task<bool> DeleteResumoAsync(int resumoId)
    {
        var resumo = await _context.Resumos.FirstOrDefaultAsync(p => p.Id == resumoId);
        if (resumo == null) return false;

        _context.Resumos.Remove(resumo);
        await _context.SaveChangesAsync();
        return true;
    }


    //public async Task<bool> ResumoExistsAsync(int resumoId)
    //{
    //    return await _context.Resumos.AnyAsync(p => p.Id == resumoId);
    //}

    //public async Task<bool> IsUserOwnerOfResumoAsync(int resumoId, string userUid)
    //{
    //    var resumo = await _context.Resumos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == resumoId);
    //    if (resumo == null) return false;
    //    return resumo.AuthorUid == userUid;
    //}

    //public async Task<List<ResumoResponseDto>> GetLatestResumosAsync(int count = 5)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .OrderByDescending(p => p.CreatedAt)
    //        .Take(count)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}
    //public async Task<List<ResumoResponseDto>> GetResumosByTitleAsync(string title)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(p => p.Titulo.Contains(title))
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<List<ResumoResponseDto>> GetResumosByAuthorNameAsync(string authorName)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(p => p.AuthorName.Contains(authorName))
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<List<ResumoResponseDto>> GetResumosByDateRangeAsync(DateTime startDate, DateTime endDate)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<List<ResumoResponseDto>> GetResumosByKeywordAsync(string keyword)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(p => p.Titulo.Contains(keyword) || p.Conteudo.Contains(keyword))
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<List<ResumoResponseDto>> GetResumosPagedAsync(int pageNumber, int pageSize)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .OrderByDescending(p => p.CreatedAt)
    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<int> GetTotalResumosCountAsync()
    //{
    //    return await _context.Resumos.CountAsync();
    //}

    //public async Task<double> GetAverageResumosPerUserAsync()
    //{
    //    var totalResumos = await _context.Resumos.CountAsync();
    //    var totalUsers = await _context.Users.CountAsync();
    //    return totalUsers == 0 ? 0 : (double)totalResumos / totalUsers;
    //}
    //public async Task<List<ResumoResponseDto>> GetMostActiveUsersResumosAsync(int topN)
    //{
    //    var topUsers = await _context.Resumos
    //        .GroupBy(r => r.AuthorUid)
    //        .OrderByDescending(g => g.Count())
    //        .Take(topN)
    //        .Select(g => g.Key)
    //        .ToListAsync();
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(r => topUsers.Contains(r.AuthorUid))
    //        .OrderByDescending(r => r.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}
    //public async Task<List<ResumoResponseDto>> GetResumosWithPaginationAsync(int pageNumber, int pageSize)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .OrderByDescending(p => p.CreatedAt)
    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    //public async Task<int> GetResumosCountAsync()
    //{
    //    return await _context.Resumos.CountAsync();
    //}

    //public async Task<List<ResumoResponseDto>> GetResumosByContentKeywordAsync(string keyword)
    //{
    //    var resumos = await _context.Resumos
    //        .Include(p => p.User)
    //        .Where(p => p.Conteudo.Contains(keyword))
    //        .OrderByDescending(p => p.CreatedAt)
    //        .ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}
    //public async Task<List<ResumoResponseDto>> GetResumosByMultipleCriteriaAsync(string? title = null, string? authorName = null, DateTime? startDate = null, DateTime? endDate = null)
    //{
    //    var query = _context.Resumos.Include(p => p.User).AsQueryable();
    //    if (!string.IsNullOrEmpty(title))
    //        query = query.Where(p => p.Titulo.Contains(title));
    //    if (!string.IsNullOrEmpty(authorName))
    //        query = query.Where(p => p.AuthorName.Contains(authorName));
    //    if (startDate.HasValue)
    //        query = query.Where(p => p.CreatedAt >= startDate.Value);
    //    if (endDate.HasValue)
    //        query = query.Where(p => p.CreatedAt <= endDate.Value);
    //    var resumos = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    //    return _mapper.Map<List<ResumoResponseDto>>(resumos);
    //}

    
}




