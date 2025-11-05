//using SabidosAPI_Core.Data;
//using SabidosAPI_Core.DTOs;
//using SabidosAPI_Core.Models;
//using SabidosAPI_Core.Services;
//using AutoMapper;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore; // ADICIONADO: Necessário para DbContextOptionsBuilder e UseInMemoryDatabase
//using Microsoft.EntityFrameworkCore.Query;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;
//using System.Linq;
//using System.Linq.Expressions;
//using System;

//namespace Api.Tests.Unit;

//public class EventoServiceTests
//{
//    private readonly Mock<AppDbContext> _mockContext;
//    private readonly Mock<IMapper> _mockMapper;
//    private readonly EventoService _service;

//    private readonly string TestAuthorUid = "firebase-uid-test-1";

//    public EventoServiceTests()
//    {
//        _mockMapper = new Mock<IMapper>();

//        // CORREÇÃO ESSENCIAL: Cria opções DbContextOptions<AppDbContext> válidas (não-nulas)
//        // O banco de dados In-Memory é usado para criar uma instância válida.
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        // Passa as opções válidas para o construtor do Mock, resolvendo o ArgumentNullException.
//        _mockContext = new Mock<AppDbContext>(options);

//        // --- MOCKS GLOBAIS OBRIGATÓRIOS ---

//        // 1. Mock para SaveChangesAsync (Usado em Create, Update, Delete)
//        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

//        // 2. Configura o serviço
//        _service = new EventoService(_mockContext.Object, _mockMapper.Object);
//    }

//    // ---------------------------------------------------------
//    // Testes para GetAllEventosAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task GetAllEventosAsync_ComUid_DeveRetornarApenasEventosDoUsuario()
//    {
//        // Arrange
//        var eventosData = new List<Evento>
//        {
//            new Evento { Id = 1, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now },
//            new Evento { Id = 2, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now.AddDays(1) },
//            new Evento { Id = 3, AuthorUid = "outro-uid", DataEvento = DateTime.Now }
//        };

//        // Usa ReturnsDbSet do Moq.EntityFrameworkCore
//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        // Simula o mapeamento de 2 entidades para 2 DTOs (apenas os do TestAuthorUid)
//        _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.Is<List<Evento>>(list => list.Count == 2)))
//                    .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto() });

//        // Act
//        var result = await _service.GetAllEventosAsync(TestAuthorUid);

//        // Assert
//        Assert.Equal(2, result.Count);
//    }

//    [Fact]
//    public async Task GetAllEventosAsync_SemUid_DeveRetornarTodosEventos()
//    {
//        // Arrange
//        var eventosData = new List<Evento>
//        {
//            new Evento { Id = 1, AuthorUid = TestAuthorUid, DataEvento = DateTime.Now },
//            new Evento { Id = 2, AuthorUid = "outro-uid", DataEvento = DateTime.Now },
//        };

//        // Usa ReturnsDbSet do Moq.EntityFrameworkCore
//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        // Simula o mapeamento de 2 entidades para 2 DTOs
//        _mockMapper.Setup(m => m.Map<List<EventoResponseDto>>(It.Is<List<Evento>>(list => list.Count == 2)))
//                    .Returns(new List<EventoResponseDto> { new EventoResponseDto(), new EventoResponseDto() });

//        // Act
//        var result = await _service.GetAllEventosAsync(null);

//        // Assert
//        Assert.Equal(2, result.Count);
//    }

//    // ---------------------------------------------------------
//    // Testes para GetEventosByIdAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task GetEventosByIdAsync_ComIdExistente_DeveRetornarDto()
//    {
//        // Arrange
//        const int targetId = 2;
//        var eventoData = new Evento { Id = targetId, TitleEvent = "Evento Encontrado" };
//        var eventosData = new List<Evento> { eventoData };

//        // Usa ReturnsDbSet do Moq.EntityFrameworkCore. Isso também permite que FindAsync funcione.
//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        var expectedDto = new EventoResponseDto { Id = targetId, TitleEvent = "Evento Encontrado" };
//        _mockMapper.Setup(m => m.Map<EventoResponseDto>(It.IsAny<Evento>())).Returns(expectedDto);

//        // Act
//        var result = await _service.GetEventoByIdAsync(targetId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(targetId, result.Id);
//        Assert.Equal("Evento Encontrado", result.TitleEvent);
//    }

//    [Fact]
//    public async Task GetEventosByIdAsync_ComIdInexistente_DeveRetornarNull()
//    {
//        // Arrange
//        var eventosData = new List<Evento> { new Evento { Id = 1 } };
//        // Usa ReturnsDbSet do Moq.EntityFrameworkCore
//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        // Act
//        var result = await _service.GetEventoByIdAsync(999);

//        // Assert
//        Assert.Null(result);
//        _mockMapper.Verify(m => m.Map<EventoResponseDto>(It.IsAny<Evento>()), Times.Never);
//    }

//    // ---------------------------------------------------------
//    // Testes para GetEventosCountByUserAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task GetEventosCountByUserAsync_ComEventosExistentes_DeveRetornarContagemCorreta()
//    {
//        // Arrange
//        var eventosData = new List<Evento>
//        {
//            new Evento { Id = 1, AuthorUid = TestAuthorUid },
//            new Evento { Id = 2, AuthorUid = TestAuthorUid },
//            new Evento { Id = 3, AuthorUid = "other-uid" }
//        };

//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        // Act
//        var result = await _service.GetEventosCountByUserAsync(TestAuthorUid);

//        // Assert
//        Assert.Equal(2, result);
//    }

//    [Fact]
//    public async Task GetEventosCountByUserAsync_SemEventos_DeveRetornarZero()
//    {
//        // Arrange
//        var eventosData = new List<Evento>
//        {
//            new Evento { Id = 1, AuthorUid = "other-uid" }
//        };

//        _mockContext.Setup(c => c.Eventos).ReturnsDbSet(eventosData);

//        // Act
//        var result = await _service.GetEventosCountByUserAsync(TestAuthorUid);

//        // Assert
//        Assert.Equal(0, result);
//    }

//    // ---------------------------------------------------------
//    // Testes para CreateEventoAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task CreateEventoAsync_ComDadosValidos_DeveSalvarERetornarDto()
//    {
//        // Arrange
//        var createDto = new EventoCreateDto { TitleEvent = "Reunião" };
//        var eventoModel = new Evento { Id = 5, TitleEvent = "Reunião" }; // Modelo após mapear createDto

//        // 1. Mock: Mapeamento de Entrada (CreateDto -> Model) - OK
//        _mockMapper.Setup(m => m.Map<Evento>(createDto)).Returns(eventoModel);

//        // 2. 🔑 CORREÇÃO: Mock: Mapeamento de Saída (Model -> ResponseDto)
//        // O serviço retorna _mapper.Map<EventoResponseDto>(eventoModel)
//        // Precisamos simular ESTE mapeamento para o valor de retorno.
//        var expectedResponseDto = new EventoResponseDto { Id = eventoModel.Id, TitleEvent = eventoModel.TitleEvent };
//        _mockMapper.Setup(m => m.Map<EventoResponseDto>(eventoModel)).Returns(expectedResponseDto);

//        // Mocks do DbContext (Add e SaveChanges)
//        _mockContext.Setup(c => c.Eventos.Add(It.IsAny<Evento>()));
//        // SaveChangesAsync já está mockado globalmente no construtor

//        // Act
//        var result = await _service.CreateEventoAsync(createDto, TestAuthorUid);

//        // Assert (Linha 209 que estava falhando)
//        Assert.NotNull(result); // Deve passar agora, pois Map<EventoResponseDto> retorna expectedResponseDto
//        Assert.Equal(expectedResponseDto.TitleEvent, result.TitleEvent); // Verifica o conteúdo
//        Assert.Equal(expectedResponseDto.Id, result.Id); // Verifica o ID

//        // Verifica mocks
//        _mockContext.Verify(c => c.Eventos.Add(It.Is<Evento>(e => e.AuthorUid == TestAuthorUid)), Times.Once); // Garante que AuthorUid foi setado
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//        Assert.Equal(TestAuthorUid, eventoModel.AuthorUid); // Verificação extra no modelo
//    }

//    // ---------------------------------------------------------
//    // Testes para UpdateEventoAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task UpdateEventoAsync_ComIdExistente_DeveAtualizarERetornarDto()
//    {
//        // Arrange
//        var existingEvento = new Evento { Id = 10, TitleEvent = "Antigo" };
//        var updateDto = new EventoResponseDto { TitleEvent = "Novo" };
//        var updatedResponseDto = new EventoResponseDto { Id = 10, TitleEvent = "Novo" };

//        // Simula o FindAsync
//        _mockContext.Setup(c => c.Eventos.FindAsync(It.Is<object[]>(ids => (int)ids[0] == 10))).ReturnsAsync(existingEvento);

//        // Simula o mapeamento
//        _mockMapper.Setup(m => m.Map(updateDto, existingEvento));
//        _mockMapper.Setup(m => m.Map<EventoResponseDto>(existingEvento)).Returns(updatedResponseDto);
//        // SaveChangesAsync já está mockado no construtor

//        // Act
//        var result = await _service.UpdateEventoAsync(10, updateDto, TestAuthorUid);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal("Novo", result.TitleEvent);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task UpdateEventoAsync_ComIdInexistente_DeveRetornarNull()
//    {
//        // Arrange
//        // Simula o FindAsync retornando null
//        _mockContext.Setup(c => c.Eventos.FindAsync(It.Is<object[]>(ids => (int)ids[0] == 999))).ReturnsAsync((Evento)null);

//        // Act
//        var result = await _service.UpdateEventoAsync(999, new EventoResponseDto());

//        // Assert
//        Assert.Null(result);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }

//    // ---------------------------------------------------------
//    // Testes para DeleteEventoAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task DeleteEventoAsync_ComIdExistente_DeveDeletarERetornarTrue()
//    {
//        // Arrange
//        var eventoToDelete = new Evento { Id = 100 };
//        // Simula o FindAsync
//        _mockContext.Setup(c => c.Eventos.FindAsync(It.Is<object[]>(ids => (int)ids[0] == 100))).ReturnsAsync(eventoToDelete);

//        _mockContext.Setup(c => c.Eventos.Remove(eventoToDelete));
//        // SaveChangesAsync já está mockado no construtor

//        // Act
//        var result = await _service.DeleteEventoAsync(100);

//        // Assert
//        Assert.True(result);
//        _mockContext.Verify(c => c.Eventos.Remove(eventoToDelete), Times.Once);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteEventoAsync_ComIdInexistente_DeveRetornarFalse()
//    {
//        // Arrange
//        // Simula o FindAsync retornando null
//        _mockContext.Setup(c => c.Eventos.FindAsync(It.Is<object[]>(ids => (int)ids[0] == 999))).ReturnsAsync((Evento)null);

//        // Act
//        var result = await _service.DeleteEventoAsync(999);

//        // Assert
//        Assert.False(result);
//        _mockContext.Verify(c => c.Eventos.Remove(It.IsAny<Evento>()), Times.Never);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }
//}