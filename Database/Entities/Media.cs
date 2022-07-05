namespace ReprodutorMultimia.Database.Entities;

public class Media
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string FileName { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public string? Album { get; set; }
    public string? Title { get; set; }
    public int Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? ThumbName { get; set; }
}