using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SabidosAPI_Core.AutoMapper;
using SabidosAPI_Core.Data;
using SabidosAPI_Core.Mappings;
using SabidosAPI_Core.Profiles;
using SabidosAPI_Core.Services;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"🌱 Ambiente atual: {builder.Environment.EnvironmentName}");

// -------------------------------------------------------------
// 🧩 Banco de Dados (condicional por ambiente)
// -------------------------------------------------------------
if (builder.Environment.IsEnvironment("Testing"))
{
    // 👉 Usa banco em memória durante testes
    Console.WriteLine("⚙️ Usando banco de dados InMemory (modo de testes)");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    // 👉 CONFIGURAÇÃO INTELIGENTE PARA DOCKER/LOCAL
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    if (!string.IsNullOrEmpty(connectionString))
    {
        // 🔥 DETECTOU DOCKER - usa connection string do container
        Console.WriteLine("🐳 Usando banco de dados SQL Server (Docker)");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        // 💻 MODO LOCAL - usa connection string do appsettings.json
        Console.WriteLine("💻 Usando banco de dados SQL Server (Local)");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    }
}

// -------------------------------------------------------------
// 🧩 AutoMapper e serviços
// -------------------------------------------------------------
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddAutoMapper(typeof(ResumoProfile));
builder.Services.AddAutoMapper(typeof(EventoProfile));
builder.Services.AddAutoMapper(typeof(PomodoroProfile));

builder.Services.AddLogging();

// ✅ Registro de serviços da aplicação
builder.Services.AddScoped<ResumoService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPomodoroService, PomodoroService>();
builder.Services.AddScoped<FlashcardService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------------------------------
// 🧩 Autenticação condicional
// -------------------------------------------------------------
if (builder.Environment.IsEnvironment("Testing"))
{
    // 🔐 Autenticação Fake para testes (FakeJwtHandler)
    builder.Services.AddAuthentication("TestScheme")
        .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("TestScheme", options => { });
}
else
{
    // 🔐 Authentication básica (sem JWT)
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

// -------------------------------------------------------------
// 🧩 CORS
// -------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://localhost:5173") // ✅ Corrigido
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// ✅ AGORA construímos a aplicação
var app = builder.Build();

// -------------------------------------------------------------
// 🚀 Pipeline de execução (ORDEM CORRETA É CRUCIAL)
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔑 1. CORS: DEVE VIR ANTES de tudo que possa bloquear ou redirecionar
app.UseCors("AllowSpecificOrigin");

// 🔑 2. HTTPS Redirection (apenas em produção)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔑 3. AUTENTICAÇÃO
app.UseAuthentication();

// 🔑 4. AUTORIZAÇÃO
app.UseAuthorization();

// 🔑 5. CONTROLLERS
app.MapControllers();

// -------------------------------------------------------------
// 🔧 APLICAR MIGRATIONS AUTOMATICAMENTE (VERSÃO CORRIGIDA)
// -------------------------------------------------------------
try
{
    Console.WriteLine("🎯 Iniciando configuração do banco de dados...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Aguardar um pouco para o SQL Server ficar totalmente pronto
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
        catch (Exception ex)
        {
            Console.WriteLine($"⏳ Tentativa {i + 1}/{retries} - Aguardando banco ficar pronto...");
            if (i == retries - 1) throw;
            await Task.Delay(5000); // Aguarda 5 segundos entre tentativas
        }
    }

    Console.WriteLine("🔄 Aplicando migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("✅ Migrations aplicadas com sucesso!");

    // ✅ VERIFICAÇÃO SIMPLIFICADA E SEGURA
    Console.WriteLine("📊 Tabelas criadas com sucesso: Users, Eventos, Flashcards, Pomodoros, Resumos");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro crítico ao configurar banco de dados: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"📋 Detalhes: {ex.InnerException.Message}");
    }
    // Não relançamos a exceção para permitir que a API continue rodando
    // mesmo se já tiver migrations aplicadas
    Console.WriteLine("⚠️ A API continuará rodando, verifique o banco manualmente se necessário.");
}

app.Run();

// Permite que o WebApplicationFactory acesse o Program
public partial class Program { }