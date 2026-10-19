using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.Extensions.Configuration;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CountrySecure.Infrastructure.Services
{
    /// <summary>
    /// Pase digital en PDF (A5): el QR va en la esquina superior izquierda, seguido de los datos del residente,
    /// de la visita, del vehículo, de la ventana horaria autorizada y las instrucciones de uso.
    /// </summary>
    public class PassDocumentService : IPassDocumentService
    {
        private static readonly string Muted = Colors.Grey.Darken1;
        private static readonly string Accent = Colors.Purple.Darken2;

        private readonly TimeZoneInfo _timeZone;

        public PassDocumentService(IConfiguration configuration)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _timeZone = ResolveTimeZone(configuration["Pass:TimeZone"] ?? "America/Argentina/Buenos_Aires");
        }

        public byte[] GeneratePdf(PassDto pass)
        {
            var qr = RenderQr(pass.Token);

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(28);
                    page.DefaultTextStyle(t => t.FontSize(10.5f).FontColor(Colors.Grey.Darken4));

                    page.Content().Column(column =>
                    {
                        column.Spacing(14);

                        // Encabezado: QR arriba a la izquierda y título a su derecha
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(130).Height(130).Image(qr).FitArea();

                            row.RelativeItem().PaddingLeft(18).Column(header =>
                            {
                                header.Spacing(3);
                                header.Item().Text("PASE DE INGRESO").FontSize(20).Bold().FontColor(Accent);
                                header.Item().Text("Country Secure").FontColor(Muted);
                                header.Item().PaddingTop(8).Text(pass.VisitorName).FontSize(14).SemiBold();
                                header.Item().Text(PassTypeLabel(pass.PassType)).FontColor(Muted);
                            });
                        });

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        Section(column, "Residente anfitrión",
                            ("Nombre", pass.ResidentName),
                            ("Lote / casa", pass.Destination));

                        Section(column, "Visitante",
                            ("Nombre", pass.VisitorName),
                            ("Documento", pass.VisitorDni.ToString()),
                            ("Acompañantes", pass.CompanionsCount.ToString()));

                        Section(column, "Vehículo", VehicleRows(pass));

                        Section(column, "Autorización",
                            ("Desde", Format(pass.ValidFrom)),
                            ("Hasta", pass.ValidTo.HasValue ? Format(pass.ValidTo.Value) : "Sin vencimiento"));

                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(box =>
                        {
                            box.Spacing(2);
                            box.Item().Text("Instrucciones").SemiBold();
                            box.Item().Text("• Presentá este código QR en la guardia (en el celular o impreso).");
                            box.Item().Text("• Llevá tu documento: el guardia lo va a corroborar.");
                            box.Item().Text("• El pase solo es válido dentro del horario autorizado.");
                            box.Item().Text(pass.PassType == PassType.SingleEntry
                                ? "• Es de un solo ingreso: una vez usado no se puede volver a utilizar."
                                : "• Permite varios ingresos y salidas dentro del horario autorizado.");
                        });
                    });

                    page.Footer().AlignCenter().Text($"Pase {pass.PermissionId.ToString()[..8].ToUpperInvariant()}")
                        .FontSize(8).FontColor(Muted);
                });
            }).GeneratePdf();
        }

        // -------------------------------------------------------------------

        private static void Section(ColumnDescriptor column, string title, params (string Label, string Value)[] rows)
        {
            column.Item().Column(section =>
            {
                section.Spacing(3);
                section.Item().Text(title.ToUpperInvariant()).FontSize(8.5f).Bold().FontColor(Accent);

                foreach (var (label, value) in rows)
                {
                    section.Item().Row(row =>
                    {
                        row.ConstantItem(95).Text(label).FontColor(Muted);
                        row.RelativeItem().Text(value).SemiBold();
                    });
                }
            });
        }

        private static (string, string)[] VehicleRows(PassDto pass)
        {
            if (pass.VehicleType == VehicleType.OnFoot)
                return [("Ingreso", "A pie")];

            var rows = new List<(string, string)>
            {
                ("Tipo", pass.VehicleType == VehicleType.Car ? "Auto" : "Moto"),
                ("Patente", pass.VehiclePlate ?? "—")
            };
            if (!string.IsNullOrWhiteSpace(pass.VehicleModel)) rows.Add(("Modelo", pass.VehicleModel));
            return rows.ToArray();
        }

        private static string PassTypeLabel(PassType type)
            => type == PassType.SingleEntry ? "Un solo ingreso" : "Múltiples ingresos";

        private string Format(DateTime utc)
        {
            var asUtc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(asUtc, _timeZone).ToString("dd/MM/yyyy HH:mm");
        }

        private static byte[] RenderQr(string token)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            // PngByteQRCode no depende de System.Drawing, así que funciona en cualquier sistema operativo
            return new PngByteQRCode(data).GetGraphic(10);
        }

        private static TimeZoneInfo ResolveTimeZone(string id)
        {
            foreach (var candidate in new[] { id, "Argentina Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }
            return TimeZoneInfo.Utc;
        }
    }
}
