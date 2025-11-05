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
    // 👉 Usa SQL Server normalmente fora do ambiente de teste
    Console.WriteLine("⚙️ Usando banco de dados SQL Server (modo normal)");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// -------------------------------------------------------------
// 🧩 AutoMapper e serviços
// -------------------------------------------------------------
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddAutoMapper(typeof(ResumoProfile));
builder.Services.AddAutoMapper(typeof(EventoProfile));
builder.Services.AddAutoMapper(typeof(PomodoroProfile)); // ✅ ADICIONAR se tiver

builder.Services.AddLogging();

// ✅ Registro de serviços da aplicação
builder.Services.AddScoped<ResumoService>();
builder.Services.AddScoped<IEventoService, EventoService>(); // ✅ USAR interface
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPomodoroService, PomodoroService>(); // ✅ USAR interface
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
    builder.Services.AddAuthentication(); // ✅ MOVER para antes do Build()
}

builder.Services.AddAuthorization(); // ✅ MOVER para antes do Build()

// -------------------------------------------------------------
// 🧩 CORS
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

app.Run();

// Permite que o WebApplicationFactory acesse o Program
public partial class Program { }