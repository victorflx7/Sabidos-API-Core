using Xunit;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Services;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Cria contexto InMemory isolado para cada teste
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Configura AutoMapper básico
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserResponseDto>();
            cfg.CreateMap<UserUpdateDto, User>();
        });
        _mapper = config.CreateMapper();

        // Logger simples para o serviço
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UserService>();

        _userService = new UserService(_context, _mapper, _logger);
    }

    [Fact]
    public async Task GetUserByFirebaseUidAsync_ComUidExistente_DeveRetornarUserResponseDto()
    {
        var firebaseUid = "12345";
        var user = new User { FirebaseUid = firebaseUid, Name = "Test User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _userService.GetUserByFirebaseUidAsync(firebaseUid);

        Assert.NotNull(result);
        Assert.Equal(firebaseUid, result.FirebaseUid);
    }

    [Fact]
    public async Task GetUserByFirebaseUidAsync_ComUidInexistente_DeveRetornarNull()
    {
        var result = await _userService.GetUserByFirebaseUidAsync("nao_existe");
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ComNovoUid_DeveCriarNovoUsuario()
    {
        var firebaseUid = "novo_uid";
        var email = "novo@email.com";
        var dto = new UserUpdateDto { Name = "Novo Usuario" };

        var result = await _userService.CreateOrUpdateAsync(firebaseUid, email, dto);

        Assert.NotNull(result);
        Assert.Single(_context.Users);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ComUidExistenteEDto_DeveAtualizarUsuario()
    {
        var firebaseUid = "uid_existente";
        var user = new User { FirebaseUid = firebaseUid, Name = "Antigo" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UserUpdateDto { Name = "Atualizado" };
        await _userService.CreateOrUpdateAsync(firebaseUid, "email@teste.com", dto);

        var atualizado = await _context.Users.FirstAsync();
        Assert.Equal("Atualizado", atualizado.Name);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ComFirebaseUidNulo_DeveLancarArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _userService.CreateOrUpdateAsync(null!, "email@teste.com", null));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
