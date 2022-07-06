namespace ReprodutorMultimia.Models;

public class Media
{
    public const string TypeMusic = "music";
    public const string TypeVideo = "video";

    public int Id { get; set; }
    public string Name { get; set; }
    public string Src { get; set; }
    public string Type { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public string? Album { get; set; }
    public string? Title { get; set; }
    public float? Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Thumbnail { get; set; }
}