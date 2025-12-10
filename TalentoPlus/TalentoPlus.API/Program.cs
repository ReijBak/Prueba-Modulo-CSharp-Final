using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.API.Extensions;
using TalentoPlus.Infrastructure.Data;

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}
else
{
    Env.Load(); // Fallback to current directory
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TalentoPlusDbContext>();
        await context.Database.MigrateAsync();

        // Seed roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed initial data - add missing values
        var estadosToAdd = new[] { "Activo", "Inactivo", "Vacaciones", "Licencia" };
        var existingEstados = await context.Estados.Select(e => e.NombreEstado.ToLower()).ToListAsync();
        foreach (var estado in estadosToAdd.Where(e => !existingEstados.Contains(e.ToLower())))
        {
            context.Estados.Add(new TalentoPlus.Core.Entities.Estado { NombreEstado = estado });
        }

        var departamentosToAdd = new[] { "Recursos Humanos", "Tecnología", "Marketing", "Operaciones", "Logística", "Contabilidad", "Ventas" };
        var existingDepartamentos = await context.Departamentos.Select(d => d.NombreDepartamento.ToLower()).ToListAsync();
        foreach (var dep in departamentosToAdd.Where(d => !existingDepartamentos.Contains(d.ToLower())))
        {
            context.Departamentos.Add(new TalentoPlus.Core.Entities.Departamento { NombreDepartamento = dep });
        }

        var cargosToAdd = new[] { "Ingeniero", "Soporte Técnico", "Analista", "Coordinador", "Desarrollador", "Auxiliar", "Administrador" };
        var existingCargos = await context.Cargos.Select(c => c.NombreCargo.ToLower()).ToListAsync();
        foreach (var cargo in cargosToAdd.Where(c => !existingCargos.Contains(c.ToLower())))
        {
            context.Cargos.Add(new TalentoPlus.Core.Entities.Cargo { NombreCargo = cargo });
        }

        var nivelesEducativosToAdd = new[] { "Bachiller", "Técnico", "Tecnólogo", "Profesional", "Especialización", "Maestría", "Doctorado" };
        var existingNiveles = await context.NivelesEducativos.Select(n => n.NombreNivel.ToLower()).ToListAsync();
        foreach (var nivel in nivelesEducativosToAdd.Where(n => !existingNiveles.Contains(n.ToLower())))
        {
            context.NivelesEducativos.Add(new TalentoPlus.Core.Entities.NivelEducativo { NombreNivel = nivel });
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TalentoPlus API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
