// UserService.cs
using AutoMapper;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SabidosAPI_Core.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext db, IMapper mapper, ILogger<UserService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    // 🔐 NOVO: Verifica se usuário existe no SQL pelo UID
    public async Task<bool> UserExistsAsync(string firebaseUid)
    {
        try
        {
            return await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.FirebaseUid == firebaseUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do usuário: {FirebaseUid}", firebaseUid);
            throw;
        }
    }

    // 🔐 NOVO: Busca usuário completo pelo UID
    public async Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        try
        {
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            return user is null ? null : _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário por FirebaseUid: {FirebaseUid}", firebaseUid);
            throw;
        }
    }

    // ✅ Mantido: Cria/atualiza usuário no SQL
    public async Task<UserResponseDto> CreateOrUpdateAsync(string firebaseUid, string? email, UserUpdateDto? dto = null)
    {
        if (firebaseUid is null)
            throw new ArgumentNullException(nameof(firebaseUid));

        var isInMemory = _db.Database.ProviderName?.Contains("InMemory") ?? false;
        var transaction = isInMemory ? null : await _db.Database.BeginTransactionAsync();

        try
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user is null)
            {
                user = new User
                {
                    FirebaseUid = firebaseUid,
                    Email = email,
                    Name = dto?.Name ?? "Novo usuário",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
                _logger.LogInformation("Novo usuário criado: {FirebaseUid}", firebaseUid);
            }
            else
            {
                if (dto is not null)
                    _mapper.Map(dto, user);

                if (!string.IsNullOrEmpty(email) && user.Email != email)
                    user.Email = email;

                user.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Usuário atualizado: {FirebaseUid}", firebaseUid);
            }

            await _db.SaveChangesAsync();

            if (!isInMemory)
                await transaction!.CommitAsync();

            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            if (!isInMemory)
                await transaction!.RollbackAsync();

            _logger.LogError(ex, "Erro ao criar/atualizar usuário: {FirebaseUid}", firebaseUid);
            throw;
        }
    }
}