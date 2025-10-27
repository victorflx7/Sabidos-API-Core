//using SabidosAPI_Core.Data;
//using SabidosAPI_Core.DTOs;
//using SabidosAPI_Core.Models;
//using SabidosAPI_Core.Services;
//using AutoMapper;
//using Moq;
//using Moq.EntityFrameworkCore;

//namespace Api.Tests.Unit;

//public class FlashcardServiceTests
//{
//    private readonly Mock<AppDbContext> _mockContext;
//    private readonly Mock<IMapper> _mockMapper;
//    private readonly FlashcardService _service;

//    private readonly string TestAuthorUid = "firebase-uid-test-1";
//    private readonly string TestAuthorName = "Teste User";

//    public FlashcardServiceTests()
//    {
//        _mockMapper = new Mock<IMapper>();
//        _mockContext = new Mock<AppDbContext>();
//        _service = new FlashcardService(_mockContext.Object, _mockMapper.Object);
//    }

//    // ---------------------------------------------------------
//    // Testes para GetFlashcardsCountByUserAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task GetFlashcardsCountByUserAsync_ComFlashcardsExistentes_DeveRetornarContagemCorreta()
//    {
//        // Arrange
//        var flashcardsData = new List<Flashcard>
//        {
//            new Flashcard { Id = 1, AuthorUid = TestAuthorUid },
//            new Flashcard { Id = 2, AuthorUid = TestAuthorUid },
//            new Flashcard { Id = 3, AuthorUid = "other-uid" }
//        };

//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);

//        // Act
//        var result = await _service.GetFlashcardsCountByUserAsync(TestAuthorUid);

//        // Assert
//        Assert.Equal(2, result);
//    }

//    [Fact]
//    public async Task GetFlashcardsCountByUserAsync_SemFlashcards_DeveRetornarZero()
//    {
//        // Arrange
//        var flashcardsData = new List<Flashcard>
//        {
//            new Flashcard { Id = 1, AuthorUid = "other-uid" }
//        };

//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);

//        // Act
//        var result = await _service.GetFlashcardsCountByUserAsync(TestAuthorUid);

//        // Assert
//        Assert.Equal(0, result);
//    }

//    // ---------------------------------------------------------
//    // Testes para GetFlashcardByIdAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task GetFlashcardByIdAsync_ComIdExistente_DeveRetornarFlashcard()
//    {
//        // Arrange
//        var flashcardModel = new Flashcard { Id = 10, AuthorUid = TestAuthorUid };
//        var responseDto = new FlashcardResponseDto { Id = 10 };
//        var flashcardsData = new List<Flashcard> { flashcardModel };

//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);
//        _mockMapper.Setup(m => m.Map<FlashcardResponseDto>(flashcardModel)).Returns(responseDto);

//        // Act
//        var result = await _service.GetFlashcardByIdAsync(10);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(10, result.Id);
//    }

//    [Fact]
//    public async Task GetFlashcardByIdAsync_ComIdInexistente_DeveRetornarNull()
//    {
//        // Arrange
//        var flashcardsData = new List<Flashcard> { new Flashcard { Id = 1 } };
//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);

//        // Act
//        var result = await _service.GetFlashcardByIdAsync(999);

//        // Assert
//        Assert.Null(result);
//    }

//    // ---------------------------------------------------------
//    // Testes para CreateFlashcardAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task CreateFlashcardAsync_ComDadosValidos_DeveSalvarERetornarDto()
//    {
//        // Arrange
//        var createDto = new FlashcardCreateUpdateDto { Frente = "Pergunta", Verso = "Resposta" };
//        var flashcardModel = new Flashcard { Id = 5, Frente = "Pergunta" };
//        var responseDto = new FlashcardResponseDto { Id = 5, Frente = "Pergunta" };

//        _mockMapper.Setup(m => m.Map<Flashcard>(createDto)).Returns(flashcardModel);
//        _mockMapper.Setup(m => m.Map<FlashcardResponseDto>(flashcardModel)).Returns(responseDto);

//        _mockContext.Setup(c => c.Flashcards.Add(It.IsAny<Flashcard>()));
//        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

//        // Act
//        var result = await _service.CreateFlashcardAsync(createDto, TestAuthorUid, TestAuthorName);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(responseDto.Frente, result.Frente);
//        _mockContext.Verify(c => c.Flashcards.Add(It.IsAny<Flashcard>()), Times.Once);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//        // Verifica se o AuthorUid e AuthorName foram setados no modelo
//        Assert.Equal(TestAuthorUid, flashcardModel.AuthorUid);
//        Assert.Equal(TestAuthorName, flashcardModel.AuthorName);
//    }

//    // ---------------------------------------------------------
//    // Testes para DeleteFlashcardAsync
//    // ---------------------------------------------------------

//    [Fact]
//    public async Task DeleteFlashcardAsync_ComIdExistente_DeveDeletarERetornarTrue()
//    {
//        // Arrange
//        var flashcardToDelete = new Flashcard { Id = 100 };
//        var flashcardsData = new List<Flashcard> { flashcardToDelete };

//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);
//        _mockContext.Setup(c => c.Flashcards.Remove(flashcardToDelete));
//        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

//        // Act
//        var result = await _service.DeleteFlashcardAsync(100);

//        // Assert
//        Assert.True(result);
//        _mockContext.Verify(c => c.Flashcards.Remove(flashcardToDelete), Times.Once);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteFlashcardAsync_ComIdInexistente_DeveRetornarFalse()
//    {
//        // Arrange
//        var flashcardsData = new List<Flashcard>();
//        _mockContext.Setup(c => c.Flashcards).ReturnsDbSet(flashcardsData);

//        // Act
//        var result = await _service.DeleteFlashcardAsync(999);

//        // Assert
//        Assert.False(result);
//        // Deve falhar silenciosamente, sem chamar Remove ou SaveChanges
//        _mockContext.Verify(c => c.Flashcards.Remove(It.IsAny<Flashcard>()), Times.Never);
//        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }
//}