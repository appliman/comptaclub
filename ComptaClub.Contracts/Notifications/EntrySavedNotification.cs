namespace ComptaClub.Contracts.Notifications;

public record EntrySavedNotification : INotification
{
    public Guid EntryId { get; set; }
    public Guid ExerciceId { get; set; }
}
