using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core_Web.Models.Security;
using Core_Web.Models;
using Core_Web.Data;

[Route("api/[controller]")]
[ApiController]
public class AppRoutesController : ControllerBase
{
    private readonly CoreContext _context;
    public AppRoutesController(CoreContext context)
    {
        _context = context;
    }

    // GET: api/AppRoute
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppRoute>>> GetAppRoute()
    {
        return await _context.Routes.ToListAsync();
    }

    // GET: api/AppRoute/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AppRoute>> GetAppRoute(long id)
    {
        var approute = await _context.Routes.FindAsync(id);

        if (approute == null)
        {
            return NotFound();
        }

        return approute;
    }

    // PUT: api/AppRoute/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAppRoute(long? id, AppRoute approute)
    {
        if (id != approute.Id)
        {
            return BadRequest();
        }

        _context.Entry(approute).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AppRouteExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/AppRoute
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<AppRoute>> PostAppRoute(AppRoute approute)
    {
        _context.Routes.Add(approute);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetAppRoute", new { id = approute.Id }, approute);
    }

    // DELETE: api/AppRoute/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppRoute(long? id)
    {
        var approute = await _context.Routes.FindAsync(id);
        if (approute == null)
        {
            return NotFound();
        }

        _context.Routes.Remove(approute);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool AppRouteExists(long? id)
    {
        return _context.Routes.Any(e => e.Id == id);
    }
}
