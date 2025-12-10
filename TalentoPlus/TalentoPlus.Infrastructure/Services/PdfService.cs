using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Repositories;

namespace TalentoPlus.Infrastructure.Services;

/// <summary>
/// PDF generation service using QuestPDF.
/// </summary>
public class PdfService : IPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PdfService> _logger;

    public PdfService(IUnitOfWork unitOfWork, ILogger<PdfService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        
        // Set QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateEmployeeResumeAsync(long documento)
    {
        var empleadoRepo = _unitOfWork.Empleados as EmpleadoRepository;
        var empleado = await empleadoRepo!.GetWithDetailsAsync(documento);

        if (empleado == null)
        {
            throw new KeyNotFoundException($"Empleado con documento {documento} no encontrado.");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, empleado));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text("HOJA DE VIDA")
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);
                
                column.Item()
                    .Text("Sistema TalentoPlus")
                    .FontSize(12)
                    .FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, Core.Entities.Empleado empleado)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Personal Information Section
            column.Item().Element(c => ComposeSection(c, "INFORMACIÓN PERSONAL", section =>
            {
                section.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Documento: {empleado.Documento}").Bold();
                        col.Item().Text($"Nombres: {empleado.Nombres} {empleado.Apellidos}");
                        col.Item().Text($"Fecha de Nacimiento: {empleado.FechaNacimiento:dd/MM/yyyy}");
                        col.Item().Text($"Edad: {CalculateAge(empleado.FechaNacimiento)} años");
                    });
                    
                    row.RelativeItem().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(empleado.Email))
                            col.Item().Text($"Email: {empleado.Email}");
                        if (!string.IsNullOrEmpty(empleado.Telefono))
                            col.Item().Text($"Teléfono: {empleado.Telefono}");
                        if (!string.IsNullOrEmpty(empleado.Direccion))
                            col.Item().Text($"Dirección: {empleado.Direccion}");
                    });
                });
            }));

            // Work Information Section
            column.Item().Element(c => ComposeSection(c, "INFORMACIÓN LABORAL", section =>
            {
                section.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Departamento: {empleado.Departamento?.NombreDepartamento ?? "N/A"}").Bold();
                        col.Item().Text($"Cargo: {empleado.Cargo?.NombreCargo ?? "N/A"}");
                        col.Item().Text($"Estado: {empleado.Estado?.NombreEstado ?? "N/A"}");
                    });
                    
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Fecha de Ingreso: {empleado.FechaIngreso:dd/MM/yyyy}");
                        col.Item().Text($"Antigüedad: {CalculateYearsWorked(empleado.FechaIngreso)}");
                        if (empleado.Salario.HasValue)
                            col.Item().Text($"Salario: ${empleado.Salario:N2}");
                    });
                });
            }));

            // Education Section
            column.Item().Element(c => ComposeSection(c, "NIVEL EDUCATIVO", section =>
            {
                section.Item().Text(empleado.NivelEducativo?.NombreNivel ?? "No especificado");
            }));

            // Professional Profile Section
            if (!string.IsNullOrEmpty(empleado.PerfilProfesional))
            {
                column.Item().Element(c => ComposeSection(c, "PERFIL PROFESIONAL", section =>
                {
                    section.Item().Text(empleado.PerfilProfesional);
                }));
            }
        });
    }

    private void ComposeSection(IContainer container, string title, Action<ColumnDescriptor> content)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item()
                .BorderBottom(2)
                .BorderColor(Colors.Blue.Darken3)
                .PaddingBottom(5)
                .Text(title)
                .FontSize(14)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            column.Item().PaddingTop(10).Column(content);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generado por TalentoPlus - ");
            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        });
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private string CalculateYearsWorked(DateTime startDate)
    {
        var today = DateTime.Today;
        var years = today.Year - startDate.Year;
        var months = today.Month - startDate.Month;
        
        if (months < 0)
        {
            years--;
            months += 12;
        }

        if (years > 0)
            return $"{years} año(s), {months} mes(es)";
        return $"{months} mes(es)";
    }
}

