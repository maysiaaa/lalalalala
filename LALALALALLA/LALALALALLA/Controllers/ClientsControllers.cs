using LALALALALLA.Exceptions;
using LALALALALLA.Models.DTOs;
using LALALALALLA.Services;
using Microsoft.AspNetCore.Mvc;

namespace LALALALALLA.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientsControllers(IDbService service) : ControllerBase
{
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTripDetails([FromRoute] int id)
    {
        try
        {
            return Ok(await service.GetClientTripDetailsAsync(id));  
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO body)
    {
        try
        {
            var clientId = await service.CreateClientAsync(body);
            return Ok(clientId);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClient(
        [FromRoute] int clientId,
        [FromRoute] int tripId 
    )
    {
        try
        {
            await service.RegisterClientAsync(clientId, tripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}