// Controllers/EventosController.cs
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;
        private readonly UserService _userService;
        private readonly ILogger<EventosController> _logger;

        public EventosController(IEventoService eventoService, UserService userService, ILogger<EventosController> logger)
        {
            _eventoService = eventoService;
            _userService = userService;
            _logger = logger;
        }
        // 📖 GET USER EVENTOS - Corrigido para usar POST
        [HttpPost("user")]
        public async Task<IActionResult> GetUserEventos([FromBody] UserRequestDto request)
        {
            if (string.IsNullOrEmpty(request.FirebaseUid))
                return BadRequest(new { success = false, message = "Firebase UID é obrigatório" });

            try
            {
                var eventos = await _eventoService.GetAllEventosAsync(request.FirebaseUid);
                return Ok(new { success = true, data = eventos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos do usuário: {FirebaseUid}", request.FirebaseUid);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // 📖 GET ALL - Pode filtrar por usuário (agora com POST)
        [HttpPost("list")]
        public async Task<IActionResult> GetAllEventos([FromBody] EventoListRequestDto request)
        {
            try
            {
                var eventos = await _eventoService.GetAllEventosAsync(request.FirebaseUid);
                return Ok(new { success = true, data = eventos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // 📖 GET BY ID (mantém GET pois não expõe UID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventoById(int id)
        {
            try
            {
                var evento = await _eventoService.GetEventoByIdAsync(id);
                
                if (evento == null)
                    return NotFound(new { success = false, message = "Evento não encontrado" });

                return Ok(new { success = true, data = evento });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar evento: {EventoId}", id);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // 🔢 COUNT por usuário (agora com POST)
        [HttpPost("count")]
        public async Task<IActionResult> GetEventosCountByUser([FromBody] UserRequestDto request)
        {
            if (string.IsNullOrEmpty(request.FirebaseUid))
                return BadRequest(new { success = false, message = "Firebase UID é obrigatório" });

            try
            {
                var count = await _eventoService.GetEventosCountByUserAsync(request.FirebaseUid);
                return Ok(new { success = true, data = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar eventos do usuário: {FirebaseUid}", request.FirebaseUid);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // ➕ CREATE - Já está correto (usa Body)
        [HttpPost]
        public async Task<IActionResult> CreateEvento([FromBody] EventoCreateRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var userExists = await _userService.UserExistsAsync(request.FirebaseUid);
                if (!userExists)
                    return Unauthorized(new { success = false, message = "Usuário não autorizado" });

                var evento = await _eventoService.CreateEventoAsync(request.EventoData, request.FirebaseUid);
                
                _logger.LogInformation("Novo evento criado por: {FirebaseUid}", request.FirebaseUid);
                return Ok(new { success = true, data = evento, message = "Evento criado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar evento para: {FirebaseUid}", request.FirebaseUid);
                return StatusCode(500, new { success = false, message = "Erro ao criar evento" });
            }
        }

        // ✏️ UPDATE - Já está correto (usa Body)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvento(int id, [FromBody] EventoUpdateRequestDto request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var evento = await _eventoService.UpdateEventoAsync(id, request.EventoData, request.FirebaseUid);
                
                if (evento == null)
                    return NotFound(new { success = false, message = "Evento não encontrado" });

                return Ok(new { success = true, data = evento, message = "Evento atualizado com sucesso" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Você não tem permissão para editar este evento" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar evento: {EventoId}", id);
                return StatusCode(500, new { success = false, message = "Erro ao atualizar evento" });
            }
        }

        // 🗑️ DELETE - Corrigido para usar Body
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id, [FromBody] UserRequestDto request)
        {
            try
            {
                var result = await _eventoService.DeleteEventoAsync(id, request.FirebaseUid);
                
                if (!result)
                    return NotFound(new { success = false, message = "Evento não encontrado" });

                return Ok(new { success = true, message = "Evento excluído com sucesso" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Você não tem permissão para excluir este evento" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir evento: {EventoId}", id);
                return StatusCode(500, new { success = false, message = "Erro ao excluir evento" });
            }
        }

        // 📋 GET USER EVENTOS - Corrigido para usar Body
        public class UserRequestDto
        {
            [Required]
            public string FirebaseUid { get; set; } = string.Empty;
        }

        // 📅 GET EVENTOS POR RANGE DE DATA - Corrigido
        [HttpPost("range")]
        public async Task<IActionResult> GetEventosByDateRange([FromBody] EventoRangeRequestDto request)
        {
            try
            {
                var eventos = await _eventoService.GetEventosByDateRangeAsync(
                    request.StartDate, 
                    request.EndDate, 
                    request.FirebaseUid);
                
                return Ok(new { success = true, data = eventos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos por range de data");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // 🔮 GET PRÓXIMOS EVENTOS - Corrigido
        [HttpPost("upcoming")]
        public async Task<IActionResult> GetUpcomingEventos([FromBody] EventoUpcomingRequestDto request)
        {
            try
            {
                var eventos = await _eventoService.GetUpcomingEventosAsync(
                    request.Days, 
                    request.FirebaseUid);
                
                return Ok(new { success = true, data = eventos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar próximos eventos");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        // 🔍 VERIFICAR SE EVENTO PERTENCE AO USUÁRIO - Corrigido
        [HttpPost("{id}/belongs-to")]
        public async Task<IActionResult> EventoBelongsToUser(int id, [FromBody] UserRequestDto request)
        {
            try
            {
                var belongs = await _eventoService.EventoBelongsToUserAsync(id, request.FirebaseUid);
                return Ok(new { success = true, data = belongs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar propriedade do evento: {EventoId}", id);
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }
    }

    // 🔐 NOVOS DTOs PARA REQUESTS SEGURAS

    public class EventoListRequestDto
    {
        public string? FirebaseUid { get; set; }
    }

    public class EventoRangeRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? FirebaseUid { get; set; }
    }

    public class EventoUpcomingRequestDto
    {
        public int Days { get; set; } = 7;
        public string? FirebaseUid { get; set; }
    }
}
        //[HttpGet("recent")]
        //public async Task<ActionResult<IEnumerable<EventoResponseDto>>> GetRecentEventos()
        //{
        //    var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        //    if (uid is null) { return Unauthorized(); }
        //    try
        //    {
        //        var eventos = await _eventoService.GetRecentEventosAsync(uid);
        //        return Ok(eventos);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        //    }
        //}
