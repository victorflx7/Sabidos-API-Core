using Xunit;
using Moq;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Linq.Expressions;

public class UserServiceTests
{
	private readonly Mock<AppDbContext> _dbMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly Mock<ILogger<UserService>> _loggerMock;
	private readonly UserService _userService;

	public UserServiceTests()
	{
		_dbMock = new Mock<AppDbContext>();
		_mapperMock = new Mock<IMapper>();
		_loggerMock = new Mock<ILogger<UserService>>();
		_userService = new UserService(_dbMock.Object, _mapperMock.Object, _loggerMock.Object);
	}

	//---------------------------------------------------------
	// Testes de Unidade para GetUserByFirebaseUidAsync
	//---------------------------------------------------------

	[Fact]
	public async Task GetUserByFirebaseUidAsync_ComUidExistente_DeveRetornarUserResponseDto()
	{
		var firebaseUid = "12345";
		var user = new User { FirebaseUid = firebaseUid, Name = "Test User" };
		var userResponseDto = new UserResponseDto { FirebaseUid = firebaseUid, Name = "Test User" };

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, It.IsAny<CancellationToken>()))
			 .ReturnsAsync(user);
		_mapperMock.Setup(m => m.Map<UserResponseDto>(user)).Returns(userResponseDto);

		var result = await _userService.GetUserByFirebaseUidAsync(firebaseUid);

		Assert.NotNull(result);
		Assert.Equal(firebaseUid, result.FirebaseUid);
	}

	[Fact]
	public async Task GetUserByFirebaseUidAsync_ComUidInexistente_DeveRetornarNull()
	{
		var firebaseUid = "nonexistent_uid";
		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, It.IsAny<CancellationToken>()))
			 .ReturnsAsync((User)null!);

		var result = await _userService.GetUserByFirebaseUidAsync(firebaseUid);

		Assert.Null(result);
	}

	//---------------------------------------------------------
	// Testes de Unidade para CreateOrUpdateAsync
	//---------------------------------------------------------

	[Fact]
	public async Task CreateOrUpdateAsync_ComNovoUid_DeveCriarNovoUsuario()
	{
		var firebaseUid = "novo_uid";
		var newEmail = "novo@email.com";
		var dto = new UserUpdateDto { Name = "Novo Usuario" };
		var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, It.IsAny<CancellationToken>()))
			 .ReturnsAsync((User)null!);
		_dbMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);
		_mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

		var result = await _userService.CreateOrUpdateAsync(firebaseUid, newEmail, dto);

		_dbMock.Verify(db => db.Users.Add(It.Is<User>(u => u.FirebaseUid == firebaseUid)), Times.Once);
		_dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
		Assert.NotNull(result);
	}

	[Fact]
	public async Task CreateOrUpdateAsync_ComUidExistenteEDto_DeveAtualizarUsuario()
	{
		var firebaseUid = "existing_uid";
		var newEmail = "new@email.com";
		var newName = "New Name";
		var dto = new UserUpdateDto { Name = newName };
		var existingUser = new User { FirebaseUid = firebaseUid, Email = "old@email.com", Name = "Old Name" };
		var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, It.IsAny<CancellationToken>()))
			 .ReturnsAsync(existingUser);
		_dbMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);
		_mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

		var result = await _userService.CreateOrUpdateAsync(firebaseUid, newEmail, dto);

		_dbMock.Verify(db => db.Users.Add(It.IsAny<User>()), Times.Never);
		_dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
		_mapperMock.Verify(m => m.Map(dto, existingUser), Times.Once);
		Assert.NotNull(result);
	}

	[Fact]
	public async Task CreateOrUpdateAsync_ComUidExistenteENenhumDto_DeveAtualizarApenasUpdatedAt()
	{
		var firebaseUid = "existing_uid";
		var existingEmail = "existing@email.com";
		var existingUser = new User
		{
			FirebaseUid = firebaseUid,
			Email = existingEmail,
			Name = "Existing User",
			CreatedAt = DateTime.UtcNow.AddDays(-10),
			UpdatedAt = DateTime.UtcNow.AddDays(-10)
		};
		var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, It.IsAny<CancellationToken>()))
			 .ReturnsAsync(existingUser);
		_dbMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);
		_mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

		var result = await _userService.CreateOrUpdateAsync(firebaseUid, existingEmail, null);

		Assert.NotNull(result);
		_dbMock.Verify(db => db.Users.Add(It.IsAny<User>()), Times.Never);
		_mapperMock.Verify(m => m.Map(It.IsAny<UserUpdateDto>(), It.IsAny<User>()), Times.Never);
		_dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
		transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task CreateOrUpdateAsync_ComFirebaseUidNulo_DeveLancarArgumentNullException()
	{
		string? firebaseUid = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.CreateOrUpdateAsync(firebaseUid!, "test@example.com", null));
		_dbMock.Verify(db => db.Users.Add(It.IsAny<User>()), Times.Never);
		_dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task CreateOrUpdateAsync_ComErroNoSave_DeveFazerRollbackEPropagarExcecao()
	{
		var firebaseUid = "novo_uid";
		var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
			 .ReturnsAsync((User)null!);
		_dbMock.Setup(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);
		_dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new DbUpdateException("Simulando um erro de DB."));

		await Assert.ThrowsAsync<DbUpdateException>(() => _userService.CreateOrUpdateAsync(firebaseUid, "test@example.com"));

		transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
		transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task CreateOrUpdateAsync_ComErroNaBuscaInicial_DevePropagarExcecao()
	{
		var firebaseUid = "uid";
		var expectedException = new InvalidOperationException("Simulando um erro na busca.");

		_dbMock.Setup(db => db.Users.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
			 .ThrowsAsync(expectedException);

		var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateOrUpdateAsync(firebaseUid, "test@email.com"));

		Assert.Equal(expectedException, thrownException);
		_dbMock.Verify(db => db.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
		_dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}