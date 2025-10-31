namespace HyDrive.Api;

public class AppSettings
{
    public string StorageDirectory { get; set; } = null!;
    public string SqliteConnection { get; set; } = null!;
    public string DefaultConnection { get; set; } = null!;
}