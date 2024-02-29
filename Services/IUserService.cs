using Internship.BlazorServerPOC.Models;

namespace Internship.BlazorServerPOC.Services;

public interface IUserService
{
    Task<IEnumerable<User>> AllAsync();
    Task<bool> SetWorkoutUserLikeRelationAsync(string workoutTitle, string userName);
    Task<bool> CheckWorkoutUserLikeRelationAsync(string workoutTitle, string userName);
}
