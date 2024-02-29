using Microsoft.AspNetCore.Mvc;
using Internship.BlazorServerPOC.Services;
using Microsoft.AspNetCore.Authorization;

namespace Internship.BlazorServerPOC.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class WorkoutController : ControllerBase
{
    /// Get a list of workouts
    /// Returns a task that represents the asynchronous operation.
    /// The task result contains http result.
    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var workoutService = new WorkoutService();
        var workouts = await workoutService.AllAsync();

        return Ok(workouts);
    }

}
