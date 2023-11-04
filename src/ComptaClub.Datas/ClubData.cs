namespace ComptaClub.Datas;

[Table("Clubs")]
public class ClubData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Object { get; set; }
    public string? Address { get; set; }
    public string? SirenNumber { get; set; }
    public int CreationDate { get; set; }
    public string? SiretNumber { get; set; }
    public string? RNA { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? WebSite { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ContactName { get; set; }
	public string? LogoBase64String { get; set; }
    public string? LogoContentType { get; set; }

}
