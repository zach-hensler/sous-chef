using core;
using core.Data;
using core.Models.ServiceModels;
using Helpers;
using services;
using Xunit;

namespace DataTests;

public class ListRecipesDataTests: Sequential {
    [Fact]
    public async Task ShouldListWithoutFilters() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var request1 = Rand.Domain.Requests.CreateRecipeRequest();
        request1.Recipe.Category = Categories.Entree;
        await RecipeService.CreateRecipe(request1);
        var request2 = Rand.Domain.Requests.CreateRecipeRequest();
        request2.Recipe.Category = Categories.Side;
        await RecipeService.CreateRecipe(request2);

        var list = await ListRecipesData.Get(new ListRecipesRequest(), conn);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task ShouldFilterByCategory() {
        await using var conn = await Setup.ResetAndGetDatabase();

        var request1 = Rand.Domain.Requests.CreateRecipeRequest();
        request1.Recipe.Category = Categories.Entree;
        await RecipeService.CreateRecipe(request1);
        var request2 = Rand.Domain.Requests.CreateRecipeRequest();
        request2.Recipe.Category = Categories.Side;
        await RecipeService.CreateRecipe(request2);
        var request3 = Rand.Domain.Requests.CreateRecipeRequest();
        request3.Recipe.Category = Categories.Side;
        await RecipeService.CreateRecipe(request3);

        var entreeList = await ListRecipesData.Get(new ListRecipesRequest { CategoryFilter = Categories.Entree }, conn);
        Assert.Single(entreeList);
    
        var sideList = await ListRecipesData.Get(new ListRecipesRequest { CategoryFilter = Categories.Side }, conn);
        Assert.Equal(2, sideList.Count);
    }
}