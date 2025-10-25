using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SabidosAPI_Core.Controllers;
using SabidosAPI_Core.Dtos;
using SabidosAPI_Core.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SabidosAPI_Core.Tests.Controllers
{
    public class PomodoroControllerTests
    {
        private readonly Mock<PomodoroService> _mockService;
        private readonly PomodoroController _controller;
        private readonly string _testUserUid = "test-uid-789";

        public PomodoroControllerTests()
        {
            _mockService = new Mock<PomodoroService>(MockBehavior.Strict, null, null); // Mock de Service
            _controller = new PomodoroController(_mockService.Object);

            // Configuração base de Claims para um usuário autenticado
            var claims = new List<Claim>
            {
                new Claim("user_id", _testUserUid), // Simula a Claim do Firebase Auth
                new Claim(ClaimTypes.NameIdentifier, "sub-fallback"),
                new Claim("sub", "sub-fallback-2")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            // Atribui o usuário (ClaimsPrincipal) ao Controller para simular autenticação
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        private void SetControllerToUnauthorized()
        {
            // Simula um usuário não autenticado (sem Claims de UID)
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        // --- Testes para GetAll() ---

        [Fact]
        public async Task GetAll_ReturnsOk_WithPomodorosForAuthenticatedUser()
        {
            // Arrange
            var expectedDtos = new List<PomoResponseDto>
            {
                new PomoResponseDto { Id = 1, AuthorUid = _testUserUid, Duration = 25 },
                new PomoResponseDto { Id = 2, AuthorUid = _testUserUid, Duration = 50 }
            };
            _mockService.Setup(s => s.GetAllAsync(_testUserUid))
                .ReturnsAsync(expectedDtos)
                .Verifiable();

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDtos = Assert.IsType<List<PomoResponseDto>>(okResult.Value);
            Assert.Equal(2, returnedDtos.Count);
            _mockService.Verify(s => s.GetAllAsync(_testUserUid), Times.Once); // Verifica se o serviço foi chamado com o UID correto
        }

        [Fact]
        public async Task GetAll_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetControllerToUnauthorized();

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            _mockService.Verify(s => s.GetAllAsync(It.IsAny<string>()), Times.Never);
        }

        // --- Testes para CountTime() ---

        [Fact]
        public async Task CountTime_ReturnsOk_WithTotalDuration()
        {
            // Arrange
            var expectedTotalTime = 250;
            _mockService.Setup(s => s.CountTimeAsync(_testUserUid))
                .ReturnsAsync(expectedTotalTime)
                .Verifiable();

            // Act
            var result = await _controller.CountTime();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<int>(okResult.Value);
            Assert.Equal(expectedTotalTime, okResult.Value);
            _mockService.Verify(s => s.CountTimeAsync(_testUserUid), Times.Once);
        }

        [Fact]
        public async Task CountTime_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetControllerToUnauthorized();

            // Act
            var result = await _controller.CountTime();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            _mockService.Verify(s => s.CountTimeAsync(It.IsAny<string>()), Times.Never);
        }

        // --- Testes para Create() ---

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_AndCallsServiceWithCorrectUid()
        {
            // Arrange
            var createDto = new PomoCreateDto { Ciclos = 4, Duration = 100, TempoTrabalho = 25, TempoDescanso = 5, Userid = 1, AuthorUid = "ignored" };
            var responseDto = new PomoResponseDto { Id = 10, Ciclos = 4, Duration = 100, AuthorUid = _testUserUid };

            _mockService.Setup(s => s.CreateAsync(createDto, _testUserUid))
                .ReturnsAsync(responseDto)
                .Verifiable();

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<PomoResponseDto>(createdResult.Value);

            Assert.Equal(nameof(_controller.GetAll), createdResult.ActionName); // Verifica se aponta para o GetAll
            Assert.Equal(responseDto.Id, returnedDto.Id);
            Assert.Equal(_testUserUid, returnedDto.AuthorUid);

            _mockService.Verify(s => s.CreateAsync(createDto, _testUserUid), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetControllerToUnauthorized();
            var createDto = new PomoCreateDto { Ciclos = 4, Duration = 100, TempoTrabalho = 25, TempoDescanso = 5, Userid = 1, AuthorUid = "ignored" };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
            _mockService.Verify(s => s.CreateAsync(It.IsAny<PomoCreateDto>(), It.IsAny<string>()), Times.Never);
        }
    }
}