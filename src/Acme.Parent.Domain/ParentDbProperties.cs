namespace Acme.Parent;

public static class ParentDbProperties
{
    public static string DbTablePrefix { get; set; } = "Parent";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Parent";
}
