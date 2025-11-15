using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.Dtos;
using SabidosAPI_Core.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SabidosAPI_Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PomodoroController : ControllerBase
    {
        // Agora depende da interface
        private readonly IPomodoroService _service;

        public PomodoroController(IPomodoroService service) // Injeção de dependência da interface
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<PomoResponseDto>>> GetAll()
        {
            var uid = User.FindFirst("user_id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            var result = await _service.GetAllAsync(uid);
            return Ok(result);
        }

        [HttpGet("count-time")]
        public async Task<ActionResult<int>> CountTime([FromQuery] string firebaseUid)
        {
            if (string.IsNullOrWhiteSpace(firebaseUid))
                return BadRequest("Firebase UID é obrigatório");

            var total = await _service.CountTimeAsync(firebaseUid);
            return Ok(total);
        }

        [HttpPost]
        public async Task<ActionResult<PomoResponseDto>> Create([FromBody] PomoCreateRequestDto request)
        {
            var uid = request.FirebaseUid;
            if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

            var created = await _service.CreateAsync(request.PomodoroData, uid); 
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        
        public class PomoCreateRequestDto
        {
            [Required]
            public string FirebaseUid { get; set; } = string.Empty;

            [Required]
            public PomoCreateDto PomodoroData { get; set; } = new();
        }
    }
}