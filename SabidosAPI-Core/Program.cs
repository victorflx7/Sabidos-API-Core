using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SabidosAPI_Core.AutoMapper;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Mappings;
using SabidosAPI_Core.Profiles;
using SabidosAPI_Core.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"🌱 Ambiente atual: {builder.Environment.EnvironmentName}");

// 🌐 Correct binding for Docker
builder.WebHost.UseUrls("http://0.0.0.0:80");

// -------------------------------------------------------------
// 🧩 Configuração do Prometheus Metrics
// -------------------------------------------------------------
builder.Services.AddHealthChecks()
    .ForwardToPrometheus();

// -------------------------------------------------------------
// 🧩 Banco de Dados (MANTIDO ORIGINAL)
// -------------------------------------------------------------
if (builder.Environment.IsEnvironment("Testing"))
{
    Console.WriteLine("⚙️ Usando banco de dados InMemory (modo de testes)");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    if (!string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("🐳 Usando banco de dados SQL Server (Docker)");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        Console.WriteLine("💻 Usando banco de dados SQL Server (Local)");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    }
}

// -------------------------------------------------------------
// 🧩 AutoMapper / Services (MANTIDO ORIGINAL)
// -------------------------------------------------------------
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddAutoMapper(typeof(ResumoProfile));
builder.Services.AddAutoMapper(typeof(EventoProfile));
builder.Services.AddAutoMapper(typeof(PomodoroProfile));

builder.Services.AddLogging();
builder.Services.AddScoped<ResumoService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPomodoroService, PomodoroService>();
builder.Services.AddScoped<FlashcardService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------------
// 🧩 Autenticação (MANTIDO ORIGINAL)
// -------------------------------------------------------------
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddAuthentication("TestScheme")
        .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("TestScheme", options => { });
}
else
{
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

// -------------------------------------------------------------
// 🧩 CORS (MANTIDO ORIGINAL)
// -------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------------------------------------------------
// 🚀 Build App
// -------------------------------------------------------------
var app = builder.Build();

// -------------------------------------------------------------
// 🚀 Pipeline (CORREÇÃO DAS MÉTRICAS)
// -------------------------------------------------------------

// ✅ MIDDLEWARE DE MÉTRICAS PRIMEIRO (antes de tudo)
app.UseRouting();
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

// ✅ ENDPOINTS
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// -------------------------------------------------------------
// 🔧 MIGRATIONS AUTOMÁTICAS (MANTIDO ORIGINAL)
// -------------------------------------------------------------
try
{
    Console.WriteLine("🎯 Iniciando configuração do banco de dados...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await Task.Delay(2000);

    Console.WriteLine("🔍 Verificando se o banco pode ser conectado...");

    int retries = 5;
    for (int i = 0; i < retries; i++)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("✅ Conexão com o banco estabelecida!");
                break;
            }
        }
        catch
        {
            Console.WriteLine($"⏳ Tentativa {i + 1}/{retries} - aguardando...");
            if (i == retries - 1) throw;
            await Task.Delay(5000);
        }
    }

    Console.WriteLine("🔄 Aplicando migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("✅ Migrations aplicadas com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro ao configurar banco: {ex.Message}");
    Console.WriteLine("⚠️ Continuando sem banco de dados...");
}

Console.WriteLine("🚀 API Sabidos iniciada com monitoramento Prometheus!");
Console.WriteLine("📊 Métricas disponíveis em: http://0.0.0.0:80/metrics");
Console.WriteLine("❤️ Health check disponível em: http://0.0.0.0:80/health");
Console.WriteLine("📚 Swagger disponível em: http://0.0.0.0:80/swagger");

app.Run();

public partial class Program { }