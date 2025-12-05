using QRCoder;
using System.Drawing; // Necesario si quieres trabajar con System.Drawing.Bitmap
using System.Drawing.Imaging; // Necesario para el formato de imagen
using System.IO;
using System.Runtime.Versioning; // Usamos System.IO para el MemoryStream

// NOTA: Para que System.Drawing funcione en .NET Core/.NET 5+, DEBES añadir 
// el paquete NuGet 'System.Drawing.Common' al proyecto donde reside esta clase.

namespace CountrySecure.Infrastructure.Utils // Asume un namespace de Infraestructura
{
    public static class QrCodeGenerator
    {
        [SupportedOSPlatform("windows")]//Significa que esta función solo es compatible con Windows.
        public static byte[] GenerateQrCodePng(string qrCodeValue)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                // 1. Crear los datos del QR a partir del valor
                using (var qrCodeData = qrGenerator.CreateQrCode(qrCodeValue, QRCodeGenerator.ECCLevel.Q))
                {
                    // 2. Crear el objeto QR para generar la imagen (Bitmap)
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        // 3. Obtener la imagen (Bitmap). El 20 es el tamaño de los módulos (píxeles)
                        using (var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true))
                        {
                            // 4. Convertir la imagen Bitmap a un array de bytes PNG
                            using (var memoryStream = new MemoryStream())
                            {
                                qrCodeImage.Save(memoryStream, ImageFormat.Png);
                                return memoryStream.ToArray();
                            }
                        }
                    }
                }
            }
        }
    }
}