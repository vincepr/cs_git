namespace CS_Git.Lib.GitObjectLogic.Models;

public record GitPersonInfo
{
    public required PersonType Type { get; set; }
    public required string Name { get; set; }
    public required string Mail { get; set; }
    public required uint Seconds { get; set; }
    public required string TimeZone { get; set; }
    
    public enum PersonType
    {
        author,
        committer,
    }

    public override string ToString() => $"{Name} <{Mail}> {Seconds} {TimeZone}";
}