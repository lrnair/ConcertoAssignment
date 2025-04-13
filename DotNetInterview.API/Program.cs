using DotNetInterview.API;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MediatR;
using DotNetInterview.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connection = new SqliteConnection("Data Source=DotNetInterview;Mode=Memory;Cache=Shared");
connection.Open();
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connection));

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// register SystemTimeProvider
builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
