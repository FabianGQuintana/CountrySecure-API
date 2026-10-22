# CountrySecure API

API REST (.NET 9 + PostgreSQL) de CountrySecure: control de acceso, reservas de amenities, solicitudes de servicio y administración de un barrio privado.

## Puesta en marcha

1. Configurar los secretos (no se versionan). La plantilla es `CountrySecure.API/appsettings.Example.json`:

   ```bash
   cd CountrySecure.API
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CountrySecureDB;Username=postgres;Password=..."
   dotnet user-secrets set "Jwt:Key" "<clave aleatoria de 32+ caracteres>"
   dotnet user-secrets set "Seed:Users:Admin:Password" "<contraseña>"
   dotnet user-secrets set "Seed:Users:Resident:Password" "<contraseña>"
   dotnet user-secrets set "Seed:Users:Security:Password" "<contraseña>"
   ```

2. Aplicar las migraciones: `dotnet ef database update --project CountrySecure.Infrastructure --startup-project CountrySecure.API`
3. Levantar la API en `http://localhost:5021` (el puerto que usa el front): `dotnet run --project CountrySecure.API --launch-profile http`

Al iniciar se crean (o se re-sincronizan) los usuarios demo de cada rol con las contraseñas configuradas.

## Correr desde Visual Studio sin que la API "se cuelgue"

Las reglas de negocio esperadas (reserva superpuesta, sesión vencida, registro inexistente…) se informan con excepciones del dominio que el `GlobalExceptionHandler` convierte en respuestas 4xx. **Con el depurador (F5), Visual Studio detiene la API en cada una** hasta que se apreta *Continuar*, y mientras tanto el front queda esperando.

- Para usar la app: **Depurar › Iniciar sin depurar (Ctrl+F5)**.
- Para depurar: en *Configuración de excepciones* (Ctrl+Alt+E), destildar *"Interrumpir cuando la excepción no está controlada por el código de usuario"* para `CountrySecure.Domain.Exceptions.*`, `System.Collections.Generic.KeyNotFoundException` y `System.UnauthorizedAccessException`.

## Tests

```bash
dotnet test
```
