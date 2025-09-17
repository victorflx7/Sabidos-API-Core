using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class ResumoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ResumoService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    
    public async Task<List<ResumoResponseDto>> GetAllresumosAsync(string? userId = null)
    {
        var query = _context.resumos.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(p => p.Id == userId);

        var resumos = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return _mapper.Map<List<ResumoResponseDto>>(resumos);
    }

   
    public async Task<ResumoResponseDto?> UpdateresumoAsync(int resumoId, string userId, ResumoCreateUpdateDto dto)
    {
        var resumo = await _context.resumos
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == resumoId && p.Id == userId);

        if (resumo == null) return null;

        _mapper.Map(dto, resumo);
        resumo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ResumoResponseDto>(resumo);
    }

   
   
    