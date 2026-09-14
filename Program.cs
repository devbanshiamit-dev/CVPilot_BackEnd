using CVPilotAPI.Repository;
using CVPilotAPI.ResumeService;
using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.TextExtractionEngine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IResumeAnalyze, ResumeAnalyze>();
builder.Services.AddScoped<IResumeParserService, TextExtracter>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IResumeServices, ResumeServices>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
