using core;using program;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// TODO set connection string elsewhere
Environment.SetEnvironmentVariable(
    EnvironmentVariables.ConnectionString, "User ID=user;Password=pass;Host=localhost;Port=5432;Database=sous-chef;");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

new Controllers(app).Register();

app.Run();