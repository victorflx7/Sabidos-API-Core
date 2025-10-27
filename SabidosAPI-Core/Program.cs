using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddLogging();
builder.Services.AddAuthorization();

// ✅ Registro de serviços da aplicação
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<ResumoService>();

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
    // 🔐 JWT Bearer (Firebase)
    var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
                ValidateAudience = true,
                ValidAudience = firebaseProjectId,
                ValidateLifetime = true
            };
        });
}

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

var app = builder.Build();

// -------------------------------------------------------------
// 🚀 Pipeline de execução
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

// 🧠 Ordem correta: primeiro autenticação, depois autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Permite que o WebApplicationFactory acesse o Program
public partial class Program { }
