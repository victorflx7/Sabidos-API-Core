
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using Xunit;

namespace Api.Tests.Unit
{
    public class ResumoServiceTests
    {
        private static IMapper BuildMapper()
        {
            // Scans the main assembly for AutoMapper profiles (profiles live in the SabidosAPI_Core project)
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Resumo).Assembly));
            return config.CreateMapper();
        }

        private static AppDbContext BuildContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateResumoAsync_SavesResumoAndReturnsDto()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using var context = BuildContext(dbName);
            var mapper = BuildMapper();
            var service = new ResumoService(context, mapper);

            var createDto = new ResumoCreateUpdateDto
            {
                Titulo = "Teste Título",
                Conteudo = "Conteúdo do resumo"
            };

            // Act
            var result = await service.CreateResumoAsync(createDto, "user-uid-1", "NomeAutor");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(createDto.Titulo, result.Titulo);
            Assert.Equal("NomeAutor", result.AuthorName);

            var saved = await context.Resumos.FirstOrDefaultAsync(r => r.Id == result.Id);
            Assert.NotNull(saved);
            Assert.Equal("user-uid-1", saved.AuthorUid);
            Assert.Equal(createDto.Conteudo, saved.Conteudo);
        }

        [Fact]
        public async Task GetAllResumosAsync_FiltersByUser_ReturnsOnlyOwnerResumos()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using var context = BuildContext(dbName);
            var mapper = BuildMapper();
            // seed two resumos
            context.Resumos.AddRange(
                new Resumo { Titulo = "A", Conteudo = "c", AuthorUid = "uid1", AuthorName = "U1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Resumo { Titulo = "B", Conteudo = "d", AuthorUid = "uid2", AuthorName = "U2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = new ResumoService(context, mapper);

            // Act
            var listUid1 = await service.GetAllResumosAsync("uid1");
            var listUid2 = await service.GetAllResumosAsync("uid2");
            var listAll = await service.GetAllResumosAsync(null);

            // Assert
            Assert.Single(listUid1);
            Assert.Equal("A", listUid1.First().Titulo);

            Assert.Single(listUid2);
            Assert.Equal("B", listUid2.First().Titulo);

            Assert.Equal(2, listAll.Count);
        }

        [Fact]
        public async Task GetResumoByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using var context = BuildContext(dbName);
            var mapper = BuildMapper();
            var service = new ResumoService(context, mapper);

            // Act
            var result = await service.GetResumoByIdAsync(9999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteResumoAsync_RemovesResumo_ReturnsTrue_WhenExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using var context = BuildContext(dbName);
            var mapper = BuildMapper();

            var resumo = new Resumo
            {
                Titulo = "ToDelete",
                Conteudo = "x",
                AuthorUid = "uidX",
                AuthorName = "NameX",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Resumos.Add(resumo);
            await context.SaveChangesAsync();

            var service = new ResumoService(context, mapper);

            // Act
            var deleted = await service.DeleteResumoAsync(resumo.Id);

            // Assert
            Assert.True(deleted);
            var exists = await context.Resumos.AnyAsync(r => r.Id == resumo.Id);
            Assert.False(exists);
        }
    }
}
