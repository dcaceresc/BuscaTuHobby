internal static class MigrationMetadata
{
    // Offline, bounded alternative to dotnet-ef for freezing the manually prepared snapshot.
    public static void Prepare()
    {
        const string directory = "src/Infrastructure/Data/Migrations/";
        const string output = directory + "20260720000000_IrionOffer.Designer.cs";
        var snapshot = File.ReadAllText(directory + "ApplicationDbContextModelSnapshot.cs");
        var designer = snapshot
            .Replace("using Microsoft.EntityFrameworkCore.Metadata;", "using Microsoft.EntityFrameworkCore.Metadata;\nusing Microsoft.EntityFrameworkCore.Migrations;")
            .Replace("    partial class ApplicationDbContextModelSnapshot : ModelSnapshot", "    [Migration(\"20260720000000_IrionOffer\")]\n    partial class IrionOffer")
            .Replace("protected override void BuildModel(", "protected override void BuildTargetModel(");
        if (!designer.Contains("partial class IrionOffer") || designer.Contains(": ModelSnapshot"))
            throw new InvalidOperationException("Unexpected snapshot shape.");
        if (File.Exists(output) && File.ReadAllText(output) != designer)
            throw new InvalidOperationException("Refusing to overwrite an existing different migration target.");
        File.WriteAllText(output, designer);
        Console.WriteLine("Prepared frozen IrionOffer migration metadata from the manually updated snapshot; no database used.");
    }
}
