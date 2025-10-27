using Xunit;
using Moq;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Dtos;
using SabidosAPI_Core.Services;
using SabidosAPI_Core.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SabidosAPI_Core.AutoMapper; // Assumindo que seu PomodoroProfile está aqui

// **ASSUME UMA CLASSE USER MÍNIMA PARA O SEEDING FUNCIONAR**
// Você precisa ter certeza que a classe User está disponível para o AppDbContext no teste.
// Adiciono uma estrutura para mostrar como o seeding deve ser feito.
// Você pode precisar adicionar o using de SabidosAPI_Core.Models se ele não estiver implícito.

namespace SabidosAPI_Core.Tests.Services
{
    public class PomodoroServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PomodoroService _service;
        private readonly string _testUserUid = "test-uid-123";
        private readonly string _otherUserUid = "other-uid-456";

        public PomodoroServiceTests()
        {
            // 1. Configurar o DbContext usando In-Memory Provider
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Novo DB para cada teste
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            // 2. Configurar e obter a instância real do AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PomodoroProfile());
                // Adicione outros perfis se o AppDbContext precisar de outras entidades
            });
            _mapper = mapperConfig.CreateMapper();

            // 3. Inicializar o Service
            _service = new PomodoroService(_context, _mapper);

            // 4. Popular o banco de dados com dados de teste
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // **CORREÇÃO CRÍTICA: Adicionar entidades User para satisfazer o .Include(p => p.User)
            // Assumindo que seu modelo User tem pelo menos Id e FirebaseUid/AuthorUid
            _context.Set<User>().AddRange(new List<User> // Use Set<User>() para ser explícito
            {
                // Os IDs aqui devem corresponder aos Userid nos Pomodoros
                new User { Id = 1, FirebaseUid = _testUserUid, Name = "User Test" },
                new User { Id = 2, FirebaseUid = _otherUserUid, Name = "Other User" }
            });

            _context.Pomodoros.AddRange(new List<Pomodoro>
            {
                new Pomodoro { Id = 1, AuthorUid = _testUserUid, Ciclos = 4, Duration = 100, TempoTrabalho = 25, TempoDescanso = 5, CreatedAt = DateTime.UtcNow.AddHours(-2), Userid = 1 },
                new Pomodoro { Id = 2, AuthorUid = _testUserUid, Ciclos = 2, Duration = 50, TempoTrabalho = 25, TempoDescanso = 5, CreatedAt = DateTime.UtcNow.AddHours(-1), Userid = 1 },
                new Pomodoro { Id = 3, AuthorUid = _otherUserUid, Ciclos = 1, Duration = 25, TempoTrabalho = 25, TempoDescanso = 5, CreatedAt = DateTime.UtcNow, Userid = 2 }
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPomodorosForSpecifiedUser()
        {
            // Act
            var result = await _service.GetAllAsync(_testUserUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(_testUserUid, p.AuthorUid));
            // Deve vir ordenado por CreatedAt (mais recente primeiro)
            Assert.Equal(2, result.First().Id);
            Assert.Equal(1, result.Last().Id);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyListForUnknownUser()
        {
            // Act
            var result = await _service.GetAllAsync("unknown-uid");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CountTimeAsync_ShouldReturnCorrectTotalDuration()
        {
            // Arrange
            var expectedTotalDuration = 100 + 50; // Pomodoros do _testUserUid

            // Act
            var result = await _service.CountTimeAsync(_testUserUid);

            // Assert
            Assert.Equal(expectedTotalDuration, result);
        }

        [Fact]
        public async Task CountTimeAsync_ShouldReturnZeroForUserWithoutPomodoros()
        {
            // Act
            var result = await _service.CountTimeAsync("user-no-data");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddPomodoroAndReturnResponseDto()
        {
            // Arrange
            var createDto = new PomoCreateDto
            {
                Ciclos = 3,
                Duration = 75,
                TempoTrabalho = 25,
                TempoDescanso = 5,
                Userid = 1, // Assumindo que este ID é passado pelo front-end
                AuthorUid = "should-be-ignored" // O service deve sobrescrever
            };
            var newPomodoroUid = "new-user-uid-789";

            // Act
            var result = await _service.CreateAsync(createDto, newPomodoroUid);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(createDto.Ciclos, result.Ciclos);
            Assert.Equal(createDto.Duration, result.Duration);
            Assert.Equal(newPomodoroUid, result.AuthorUid); // Verificação crucial: UID sobrescrito
            Assert.NotEqual(default, result.CreatedAt); // Deve ter sido definido pelo AutoMapper/Service

            // Verifica se foi realmente adicionado ao DB
            var dbEntry = await _context.Pomodoros.FindAsync(result.Id);
            Assert.NotNull(dbEntry);
            Assert.Equal(newPomodoroUid, dbEntry.AuthorUid);
        }
    }
}