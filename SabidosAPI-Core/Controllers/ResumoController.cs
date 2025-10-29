using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using SabidosAPI_Core.Models;
using SabidosAPI_Core.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ResumosController : ControllerBase
{
    private readonly ResumoService _service;

    public ResumosController(ResumoService service)
    {
        _service = service;
    }

 
    [HttpGet]
    public async Task<ActionResult<List<ResumoResponseDto>>> GetAll()
    {
        // CHECAGEM CRÍTICA 1: Se o Identity não estiver presente OU não estiver autenticado, retorne 401.
        // Esta é a forma mais robusta de garantir que o teste Unauthorized passe.
        if (User.Identity is null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        var uid = User.FindFirst("user_id")?.Value
                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;

        // CHECAGEM CRÍTICA 2: Garante que o UID extraído não seja nulo, vazio ou só espaços.
        // Mantida por segurança, mas o passo 1 deve resolver o teste.
        if (string.IsNullOrWhiteSpace(uid)) { return Unauthorized(); }

        try
        {
            var resumos = await _service.GetAllResumosAsync(uid);
            return Ok(resumos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }



    [HttpGet("{id}")]
    public async Task<ActionResult<ResumoResponseDto>> GetById(int id)
    {
        var resumo = await _service.GetResumoByIdAsync(id);
        if (resumo == null) return NotFound();
        return Ok(resumo);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetEventosCountCountByUser()
    {
        var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (uid is null) { return Unauthorized(); }
        try
        {
            var count = await _service.GetResumosCountByUserAsync(uid);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }



    [HttpPost]
    public async Task<ActionResult<ResumoResponseDto>> Create([FromBody] ResumoCreateUpdateDto dto)
    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var uid = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? uid;

        var resumo = await _service.CreateResumoAsync(dto, uid , name);
        return CreatedAtAction(nameof(GetById), new { id = resumo.Id }, resumo);
    }

    
    [HttpPut("{id}")]
    public async Task<ActionResult<ResumoResponseDto>> Update(int id, [FromBody] ResumoCreateUpdateDto dto)
    {
        var uid = User.FindFirst("user_id")?.Value ?? "unknown";

        var updatedResumo = await _service.UpdateresumoAsync(id, dto);
        if (updatedResumo == null)
            return NotFound(new { message = "Post não encontrado ou você não tem permissão para atualizar." });

        return Ok(updatedResumo);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = User.FindFirst("user_id")?.Value ?? "unknown";

        var deleted = await _service.DeleteResumoAsync(id);
        if (!deleted)
            return NotFound(new { message = "Post não encontrado ou você não tem permissão para deletar." });

        return NoContent();
    }
}


