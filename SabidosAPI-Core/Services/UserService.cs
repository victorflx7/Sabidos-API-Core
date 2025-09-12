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
    public UserService(AppDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    public async Task<UserResponseDto?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        return user is null ? null : _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateOrUpdateAsync(string firebaseUid, string? email, UserUpdateDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (user is null)
        {
            user = new User { FirebaseUid = firebaseUid, Email = email, Name = dto.Name };
            _db.Users.Add(user);
        }
        else
        {
            _mapper.Map(dto, user);
            user.Email = email ?? user.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return _mapper.Map<UserResponseDto>(user);
    }
} 