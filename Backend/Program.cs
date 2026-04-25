
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// registering db context
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source = app.db"));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value.Errors.Select(x => x.ErrorMessage)
            });

        return new BadRequestObjectResult(errors);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    if (!db.BankLookups.Any())
    {
        db.BankLookups.AddRange(
            new BankLookup { Code = "1234", BankName = "Nairobi Bank" },
            new BankLookup { Code = "1235", BankName = "Denver Bank" },
            new BankLookup { Code = "1236", BankName = "Moscow Bank" },
            new BankLookup { Code = "1237", BankName = "Tokio Bank" }
        );

        db.SaveChanges();
    }
    if(!db.Customers.Any())
    {
        var defaultCustomer = new Customer
        {
            Name = "123456789",
            Email = "sampleUser@test.com",
            PasswordHash = HashPassword("password123"),
            CreatedAt = DateTime.UtcNow
        };

        db.Customers.Add(defaultCustomer);
        db.SaveChanges();
    }
}
string HashPassword(string password)
{
    using var sha = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password);
    return Convert.ToBase64String(sha.ComputeHash(bytes));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
