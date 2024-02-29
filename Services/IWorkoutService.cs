using Internship.BlazorServerPOC.Models;

namespace Internship.BlazorServerPOC.Services;

public interface IWorkoutService
{
    Task<IEnumerable<Workout>> AllAsync(ActivityType? activity);
    Task<IEnumerable<Workout>> AllFromUserAsync(string userName);
    Task CreateAsync(Workout workout, string userName);
    Task DeleteAsync(string workoutTitle);
    Task SetWorkoutPrivate(string workoutTitle);
    Task SetWorkoutPublic(string workoutTitle);
}
