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

=
    public async Task<UserResponseDto> CreateOrUpdateAsync(string firebaseUid, string? email, UserUpdateDto? dto = null)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            if (user is null)
            {
                // Criar novo usuário
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
                {
                    _mapper.Map(dto, user);
                }

               
                if (!string.IsNullOrEmpty(email) && user.Email != email)
                {
                    user.Email = email;
                }

                user.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Usuário atualizado: {FirebaseUid}", firebaseUid);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao criar/atualizar usuário: {FirebaseUid}", firebaseUid);
            throw;
        }
    }
}