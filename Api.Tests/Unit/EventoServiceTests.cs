using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using AutoMapper;
using Moq;
using Moq.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query; 
namespace Api.Tests.Unit;

public class EventoServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly EventoService _service;

    private readonly string TestAuthorUid = "firebase-uid-test-1";

    public EventoServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        // É necessário configurar um DbSet vazio, pois o EF Core mocking requer isso.
        _mockContext = new Mock<AppDbContext>();
        _service = new EventoService(_mockContext.Object, _mockMapper.Object);
    }

    // ---------------------------------------------------------
    // Testes para GetEventosCountByUserAsync
    // ---------------------------------------------------------

    // --- Testes para GetAllEventosAsync ---

    [Fact]
    public async Task GetAllEventosAsync_ComUid_DeveRetornarApenasEventosDoUsuario()
    {
        // Arrange
        var eventosData = new List<Evento>
    {
        new Evento { Id = 1, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now },
        new Evento { Id = 2, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now.AddDays(1) },
        new Evento { Id = 3, AuthorUid = "outro-uid", DataEvento = DateTime.Now }
    };

        // Configura o DbSet com os dados
        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Simula o mapeamento de 2 entidades para 2 DTOs
        _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.Is<List<Evento>>(list => list.Count == 2)))
                    .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto() });

        // Act
        var result = await _service.GetAllEventosAsync(TestAuthorUid);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllEventosAsync_SemUid_DeveRetornarTodosEventos()
    {
        // Arrange
        var eventosData = new List<Evento>
    {
        new Evento { Id = 1, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now },
        new Evento { Id = 2, AuthorUid = "outro-uid", DataEvento = DateTime.Now },
    };

        // Configura o DbSet com os dados
        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Simula o mapeamento de 2 entidades para 2 DTOs
        _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.Is<List<Evento>>(list => list.Count == 2)))
                    .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto() });

        // Act
        var result = await _service.GetAllEventosAsync(null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    // --- Testes para GetEventosByIdAsync ---

    [Fact]
    public async Task GetEventosByIdAsync_ComIdExistente_DeveRetornarDto()
    {
        // Arrange
        const int targetId = 2;
        var eventoData = new Evento { Id = targetId, TitleEvent = "Evento Encontrado" };
        var eventosData = new List<Evento> { eventoData };
        var expectedDto = new EventoResponseDto { Id = targetId, TitleEvent = "Evento Encontrado" };

        // Configura o DbSet com os dados
        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Simula o mapeamento
        _mockMapper.Setup(m => m.Map<EventoResponseDto>(It.IsAny<Evento>())).Returns(expectedDto);

        // Act
        var result = await _service.GetEventosByIdAsync(targetId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Evento Encontrado", result.TitleEvent);
    }

    [Fact]
    public async Task GetEventosByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var eventosData = new List<Evento> { new Evento { Id = 1 } };

        // Configura o DbSet com os dados (apenas ID=1 existe)
        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Act
        var result = await _service.GetEventosByIdAsync(999);

        // Assert
        Assert.Null(result);
        // Verifica que o mapper não foi chamado, pois o evento não foi encontrado
        _mockMapper.Verify(m => m.Map<EventoResponseDto>(It.IsAny<Evento>()), Times.Never);
    }

    [Fact]
    public async Task GetEventosCountByUserAsync_ComEventosExistentes_DeveRetornarContagemCorreta()
    {
        // Arrange
        var eventosData = new List<Evento>
        {
            new Evento { Id = 1, AuthorUid = TestAuthorUid },
            new Evento { Id = 2, AuthorUid = TestAuthorUid },
            new Evento { Id = 3, AuthorUid = "other-uid" }
        };

        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Act
        var result = await _service.GetEventosCountByUserAsync(TestAuthorUid);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetEventosCountByUserAsync_SemEventos_DeveRetornarZero()
    {
        // Arrange
        var eventosData = new List<Evento>
        {
            new Evento { Id = 1, AuthorUid = "other-uid" }
        };

        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

        // Act
        var result = await _service.GetEventosCountByUserAsync(TestAuthorUid);

        // Assert
        Assert.Equal(0, result);
    }

    // ---------------------------------------------------------
    // Testes para CreateEventoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateEventoAsync_ComDadosValidos_DeveSalvarERetornarDto()
    {
        // Atenção: O DTO de entrada é o EventoResponseDto
        // Arrange
        var createDto = new EventoResponseDto { TitleEvent = "Reunião" };
        var eventoModel = new Evento { Id = 5, TitleEvent = "Reunião" };

        _mockMapper.Setup(m => m.Map<Evento>(createDto)).Returns(eventoModel);
        _mockMapper.Setup(m => m.Map<EventoResponseDto>(eventoModel)).Returns(createDto); // Mapeamento de volta

        _mockContext.Setup(c => c.Eventos.Add(It.IsAny<Evento>()));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateEventoAsync(createDto, TestAuthorUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.TitleEvent, result.TitleEvent);
        _mockContext.Verify(c => c.Eventos.Add(It.IsAny<Evento>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Verifica se o AuthorUid foi setado no modelo
        Assert.Equal(TestAuthorUid, eventoModel.AuthorUid);
    }

    // ---------------------------------------------------------
    // Testes para UpdateEventoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateEventoAsync_ComIdExistente_DeveAtualizarERetornarDto()
    {
        // Arrange
        var existingEvento = new Evento { Id = 10, TitleEvent = "Antigo" };
        var updateDto = new EventoResponseDto { TitleEvent = "Novo" };
        var updatedResponseDto = new EventoResponseDto { Id = 10, TitleEvent = "Novo" };

        // Simula o FindAsync
        _mockContext.Setup(c => c.Eventos.FindAsync(10)).ReturnsAsync(existingEvento);
        // Simula o mapeamento
        _mockMapper.Setup(m => m.Map(updateDto, existingEvento));
        _mockMapper.Setup(m => m.Map<EventoResponseDto>(existingEvento)).Returns(updatedResponseDto);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateEventoAsync(10, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Novo", result.TitleEvent);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEventoAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        _mockContext.Setup(c => c.Eventos.FindAsync(999)).ReturnsAsync((Evento)null);

        // Act
        var result = await _service.UpdateEventoAsync(999, new EventoResponseDto());

        // Assert
        Assert.Null(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------
    // Testes para DeleteEventoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteEventoAsync_ComIdExistente_DeveDeletarERetornarTrue()
    {
        // Arrange
        var eventoToDelete = new Evento { Id = 100 };
        _mockContext.Setup(c => c.Eventos.FindAsync(100)).ReturnsAsync(eventoToDelete);

        _mockContext.Setup(c => c.Eventos.Remove(eventoToDelete));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteEventoAsync(100);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.Eventos.Remove(eventoToDelete), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEventoAsync_ComIdInexistente_DeveRetornarFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.Eventos.FindAsync(999)).ReturnsAsync((Evento)null);

        // Act
        var result = await _service.DeleteEventoAsync(999);

        // Assert
        Assert.False(result);
        _mockContext.Verify(c => c.Eventos.Remove(It.IsAny<Evento>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}