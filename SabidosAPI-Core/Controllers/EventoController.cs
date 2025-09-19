using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly EventoService _eventoService;

        public EventosController(EventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventoResponseDto>>> GetAllEventos()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (uid is null) { return Unauthorized(); }

            try
            {
                var eventos = await _eventoService.GetAllEventosAsync(uid);
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventoResponseDto>> GetEventoById(int id)
        {
            try
            {
                var evento = await _eventoService.GetEventosByIdAsync(id);

                if (evento == null)
                {
                    return NotFound($"Evento com ID {id} não encontrado.");
                }

                return Ok(evento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetEventosCountCountByUser()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (uid is null) { return Unauthorized(); }
            try
            {
                var count = await _eventoService.GetEventosCountByUserAsync(uid);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
       

        [HttpPost]
        public async Task<ActionResult<EventoResponseDto>> CreateEvento([FromBody] EventoResponseDto eventoDto)
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (uid is null) return Unauthorized();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdEvento = await _eventoService.CreateEventoAsync(eventoDto,uid);
                return CreatedAtAction(nameof(GetEventoById), new { id = createdEvento.Id }, createdEvento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EventoResponseDto>> UpdateEvento(int id, [FromBody] EventoResponseDto eventoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedEvento = await _eventoService.UpdateEventoAsync(id, eventoDto);

                if (updatedEvento == null)
                {
                    return NotFound($"Evento com ID {id} não encontrado.");
                }

                return Ok(updatedEvento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            try
            {
                var result = await _eventoService.DeleteEventoAsync(id);

                if (!result)
                {
                    return NotFound($"Evento com ID {id} não encontrado.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
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
    }
}