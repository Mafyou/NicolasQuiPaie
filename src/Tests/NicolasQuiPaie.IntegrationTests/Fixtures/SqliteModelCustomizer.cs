namespace NicolasQuiPaie.IntegrationTests.Fixtures;

/// <summary>
/// The API's model as it is, made readable by SQLite: the columns it types "nvarchar(max)" for SQL Server become TEXT.
/// </summary>
public class SqliteModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetProperties()))
        {
            if (string.Equals(property.GetColumnType(), "nvarchar(max)", StringComparison.OrdinalIgnoreCase))
            {
                property.SetColumnType("TEXT");
            }
        }
    }
}
