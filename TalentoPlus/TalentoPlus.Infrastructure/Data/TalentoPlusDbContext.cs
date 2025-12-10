using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Entities;

namespace TalentoPlus.Infrastructure.Data;

/// <summary>
/// Application database context with Identity support.
/// </summary>
public class TalentoPlusDbContext : IdentityDbContext<ApplicationUser>
{
    public TalentoPlusDbContext(DbContextOptions<TalentoPlusDbContext> options) : base(options)
    {
    }

    public DbSet<Estado> Estados { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<NivelEducativo> NivelesEducativos { get; set; }
    public DbSet<Empleado> Empleados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Estado configuration
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.ToTable("estado");
            entity.HasKey(e => e.EstadoId);
            entity.Property(e => e.EstadoId).HasColumnName("estado_id");
            entity.Property(e => e.NombreEstado)
                .HasColumnName("nombre_estado")
                .HasMaxLength(50)
                .IsRequired();
            entity.HasIndex(e => e.NombreEstado).IsUnique();
        });

        // Departamento configuration
        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.ToTable("departamento");
            entity.HasKey(e => e.DepartamentoId);
            entity.Property(e => e.DepartamentoId).HasColumnName("departamento_id");
            entity.Property(e => e.NombreDepartamento)
                .HasColumnName("nombre_departamento")
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.NombreDepartamento).IsUnique();
        });

        // Cargo configuration
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable("cargo");
            entity.HasKey(e => e.CargoId);
            entity.Property(e => e.CargoId).HasColumnName("cargo_id");
            entity.Property(e => e.NombreCargo)
                .HasColumnName("nombre_cargo")
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.NombreCargo).IsUnique();
        });

        // NivelEducativo configuration
        modelBuilder.Entity<NivelEducativo>(entity =>
        {
            entity.ToTable("nivel_educativo");
            entity.HasKey(e => e.NivelEducativoId);
            entity.Property(e => e.NivelEducativoId).HasColumnName("nivel_educativo_id");
            entity.Property(e => e.NombreNivel)
                .HasColumnName("nombre_nivel")
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.NombreNivel).IsUnique();
        });

        // Empleado configuration
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("empleado");
            entity.HasKey(e => e.Documento);
            entity.Property(e => e.Documento).HasColumnName("documento");
            entity.Property(e => e.Nombres)
                .HasColumnName("nombres")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.Apellidos)
                .HasColumnName("apellidos")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.FechaNacimiento)
                .HasColumnName("fecha_nacimiento")
                .IsRequired();
            entity.Property(e => e.Direccion)
                .HasColumnName("direccion")
                .HasMaxLength(255);
            entity.Property(e => e.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(20);
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Salario)
                .HasColumnName("salario")
                .HasColumnType("numeric(10,2)");
            entity.Property(e => e.FechaIngreso)
                .HasColumnName("fecha_ingreso")
                .IsRequired();
            entity.Property(e => e.PerfilProfesional)
                .HasColumnName("perfil_profesional");
            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255);
            entity.Property(e => e.EstadoId)
                .HasColumnName("estado_id")
                .IsRequired();
            entity.Property(e => e.NivelEducativoId)
                .HasColumnName("nivel_educativo_id")
                .IsRequired();
            entity.Property(e => e.DepartamentoId)
                .HasColumnName("departamento_id")
                .IsRequired();
            entity.Property(e => e.CargoId)
                .HasColumnName("cargo_id")
                .IsRequired();

            // Foreign key relationships
            entity.HasOne(e => e.Estado)
                .WithMany(s => s.Empleados)
                .HasForeignKey(e => e.EstadoId)
                .HasConstraintName("fk_estado");

            entity.HasOne(e => e.NivelEducativo)
                .WithMany(n => n.Empleados)
                .HasForeignKey(e => e.NivelEducativoId)
                .HasConstraintName("fk_nivel_educativo");

            entity.HasOne(e => e.Departamento)
                .WithMany(d => d.Empleados)
                .HasForeignKey(e => e.DepartamentoId)
                .HasConstraintName("fk_departamento");

            entity.HasOne(e => e.Cargo)
                .WithMany(c => c.Empleados)
                .HasForeignKey(e => e.CargoId)
                .HasConstraintName("fk_cargo");
        });
    }
}

/// <summary>
/// Custom application user for administrators.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}

