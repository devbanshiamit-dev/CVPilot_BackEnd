using CVPilotAPI.AnalysisControll;
using CVPilotAPI.AnalysisRepository;
using CVPilotAPI.Middleware;
using CVPilotAPI.Repository;
using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.ResumeService;
using CVPilotAPI.SuggestionRepository;
using CVPilotAPI.SuggetionRepository;
using CVPilotAPI.TextExtractionEngine;
using CVPilotAPI.Validate;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IResumeAnalyze, ResumeAnalyze>();
builder.Services.AddScoped<IResumeParserService, TextExtracter>();

// Register repositories
builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IUserAnalysisRepository, UserAnalysisRepository>();

builder.Services.AddScoped<ValidateToken>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("FixedPolicy", config =>
    {
        config.PermitLimit = 30;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseMiddleware<ValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactPolicy");

app.UseRateLimiter();

app.UseMiddleware<ValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
