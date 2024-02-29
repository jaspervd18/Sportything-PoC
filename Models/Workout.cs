namespace Internship.BlazorServerPOC.Models;

public class Workout
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime Date { get; set; } = DateTime.Now;
    public ActivityType ActivityType { get; set; } = default!;
    public double Distance { get; set; } = default!;
    public int LikesCount { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public List<string> UsersLiked { get; set; } = new List<string>();
    public bool IsPrivate { get; set; } = default!;
}
