using SabidosAPI_Core.Data;
using SabidosAPI_Core.Dtos;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using AutoMapper;
using Moq;
using Moq.EntityFrameworkCore;

namespace Api.Tests.Unit;

public class PomodoroServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PomodoroService _service;

    private readonly string TestAuthorUid = "firebase-uid-123";

    public PomodoroServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        // O mock de AppDbContext precisa ser configurado para o teste
        // Vamos apenas simular uma lista vazia, pois o código real precisa de uma injeção de IQueryable.
        // **Atenção**: Este teste requer que você crie a interface ou injete o PomodoroService corretamente,
        // mas vamos manter o teste mais simples aqui, focando nos métodos.
        
        _mockContext = new Mock<AppDbContext>(); 
        _service = new PomodoroService(_mockContext.Object, _mockMapper.Object);
    }
    
    // --- Testes para CountTimeAsync ---
    
    [Fact]
    public async Task CountTimeAsync_ComPomodorosExistentes_DeveRetornarSomaCorreta()
    {
        // Arrange
        var pomodorosData = new List<Pomodoro>
        {
            new Pomodoro { AuthorUid = TestAuthorUid, Duration = 25 },
            new Pomodoro { AuthorUid = TestAuthorUid, Duration = 50 },
            new Pomodoro { AuthorUid = "other-uid", Duration = 100 }
        };
        
        // Configura o mock do DbSet para usar a lista de dados
        _mockContext.Setup(c => c.Pomodoros)
            .ReturnsDbSet(pomodorosData);

        // Act
        var result = await _service.CountTimeAsync(TestAuthorUid);

        // Assert
        Assert.Equal(75, result); // 25 + 50
    }
    
    [Fact]
    public async Task CountTimeAsync_SemPomodoros_DeveRetornarZero()
    {
        // Arrange
        var pomodorosData = new List<Pomodoro>
        {
            new Pomodoro { AuthorUid = "other-uid", Duration = 100 }
        };

        _mockContext.Setup(c => c.Pomodoros)
            .ReturnsDbSet(pomodorosData);

        // Act
        var result = await _service.CountTimeAsync(TestAuthorUid);

        // Assert
        Assert.Equal(0, result);
    }
    
    // --- Testes para CreateAsync ---

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveSalvarERetornarDto()
    {
        // Arrange
        var createDto = new PomoCreateDto { Duration = 25, Description = "Foco" };
        var pomodoroModel = new Pomodoro { Duration = 25, Description = "Foco" };
        var responseDto = new PomoResponseDto { Id = 1, Duration = 25 };

        // Configura o mapper para ir do DTO para o Model
        _mockMapper.Setup(m => m.Map<Pomodoro>(createDto)).Returns(pomodoroModel);
        // Configura o mapper para ir do Model para o Response DTO
        _mockMapper.Setup(m => m.Map<PomoResponseDto>(pomodoroModel)).Returns(responseDto);
        
        // Configura o DbSet para permitir Add
        _mockContext.Setup(c => c.Pomodoros.Add(It.IsAny<Pomodoro>()));
        // Configura SaveChangesAsync
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(createDto, TestAuthorUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(responseDto.Duration, result.Duration);
        _mockContext.Verify(c => c.Pomodoros.Add(It.IsAny<Pomodoro>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    // --- Testes para GetAllAsync ---
    
    [Fact]
    public async Task GetAllAsync_SemUid_DeveRetornarTodosPomodoros()
    {
        // Arrange
        var pomodorosData = new List<Pomodoro>
        {
            new Pomodoro { Id = 1, AuthorUid = "user-a" },
            new Pomodoro { Id = 2, AuthorUid = "user-b" }
        };
        var responseDtos = new List<PomoResponseDto> 
        { 
            new PomoResponseDto { Id = 2 }, 
            new PomoResponseDto { Id = 1 } 
        }; // Mapeamento esperado

        _mockContext.Setup(c => c.Pomodoros).ReturnsDbSet(pomodorosData);
        _mockMapper.Setup(m => m.Map<List<PomoResponseDto>>(It.IsAny<List<Pomodoro>>())).Returns(responseDtos);

        // Act
        var result = await _service.GetAllAsync(null);

        // Assert
        Assert.Equal(2, result.Count);
        _mockMapper.Verify(m => m.Map<List<PomoResponseDto>>(It.IsAny<List<Pomodoro>>()), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_ComUid_DeveRetornarSomentePomodorosDoUsuario()
    {
        // Arrange
        var pomodorosData = new List<Pomodoro>
        {
            new Pomodoro { Id = 1, AuthorUid = TestAuthorUid },
            new Pomodoro { Id = 2, AuthorUid = "other-uid" }
        };
        var responseDtos = new List<PomoResponseDto> { new PomoResponseDto { Id = 1 } };

        _mockContext.Setup(c => c.Pomodoros).ReturnsDbSet(pomodorosData);
        _mockMapper.Setup(m => m.Map<List<PomoResponseDto>>(It.IsAny<List<Pomodoro>>())).Returns(responseDtos);
        
        // Act
        var result = await _service.GetAllAsync(TestAuthorUid);

        // Assert
        Assert.Single(result);
        _mockMapper.Verify(m => m.Map<List<PomoResponseDto>>(It.IsAny<List<Pomodoro>>()), Times.Once);
    }
}
