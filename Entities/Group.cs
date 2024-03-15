using System.ComponentModel.DataAnnotations;

namespace API.Entites;

public class Group
{
    public Group(string name)
    {
        Name = name;
    }
    public Group() { }
    [Key]
    public string Name { get; set; }



    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
}
