using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RahalApi.Data;
using RahalApi.Models;

namespace RahalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly RahalDbContext _context;

    public PlacesController(RahalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Place>>> GetPlaces()
    {
        return await _context.Places.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Place>> GetPlace(int id)
    {
        Place? place = await _context.Places.FindAsync(id);

        if (place == null)
        {
            return NotFound();
        }

        return place;
    }

    [HttpPost]
    public async Task<ActionResult<Place>> CreatePlace(Place place)
    {
        _context.Places.Add(place);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPlace),
            new { id = place.Id },
            place
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlace(int id)
    {
        Place? place = await _context.Places.FindAsync(id);

        if (place == null)
        {
            return NotFound();
        }

        _context.Places.Remove(place);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}