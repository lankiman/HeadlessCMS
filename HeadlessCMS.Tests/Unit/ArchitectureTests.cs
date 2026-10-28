using System.Reflection;

namespace HeadlessCMS.Tests.Unit;

/// <summary>
/// Keeps the layers honest: API → Services → Data → Common, and never the other way.
/// An assembly lists the projects its code actually uses, so a forbidden dependency shows up here.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly Common = typeof(HeadlessCMS.Common.Enums.EventType).Assembly;
    private static readonly Assembly Data = typeof(HeadlessCMS.Data.ApplicationDbContext).Assembly;
    private static readonly Assembly Services = typeof(HeadlessCMS.Services.ServiceCollectionExtensions).Assembly;
    private static readonly Assembly Api = typeof(HeadlessCMS.API.Extensions.LoggingExtensions).Assembly;

    private static string[] ProjectsUsedBy(Assembly assembly)
        => assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("HeadlessCMS.", StringComparison.Ordinal))
            .ToArray();

    [Fact]
    public void Common_uses_no_other_project()
        => Assert.Empty(ProjectsUsedBy(Common));

    [Fact]
    public void Data_uses_only_Common()
        => Assert.Empty(ProjectsUsedBy(Data).Except(["HeadlessCMS.Common"]));

    [Fact]
    public void Services_use_only_Data_and_Common()
        => Assert.Empty(ProjectsUsedBy(Services).Except(["HeadlessCMS.Data", "HeadlessCMS.Common"]));

    [Fact]
    public void Api_uses_only_Services_and_Common()
        => Assert.Empty(ProjectsUsedBy(Api).Except(["HeadlessCMS.Services", "HeadlessCMS.Common"]));
}
