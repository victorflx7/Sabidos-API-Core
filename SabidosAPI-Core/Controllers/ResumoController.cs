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

    

    
}
