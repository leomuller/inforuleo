using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UrlShortener2.Data;
using UrlShortener2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Required so UseExceptionHandler() below writes a standard ProblemDetails body (no stack trace) for unhandled exceptions.
builder.Services.AddProblemDetails(options =>
{
	// The default exception handler leaves Detail empty (it won't put the exception message there, on purpose -
	// that could leak internals). Give unhandled exceptions a generic Detail; leave Problem(...) calls that already set one alone.
	options.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Detail ??= "An unexpected error occurred. Please try again later.";
	};
});

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("UrlMapperDB")));

builder.Services.AddScoped<UrlShorteningService>();

// /health below actually pings the DB (via AppDbContext.Database.CanConnectAsync()), not just "is the process alive".
builder.Services.AddHealthChecks()
	.AddDbContextCheck<AppDbContext>();

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

// No path given: the default handler (backed by AddProblemDetails above) turns any unhandled
// exception into a 500 ProblemDetails response with no stack trace, in every environment.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment() == true)
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}
else
{
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

// Exposes the implicit top-level Program class so WebApplicationFactory<Program> can reference it from the test projects.
public partial class Program
{
}
