using LALALALALLA.Services;
using Microsoft.AspNetCore.Mvc;

namespace LALALALALLA.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsControllers(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTripDetails()
    {
        return Ok(await service.GetTripDetailsAsync());
    }

}