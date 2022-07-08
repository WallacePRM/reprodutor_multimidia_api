
using Microsoft.EntityFrameworkCore;
using ReprodutorMultimia.Database;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        System.IO.Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? Environment.CurrentDirectory, "LogFiles", "Application", "diagnostics.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        retainedFileCountLimit: 2,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddCors((options) => options.AddDefaultPolicy((policy) => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    builder.Services.AddDbContext<AppDbContext>((options) => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

    var app = builder.Build();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
    dbContext?.Database.Migrate();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpsRedirection();
    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch(Exception error)
{
    Log.Fatal(error, "Falha ao iniciar aplicação");
}