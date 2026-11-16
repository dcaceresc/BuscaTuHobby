internal static class MigrationMetadata
{
    // Offline, bounded alternative to dotnet-ef for freezing the manually prepared snapshot.
    // No arguments reproduce the original IrionOffer designer; pass "<id>" "<class>" only for a newer migration.
    public static void Prepare(string id = "20260720000000_IrionOffer", string className = "IrionOffer")
    {
        const string directory = "src/Infrastructure/Data/Migrations/";
        var output = directory + id + ".Designer.cs";
        var snapshot = File.ReadAllText(directory + "ApplicationDbContextModelSnapshot.cs");
        var designer = snapshot
            .Replace("using Microsoft.EntityFrameworkCore.Metadata;", "using Microsoft.EntityFrameworkCore.Metadata;\nusing Microsoft.EntityFrameworkCore.Migrations;")
            .Replace("    partial class ApplicationDbContextModelSnapshot : ModelSnapshot", $"    [Migration(\"{id}\")]\n    partial class {className}")
            .Replace("protected override void BuildModel(", "protected override void BuildTargetModel(");
        if (!designer.Contains("partial class " + className) || designer.Contains(": ModelSnapshot"))
            throw new InvalidOperationException("Unexpected snapshot shape.");
        if (File.Exists(output) && File.ReadAllText(output) != designer)
            throw new InvalidOperationException("Refusing to overwrite an existing different migration target.");
        File.WriteAllText(output, designer);
        Console.WriteLine($"Prepared frozen {className} migration metadata from the manually updated snapshot; no database used.");
    }
}
