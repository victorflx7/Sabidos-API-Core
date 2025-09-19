using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        public UserController(UserService service) => _service = service;

        /// Retorna o perfil do usuário autenticado
        /// Se não existir no SQL, cria automaticamente
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var uid = User.FindFirst("user_id")?.Value;
            var email = User.FindFirst("email")?.Value;

            if (uid is null) return Unauthorized();

            // Se não existe, cria com dados mínimos
            var me = await _service.CreateOrUpdateAsync(uid, email);

            return Ok(me);
        }

        /// Atualiza perfil do usuário autenticado
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
