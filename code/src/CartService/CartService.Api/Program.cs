using MicroCommerce.BuildingBlocks.ServiceDefaults;
using MicroCommerce.CartService.Application;
using MicroCommerce.CartService.Infrastructure;
using MicroCommerce.CartService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplication();
builder.AddInfrastructure();

builder.Services.AddCors(options => options.AddDefaultPolicy(
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    )
);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost";
        options.Audience = "https://localhost";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = false,
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    //await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseCors();

app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefault();
app.MapDefaultEndpoints();

app.MapGet("/api/carts", () => new[]
{
    new { Id = 1, Name = "carts 1" },
    new { Id = 2, Name = "carts 2" },
    new { Id = 3, Name = "carts 3" },
});

await app.RunAsync();
