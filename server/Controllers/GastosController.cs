using System.Security.Claims;
using Control_de_Gastos.Data;
using Control_de_Gastos.Models;
using Control_de_Gastos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Control_de_Gastos.Controllers;
[ApiController, Route("api/[controller]"), Authorize]
public sealed class GastosController(AppDbContext db, IGastoService totals, IConfiguration configuration) : ControllerBase
{
    private int OwnerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private IQueryable<Gasto> Mine => db.Gastos.Where(x => x.UserId == OwnerId);
    [HttpGet]
    public async Task<ActionResult<List<Gasto>>> List() => await Mine.AsNoTracking().ToListAsync();
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Gasto>> Get(int id)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        return entity is null ? NotFound() : entity;
    }
    [HttpPost]
    public async Task<ActionResult<Gasto>> Create(ExpenseInput input)
    {
        if (!await db.Categorias.AnyAsync(c => c.Id == input.CategoriaId && c.UserId == OwnerId))
            return BadRequest(new { message = "Selecciona una categoría de tu cuenta." });
        var entity = new Gasto { UserId = OwnerId };
        entity.Descripcion = input.Descripcion.Trim();
        entity.Monto = input.Monto; entity.Fecha = input.Fecha; entity.CategoriaId = input.CategoriaId;
        db.Gastos.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ExpenseInput input)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        if (!await db.Categorias.AnyAsync(c => c.Id == input.CategoriaId && c.UserId == OwnerId))
            return BadRequest(new { message = "Selecciona una categoría de tu cuenta." });
        entity.Descripcion = input.Descripcion.Trim();
        entity.Monto = input.Monto; entity.Fecha = input.Fecha; entity.CategoriaId = input.CategoriaId;
        await db.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await Mine.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        db.Gastos.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("total")]
    public async Task<IActionResult> Total() => Ok(new { aplicacion = configuration["AppSettings:NombreAplicacion"], totalGastos = totals.CalcularTotal(await Mine.ToListAsync()) });
}
