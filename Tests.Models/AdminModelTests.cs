using Helpers;
using migrations;
using view.Pages.Admin;
using Xunit;

namespace PageTests;

public class AdminModelTests {
    private AdminModel GetModel() {
        return new AdminModel {
            PageContext = Setup.GetPageContext(),
            MetadataProvider = Setup.MetadataProvider()
        };
    }

    [Fact]
    public async Task ShouldShowMigrationVersion() {
        await Setup.ResetAndGetDatabase(false);
        var model = GetModel();
        Assert.Null(model.LatestVersion);

        await model.OnPostMigrate();
        var expected = (int)MigrationInfo.GetLatestVersion();
        Assert.Equal(expected, model.LatestVersion?.Version);
    }
}