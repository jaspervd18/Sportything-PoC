using Blazored.Toast.Services;
using Internship.BlazorServerPOC.Models;
using Internship.BlazorServerPOC.Services;
using Microsoft.AspNetCore.Components;

namespace Internship.BlazorServerPOC.Pages;
public partial class Profile
{
    private IEnumerable<Workout> Workouts { get; set; } = new List<Workout>();
    private IEnumerable<User> Users { get; set; } = new List<User>();
    private string User { get; set; } = string.Empty;
    [Inject] public IWorkoutService WorkoutService { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;
    [Inject] public IToastService ToastService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await FetchUsers();
    }

    private async Task FetchWorkouts(string user)
    {
        Workouts = await WorkoutService.AllFromUserAsync(user);
    }

    private async Task FetchUsers()
    {
        Users = await UserService.AllAsync();
    }

    private async void OnSelectUser(ChangeEventArgs e)
    {
        var user = e?.Value?.ToString();
        User = user!;
        if (string.IsNullOrEmpty(user))
        {
            Workouts = new List<Workout>();
            await InvokeAsync(StateHasChanged);
            return;
        }
        await FetchWorkouts(user);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnDeleteWorkout(string workoutTitle)
    {
        await WorkoutService.DeleteAsync(workoutTitle);
        ToastService.ShowSuccess("Activiteit succesvol verwijderd.");
        await FetchWorkouts(User);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnPrivateWorkout(string workoutTitle)
    {
        Workout workout = Workouts.FirstOrDefault(w => w.Title == workoutTitle)!;
        if (workout.IsPrivate)
        {
            await WorkoutService.SetWorkoutPublic(workoutTitle);
            ToastService.ShowSuccess("Activiteit succesvol openbaar gemaakt.");
        } else
        {
            await WorkoutService.SetWorkoutPrivate(workoutTitle);
            ToastService.ShowSuccess("Activiteit succesvol privé gemaakt.");

        }
        await FetchWorkouts(User);
        await InvokeAsync(StateHasChanged);
        return;
    }
}
