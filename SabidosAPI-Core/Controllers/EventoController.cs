using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Services;
using Microsoft.AspNetCore.Authorization; // ADICIONADO: Necessário para [Authorize]
using System; // ADICIONADO: Necessário para Exception
using System.Collections.Generic; // ADICIONADO: Necessário para IEnumerable
using System.Threading.Tasks; // ADICIONADO: Necessário para Task

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔑 CORREÇÃO: Adiciona autorização para todo o controller
    public class EventosController : ControllerBase
    {
        // CORREÇÃO: Usar a interface IEventoService para melhor testabilidade (opcional, mas recomendado)
        // Se você ainda não criou a interface, pode manter EventoService por agora.
        private readonly EventoService _eventoService;

        public EventosController(EventoService eventoService) // Injete IEventoService se tiver criado
        {
            _eventoService = eventoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventoResponseDto>>> GetAllEventos()
        {
            // 🔑 CORREÇÃO: Checagem de UID robusta
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            try
            {
                var eventos = await _eventoService.GetAllEventosAsync(uid);
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                // Logar a exceção é uma boa prática
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventoResponseDto>> GetEventoById(int id)
        {
            // 🔑 CORREÇÃO: Adicionar checagem de UID que estava faltando
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            try
            {
                var evento = await _eventoService.GetEventosByIdAsync(id);

                if (evento == null)
                {
                    return NotFound($"Evento com ID {id} não encontrado.");
                }

                // OPCIONAL: Verificar se o evento pertence ao usuário (uid)
                // if (evento.AuthorUid != uid) return Forbid(); // Ou NotFound()

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
            // 🔑 CORREÇÃO: Checagem de UID robusta
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

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
        // 🔑 CORREÇÃO: Receber EventoCreateDto, não EventoResponseDto
        public async Task<ActionResult<EventoResponseDto>> CreateEvento([FromBody] EventoCreateDto dto)
        {
            // 🔑 CORREÇÃO: Checagem de UID robusta
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) return Unauthorized();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // 🔑 CORREÇÃO: Passar o DTO correto para o serviço
                // Assumindo que o serviço foi atualizado para aceitar EventoCreateDto
                // Se o serviço ainda espera EventoResponseDto, você precisará mapear aqui ou ajustar o serviço.
                // Vou assumir que o serviço foi ajustado ou que o mapeamento funciona de CreateDto -> Model.

                // Ajuste aqui se o seu serviço espera EventoResponseDto
                // var eventoParaCriar = _mapper.Map<EventoResponseDto>(dto); // Exemplo se precisar mapear
                // var createdEvento = await _eventoService.CreateEventoAsync(eventoParaCriar, uid); 

                // Assumindo que o serviço aceita o CreateDto ou mapeia internamente:
                var createdEvento = await _eventoService.CreateEventoAsync(dto, uid); // Passa o EventoCreateDto

                return CreatedAtAction(nameof(GetEventoById), new { id = createdEvento.Id }, createdEvento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        // 🔑 CORREÇÃO: Idealmente, Update usaria um DTO específico (EventoUpdateDto)
        // Mantendo EventoResponseDto por enquanto, mas adicionando checagem de UID
        public async Task<ActionResult<EventoResponseDto>> UpdateEvento(int id, [FromBody] EventoResponseDto eventoDto)
        {
            // 🔑 CORREÇÃO: Adicionar checagem de UID
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // OPCIONAL: Antes de atualizar, verificar se o evento pertence ao usuário
                // var eventoExistente = await _eventoService.GetEventosByIdAsync(id);
                // if (eventoExistente == null) return NotFound();
                // if (eventoExistente.AuthorUid != uid) return Forbid(); // Ou NotFound()

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
            // 🔑 CORREÇÃO: Adicionar checagem de UID
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            try
            {
                // OPCIONAL: Verificar permissão antes de deletar
                // var eventoExistente = await _eventoService.GetEventosByIdAsync(id);
                // if (eventoExistente == null) return NotFound();
                // if (eventoExistente.AuthorUid != uid) return Forbid(); // Ou NotFound()

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