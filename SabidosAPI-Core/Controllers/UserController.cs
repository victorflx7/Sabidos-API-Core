using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;
using Microsoft.Extensions.Logging;


namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;

        private readonly ILogger<UserController> _logger;

        public UserController(UserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // 🔐 NOVA ROTA: Validação de login (sem JWT)
        [HttpPost("validate-login")]
        public async Task<IActionResult> ValidateLogin([FromBody] LoginValidationDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                // Verifica se o usuário existe no SQL
                var userExists = await _service.UserExistsAsync(dto.FirebaseUid);
                
                if (!userExists)
                {
                    _logger.LogWarning("Tentativa de login com UID não cadastrado: {FirebaseUid}", dto.FirebaseUid);
                    return Unauthorized(new { message = "Usuário não cadastrado no sistema." });
                }

                // Busca dados completos do usuário
                var user = await _service.GetUserByFirebaseUidAsync(dto.FirebaseUid);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Erro ao recuperar dados do usuário." });
                }


                _logger.LogInformation("Login validado com sucesso para: {FirebaseUid}", dto.FirebaseUid);
                return Ok(new { 
                    success = true, 
                    user = user,
                    message = "Login validado com sucesso." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar login para: {FirebaseUid}", dto.FirebaseUid);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }


        // ✅ Mantido: Sincronização (usado no cadastro)
        [HttpPost("sync")]
        public async Task<IActionResult> SyncUser([FromBody] UserSyncDto dto)

        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var me = await _service.CreateOrUpdateAsync(
                    dto.FirebaseUid,
                    dto.Email,
                    new UserUpdateDto { Name = dto.Name }
                );

                _logger.LogInformation("Usuário sincronizado: {FirebaseUid}", dto.FirebaseUid);
                return Ok(new { 
                    success = true, 
                    user = me,
                    message = "Usuário sincronizado com sucesso." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar usuário: {FirebaseUid}", dto.FirebaseUid);
                return StatusCode(500, new { message = "Erro ao sincronizar usuário." });
            }
        }

        // 🔍 Rota para verificar saúde do serviço
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "API User está funcionando", timestamp = DateTime.UtcNow });
        }
    }
}