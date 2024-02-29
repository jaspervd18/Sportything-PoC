using Blazored.Toast.Services;
using Internship.BlazorServerPOC.Models;
using Internship.BlazorServerPOC.Services;
using Microsoft.AspNetCore.Components;

namespace Internship.BlazorServerPOC.Pages;
public partial class WorkoutForm
{
    public Workout Workout { get; set; } = new Workout();
    private IEnumerable<User> Users { get; set; } = new List<User>();
    public string User { get; set; } = default!;
    [Inject] public IWorkoutService WorkoutService { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;
    [Inject] public IToastService ToastService { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await FetchUsers();
    }
    private async Task FetchUsers()
    {
        Users = await UserService.AllAsync();
    }

    private async void Submit()
    {
        await WorkoutService.CreateAsync(Workout, User);
        ToastService.ShowSuccess("Activiteit succesvol toegevoegd.");
        Workout = new Workout();
        await InvokeAsync(StateHasChanged);
        NavigationManager.NavigateTo("/");
    }
}
