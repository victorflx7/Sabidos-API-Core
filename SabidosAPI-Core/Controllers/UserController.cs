using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // 👈 importante

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        private readonly ILogger<UserController> _logger; // 👈 adiciona o logger

        // ✅ Injeta também o logger no construtor
        public UserController(UserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Rota Antiga: Retorna o perfil do usuário AUTENTICADO
        /// Se não existir no SQL, cria automaticamente (ainda usa JWT)
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            Console.WriteLine("C# TRACE 1: Requisição GET /user/me recebida. Token Firebase é válido.");
            var uid = User.FindFirst("user_id")?.Value
                   ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

            var email = User.FindFirst("email")?.Value
                     ?? User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(uid))
            {
                Console.WriteLine("C# TRACE 2: ERRO! Token válido, mas claim 'uid' não encontrada. Retornando 401.");
                _logger.LogWarning("Token JWT válido recebido, mas a claim 'user_id' (ou NameIdentifier/sub) não foi encontrada.");
                return Unauthorized("Claim de UID não encontrada no token.");
            }
            Console.WriteLine($"C# TRACE 2: UID extraído com sucesso: {uid}");

            var me = await _service.CreateOrUpdateAsync(uid, email);
            Console.WriteLine("C# TRACE 3: Dados do usuário processados e retornando 200 OK.");
            return Ok(me);
        }
        [HttpOptions("me")]
        [AllowAnonymous]
        public IActionResult OptionsMe()
        {
            // Não faz nada, apenas permite que o navegador receba as respostas CORS corretas.
            return Ok();
        }

        /// 🌟 NOVA ROTA: Sincroniza o usuário recebendo UID e Email do Frontend.
        /// Esta rota NÃO exige autorização (JWT)
        
        /// 🌟 NOVA ROTA: Sincroniza o usuário recebendo UID e Email do Frontend.
        /// Esta rota NÃO exige autorização (JWT)
        [HttpPost("sync")]
        [AllowAnonymous] // 🚨 ESTA É A CHAVE NO BACKEND
        public async Task<IActionResult> SyncUser([FromBody] UserSyncDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Chama o serviço existente usando os dados fornecidos pelo DTO
            var me = await _service.CreateOrUpdateAsync(
                dto.FirebaseUid,
                dto.Email,
                new UserUpdateDto { Name = dto.Name }
            );

            return Ok(me);
        }

        /// Atualiza perfil do usuário autenticado (Mantém [Authorize] para proteção)
        [HttpPost("profile")]
        [Authorize]
        public async Task<IActionResult> UpsertProfile([FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var uid = User.FindFirst("user_id")?.Value;
            var email = User.FindFirst("email")?.Value;

            if (uid is null) return Unauthorized();

            var me = await _service.CreateOrUpdateAsync(uid, email, dto);

            return Ok(me);
        }
    }
}
