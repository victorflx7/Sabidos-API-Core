using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabidosAPI_Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class PostsController : ControllerBase
{
    private readonly ResumoService _service;

    public PostsController(PostService service)
    {
        _service = service;
    }

    
    [HttpGet]
    public async Task<ActionResult<List<ResumoResponseDto>>> GetAll([FromQuery] bool onlyMine = false)
    {
        var userId = User.FindFirst("user_id")?.Value;

        string? filterUserId = onlyMine ? userId : null;
        var resumo = await _service.GetAllPostsAsync(filterUserId);

        return Ok(resumo);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<ResumoResponseDto>> GetById(int id)
    {
        var resumo = await _service.ResumoByIdAsync(id);
        if (resumo == null) return NotFound();
        return Ok(resumo);
    }

   
    [HttpPost]
    public async Task<ActionResult<ResumoResponseDto>> Create([FromBody] ResumoCreateUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var uid = User.FindFirst("user_id")?.Value ?? "unknown";
        var name = User.FindFirst("name")?.Value ?? User.Identity?.Name ?? uid;

        var resumo = await _service.CreatePostAsync(uid, name, dto);
        return CreatedAtAction(nameof(GetById), new { id = resumo.Id }, post);
    }

    
    [HttpPut("{id}")]
    public async Task<ActionResult<ResumoResponseDto>> Update(int id, [FromBody] ResumoCreateUpdateDto dto)
    {
        var uid = User.FindFirst("user_id")?.Value ?? "unknown";

        var updatedResumo = await _service.UpdateResumoAsync(id, uid, dto);
        if (updatedResumo == null)
            return NotFound(new { message = "Post não encontrado ou você não tem permissão para atualizar." });

        return Ok(updatedResumo);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = User.FindFirst("user_id")?.Value ?? "unknown";

        var deleted = await _service.DeleteResumoAsync(id, uid);
        if (!deleted)
            return NotFound(new { message = "Post não encontrado ou você não tem permissão para deletar." });

        return NoContent();
    }
}


    

    
}
