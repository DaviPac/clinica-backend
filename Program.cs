using Api.Application.Services;
using Api.Domain;
using Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // URL do frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var connStr = BuildConnectionString();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connStr));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IDespesaClinicaRepository, DespesaClinicaRepository>();
builder.Services.AddScoped<IDespesaClinicaService, DespesaClinicaService>();
builder.Services.AddScoped<IAcertoComissaoRepository, AcertoComissaoRepository>();
builder.Services.AddScoped<IAcertoComissaoService, AcertoComissaoService>();
builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();

// --- Autenticação JWT ---
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "user_id",
            RoleClaimType = "role"
        };
    });

// --- Autorização com policy de admin ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole(nameof(Role.ADMIN)));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.CanConnectAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static string BuildConnectionString()
{
    var host     = Environment.GetEnvironmentVariable("DB_HOST")     ?? "localhost";
    var port     = Environment.GetEnvironmentVariable("DB_PORT")     ?? "5432";
    var user     = Environment.GetEnvironmentVariable("DB_USER")     ?? throw new InvalidOperationException("DB_USER não definido");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD não definido");
    var database = Environment.GetEnvironmentVariable("DB_NAME")     ?? throw new InvalidOperationException("DB_NAME não definido");
    var sslMode  = Environment.GetEnvironmentVariable("DB_SSLMODE")  ?? "disable";

    var sslModeNpgsql = sslMode.Equals("disable", StringComparison.OrdinalIgnoreCase)
        ? "Disable" : "Require";

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode={sslModeNpgsql}";
}