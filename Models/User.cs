namespace Internship.BlazorServerPOC.Models;

public class User
{
    public string Name { get; set; } = default!;
    public string Gender { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default!;
    public string Location{ get; set; } = default!;
    public IEnumerable<Workout> Workouts { get; set; } = default!;
}
