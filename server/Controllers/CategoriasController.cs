using System.Security.Claims;
using Control_de_Gastos.Data;
using Control_de_Gastos.Models;
using Control_de_Gastos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Control_de_Gastos.Controllers;
[ApiController, Route("api/[controller]"), Authorize]
public sealed class CategoriasController(AppDbContext db) : ControllerBase
{
    private int OwnerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private IQueryable<Categoria> Mine => db.Categorias.Where(x => x.UserId == OwnerId);
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> List() => await Mine.AsNoTracking().ToListAsync();
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> Get(int id)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        return entity is null ? NotFound() : entity;
    }
    [HttpPost]
    public async Task<ActionResult<Categoria>> Create(CategoryInput input)
    {
        var id = 0;
        if (await Mine.AnyAsync(c => c.Nombre == input.Nombre.Trim() && c.Id != id))
            return Conflict(new { message = "Ya tienes una categoría con ese nombre." });
        var entity = new Categoria { UserId = OwnerId };
        entity.Descripcion = input.Descripcion.Trim();
        entity.Nombre = input.Nombre.Trim();
        db.Categorias.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryInput input)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        if (await Mine.AnyAsync(c => c.Nombre == input.Nombre.Trim() && c.Id != id))
            return Conflict(new { message = "Ya tienes una categoría con ese nombre." });
        entity.Descripcion = input.Descripcion.Trim();
        entity.Nombre = input.Nombre.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        if (await db.Gastos.AnyAsync(g => g.UserId == OwnerId && g.CategoriaId == id))
            return Conflict(new { message = "Esta categoría tiene gastos. Cámbialos de categoría antes de eliminarla." });
        db.Categorias.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
