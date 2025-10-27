using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using AutoMapper;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore; // Mantido para simular chamadas do EF Core

// Nota: Adaptei o namespace do Service para refletir o que o seu código geralmente usa.

public class ResumoServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ResumoService _service;

    private readonly string TestAuthorUid = "firebase-uid-resumo-123";
    private readonly string TestAuthorName = "Autor Teste";

    public ResumoServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        _mockContext = new Mock<AppDbContext>(options); 

        _service = new ResumoService(_mockContext.Object, _mockMapper.Object);
    }

    // ---------------------------------------------------------
    // Testes para GetResumosCountByUserAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task GetResumosCountByUserAsync_ComResumosExistentes_DeveRetornarContagemCorreta()
    {
        // Arrange
        var resumosData = new List<Resumo>
        {
            new Resumo { Id = 1, AuthorUid = TestAuthorUid },
            new Resumo { Id = 2, AuthorUid = TestAuthorUid },
            new Resumo { Id = 3, AuthorUid = "other-uid" }
        };

        _mockContext.Setup(c => c.Resumos).ReturnsDbSet(resumosData);

        // Act
        var result = await _service.GetResumosCountByUserAsync(TestAuthorUid);

        // Assert
        Assert.Equal(2, result);
    }

    // ---------------------------------------------------------
    // Testes para CreateResumoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task CreateResumoAsync_ComDadosValidos_DeveSalvarERetornarDto()
    {
        // Arrange
        var createDto = new ResumoCreateUpdateDto { Titulo = "Novo Resumo", Conteudo = "Detalhes" };
        var resumoModel = new Resumo { Titulo = "Novo Resumo" };
        var responseDto = new ResumoResponseDto { Id = 1, Titulo = "Novo Resumo" };

        _mockMapper.Setup(m => m.Map<Resumo>(createDto)).Returns(resumoModel);
        _mockMapper.Setup(m => m.Map<ResumoResponseDto>(resumoModel)).Returns(responseDto);

        _mockContext.Setup(c => c.Resumos.Add(It.IsAny<Resumo>()));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateResumoAsync(createDto, TestAuthorUid, TestAuthorName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(responseDto.Titulo, result.Titulo);
        _mockContext.Verify(c => c.Resumos.Add(It.IsAny<Resumo>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(TestAuthorUid, resumoModel.AuthorUid);
        Assert.Equal(TestAuthorName, resumoModel.AuthorName);
    }

    // ---------------------------------------------------------
    // Testes para UpdateresumoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task UpdateResumoAsync_ComIdExistente_DeveAtualizarERetornarDto()
    {
        // Arrange
        var existingResumo = new Resumo { Id = 10, Titulo = "Antigo", AuthorUid = TestAuthorUid };
        var updateDto = new ResumoCreateUpdateDto { Titulo = "Atualizado" };
        var updatedResponseDto = new ResumoResponseDto { Id = 10, Titulo = "Atualizado" };

        var resumosData = new List<Resumo> { existingResumo };

        // 🔑 CORREÇÃO: Apenas configure o DbSet com os dados. 
        // Moq.EntityFrameworkCore simulará o FirstOrDefaultAsync/Include.
        _mockContext.Setup(c => c.Resumos).ReturnsDbSet(resumosData);

        _mockMapper.Setup(m => m.Map(updateDto, existingResumo));
        _mockMapper.Setup(m => m.Map<ResumoResponseDto>(existingResumo)).Returns(updatedResponseDto);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateresumoAsync(10, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Atualizado", result.Titulo);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResumoAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        // Apenas configure o DbSet para não ter o item, a consulta falhará naturalmente.
        _mockContext.Setup(c => c.Resumos).ReturnsDbSet(new List<Resumo>());

        // Act
        var result = await _service.UpdateresumoAsync(999, new ResumoCreateUpdateDto());

        // Assert
        Assert.Null(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------
    // Testes para DeleteResumoAsync
    // ---------------------------------------------------------

    [Fact]
    public async Task DeleteResumoAsync_ComIdExistente_DeveDeletarERetornarTrue()
    {
        // Arrange
        var resumoToDelete = new Resumo { Id = 100 };
        var resumosData = new List<Resumo> { resumoToDelete };

        // 🔑 CORREÇÃO: Apenas configure o DbSet.
        _mockContext.Setup(c => c.Resumos).ReturnsDbSet(resumosData);

        _mockContext.Setup(c => c.Resumos.Remove(resumoToDelete));
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteResumoAsync(100);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.Resumos.Remove(It.IsAny<Resumo>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteResumoAsync_ComIdInexistente_DeveRetornarFalse()
    {
        // Arrange
        _mockContext.Setup(c => c.Resumos).ReturnsDbSet(new List<Resumo>()); // Simular banco vazio

        // Act
        var result = await _service.DeleteResumoAsync(999);

        // Assert
        Assert.False(result);
        _mockContext.Verify(c => c.Resumos.Remove(It.IsAny<Resumo>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}