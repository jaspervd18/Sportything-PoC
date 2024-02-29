using Internship.BlazorServerPOC.Models;
using Internship.BlazorServerPOC.Services;
using Microsoft.AspNetCore.Components;

namespace Internship.BlazorServerPOC.Pages;
public partial class WorkoutOverview
{
    private IEnumerable<Workout> Workouts { get; set; } = new List<Workout>();
    private IEnumerable<User> Users { get; set; } = new List<User>();

    private string User { get; set; } = string.Empty;
    private ActivityType? SelectedActivityType { get; set; } = default!;

    [Inject] public IWorkoutService WorkoutService { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await FetchWorkouts();
        await FetchUsers();
    }

    private async Task FetchWorkouts()
    {
        Workouts = await WorkoutService.AllAsync(SelectedActivityType);
        Console.WriteLine(Workouts.ToList().Count());
    }

    private async Task FetchUsers()
    {
        Users = await UserService.AllAsync();
    }

    private async void OnSelectUser(ChangeEventArgs e)
    {
        var user = e?.Value?.ToString();
        User = user!;
        if (!string.IsNullOrEmpty(user))
        {
            await FetchWorkouts();
        }
        await InvokeAsync(StateHasChanged);
        return;
    }

    private async void OnFilterActivityType(ChangeEventArgs e)
    {
        var activityType = e?.Value?.ToString();
        if (!string.IsNullOrEmpty(activityType))
        {
            SelectedActivityType = Enum.Parse<ActivityType>(activityType, true);
        } else
        {
            SelectedActivityType = null;
        }
        await FetchWorkouts();
        await InvokeAsync(StateHasChanged);
        return;
    }

    private async Task LikePost(string workoutTitle)
    {
        if (string.IsNullOrEmpty(User))
        {
            return;
        }
        var liked = await UserService.CheckWorkoutUserLikeRelationAsync(workoutTitle, User);
        if (liked) {
            return;
        }
        await UserService.SetWorkoutUserLikeRelationAsync(workoutTitle, User);
        await FetchWorkouts();
        await InvokeAsync(StateHasChanged);
    }
}
