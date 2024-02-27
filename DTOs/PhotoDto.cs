namespace API;

public class PhotoDto
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool isMain { get; set; }
    public string PublicId { get; internal set; }
}
