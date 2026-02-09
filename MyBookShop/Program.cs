using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyBookShop.Data.Context;
using MyBookShop.Models.Identity;
using MyBookShop.Services.Auth;
using MyBookShop.Services.CheckOut;
using MyBookShop.Services.Gateway.Zarinpal;
using MyBookShop.Services.Media;
using MyBookShop.Services.Payment;
using MyBookShop.Services.SeedData;
using Scalar.AspNetCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region DbContext

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Identity

builder.Services.AddIdentity<MyApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

#endregion

#region JwtAuthentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    }
    );

#endregion

builder.Services.AddAuthorization();

#region CustomServices

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<ICheckoutService, CheckoutService>();

builder.Services.AddScoped<IImageService, ImageService>();

#endregion

#region Zarinpal

builder.Services.Configure<ZarinpalSettings>(builder.Configuration.GetSection("Zarinpal"));

builder.Services.AddSingleton<Zarinpal>();

#endregion

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseCors("DevCors");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#region Migration Builder

var retry = 5;
while (retry > 0)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();

            context.Database.EnsureCreated();

            await SeedAdmin.InitializeAsync(app);
        }
        break;
    }
    catch
    {
        retry--;
        Console.WriteLine($"Error connecting to DB. Retrying... ({retry} attempts left)");
        Thread.Sleep(5000);
    }
}
#endregion



app.Run();
