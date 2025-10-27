using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.Services;
using SabidosAPI_Core.Dtos;

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PomodoroController : ControllerBase
    {
        private readonly PomodoroService _service;

        public PomodoroController(PomodoroService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PomoResponseDto>>> GetAll()
        {
            var uid = User.FindFirst("user_id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            // CORRIGIDO: Checagem robusta de autorização
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            var result = await _service.GetAllAsync(uid);
            return Ok(result);
        }

        [HttpGet("count-time")]
        public async Task<ActionResult<int>> CountTime()
        {
            var uid = User.FindFirst("user_id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            // CORRIGIDO: Checagem robusta de autorização
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            var total = await _service.CountTimeAsync(uid);
            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<PomoResponseDto>> Create([FromBody] PomoCreateDto dto)
        {
            var uid = User.FindFirst("user_id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            // CORRIGIDO: Checagem robusta de autorização
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            var created = await _service.CreateAsync(dto, uid);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }
    }
}