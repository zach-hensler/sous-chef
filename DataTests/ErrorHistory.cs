using core.Data;
using core.Models;
using core.Models.DbModels;
using Helpers;
using Xunit;

namespace DataTests;

public class ErrorHistory: Sequential {
    [Fact]
    public async Task ShouldAddErrorsToHistory() {
        await using var conn = (await Setup.ResetAndGetDatabase()).GetConnection();
        await conn.OpenAsync(TestContext.Current.CancellationToken);

        List<ErrorHistoryDb> errors = [];
        for (var i = 0; i < Rand.Primitive.Int(5, 10); i ++) {
            errors.Add(Rand.Domain.Db.ErrorHistoryDb());
            await Common.ErrorHistory.Add(errors.Last(), conn);
        }

        var listedErrors = await Common.ErrorHistory.List(0, 20, conn);
        Assert.Equal(errors.Count, listedErrors.Count);
        foreach (var error in listedErrors) {
            Assert.Contains(error, errors);
        }
    }
}