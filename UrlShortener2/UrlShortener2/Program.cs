using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UrlShortener2.Data;
using UrlShortener2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("UrlMapperDB")));

builder.Services.AddScoped<UrlShorteningService>();

// Add CORS support for local development
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowLocalDevelopment", policy =>
	{
		policy
			.WithOrigins("http://localhost:4200", "https://localhost:4200")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	});
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() == true)
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
