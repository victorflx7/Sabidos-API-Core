using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using System; // ADICIONADO: Para usar Exception e DateTime

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
        public async Task<ActionResult<List<FlashcardResponseDto>>> GetAllFlashcardsByUser()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            // 🔑 CORREÇÃO CRÍTICA: Garante 401 (Unauthorized) se o token estiver faltando.
            
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
        [HttpGet("count")] // Rota correta para o teste GetAsync($"{Endpoint}/count")
        public async Task<ActionResult<int>> GetFlashcardsCountCountByUser()
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            // 🔑 CORREÇÃO CRÍTICA: Garante 401 (Unauthorized) se o token estiver faltando.
            
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


        // B. Corrija o método CreateFlashcard
        // ... (Método CreateFlashcard)
        [HttpPost]
        public async Task<ActionResult<FlashcardResponseDto>> CreateFlashcard([FromBody] FlashcardCreateUpdateDto dto)
        {
            var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? uid;

            // 🔑 CORREÇÃO CRÍTICA: Adiciona o check de 401 que estava faltando.
           
            if (uid is null) { return Unauthorized(); }

            var flashcard = await _service.CreateFlashcardAsync(dto, uid, name);
            return CreatedAtAction(nameof(GetFlashcardById), new { id = flashcard.Id }, flashcard);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlashcardResponseDto>> GetFlashcardById(int id)
        {
            try
            {
                var flashcard = await _service.GetFlashcardByIdAsync(id);

                if (flashcard == null)
                {
                    return NotFound($"Flashcard com ID {id} não encontrado."); // CORREÇÃO: De Evento para Flashcard
                }

                return Ok(flashcard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FlashcardResponseDto>> UpdateFlashcard(int id, [FromBody] FlashcardCreateUpdateDto dto)
        {
            var uid = User.FindFirst("user_id")?.Value ?? "unknown";

            var updatedFlashcard = await _service.UpdateFlashcardAsync(id, dto);
            if (updatedFlashcard == null)
                return NotFound(new { message = "Flashcard não encontrado ou você não tem permissão para atualizar." }); // CORREÇÃO: De Post para Flashcard

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
                    return NotFound($"Flashcard com ID {id} não encontrado."); // CORREÇÃO: De Evento para Flashcard
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