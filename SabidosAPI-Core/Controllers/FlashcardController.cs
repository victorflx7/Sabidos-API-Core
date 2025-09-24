using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;

namespace SabidosAPI_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlashcardController : ControllerBase
    {
        private readonly FlashcardService _service;
        public FlashcardController(FlashcardService flashcardService)
        {
            _service = flashcardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlashcardResponseDto>>> GetAllFlashcards()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (uid is null) { return Unauthorized(); }

            try
            {
                var flashcards = await _service.GetAllFlashcardsAsync(uid);
                return Ok(flashcards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlashcardResponseDto>> GetFlashcardById(int id)
        {
            try
            {
                var flashcard = await _service.GetFlashcardByIdAsync(id);

                if (flashcard == null)
                {
                    return NotFound($"Evento com ID {id} não encontrado.");
                }

                return Ok(flashcard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetFlashcardsCountCountByUser()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (uid is null) { return Unauthorized(); }
            try
            {
                var count = await _service.GetFlashcardsCountByUserAsync(uid);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<FlashcardResponseDto>> CreateFlashcard([FromBody] FlashcardCreateUpdateDto dto)
        {

            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? uid;

            var flashcard = await _service.CreateFlashcardAsync(dto, uid, name);
            return CreatedAtAction(nameof(GetFlashcardById), new { id = flashcard.Id }, flashcard);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<FlashcardResponseDto>> UpdateFlashcard(int id, [FromBody] FlashcardCreateUpdateDto dto)
        {
            var uid = User.FindFirst("user_id")?.Value ?? "unknown";

            var updatedFlashcard = await _service.UpdateFlashcardAsync(id, dto);
            if (updatedFlashcard == null)
                return NotFound(new { message = "Post não encontrado ou você não tem permissão para atualizar." });

            return Ok(updatedFlashcard);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            try
            {
                var result = await _service.DeleteFlashcardAsync(id);

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

    }
}
