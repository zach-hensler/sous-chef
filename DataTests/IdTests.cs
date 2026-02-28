using core.Models.DbModels;
using Xunit;

namespace DataTests;

public class IdTests {
    [Fact]
    public void ShouldInterpolateToString() {
        var id = new VersionId(12);
        Assert.Equal("12", $"{id}");
    }

    [Fact]
    public void ShouldEqual() {
        var id1 = new VersionId(4);
        var id2 = new VersionId(4);
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void ShouldNotEqual() {
        var id1 = new VersionId(53);
        var id2 = new VersionId(104);
        Assert.NotEqual(id1, id2);
    }
}