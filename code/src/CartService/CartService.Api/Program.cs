using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MicroCommerce.BuildingBlocks.ServiceDefaults;
using MicroCommerce.CartService.Api.Endpoints;
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

app.UseDefault();

app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapCarts();

app.MapPost("/login", (string username, string password) =>
{
    if (username != "admin" || password != "password")
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    return Results.SignIn(new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme)), authenticationScheme: JwtBearerDefaults.AuthenticationScheme);
});

await app.RunAsync();
