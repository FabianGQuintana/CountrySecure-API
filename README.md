# 🏡 CountrySecure API (Backend - .NET Core)

Este repositorio contiene el proyecto de Backend para el sistema de gestión y seguridad de Countries Privados, desarrollado en **C# y ASP.NET Core**.

El proyecto sigue la **Arquitectura Limpia (Clean Architecture)** para garantizar la separación de preocupaciones, alta mantenibilidad y facilidad de prueba.

---

## 🎯 Arquitectura del Proyecto (Clean Architecture)

La solución está dividida en cuatro proyectos (`.csproj`), donde cada uno representa una capa de la arquitectura.

**REGLA FUNDAMENTAL:** La dependencia siempre apunta hacia el interior. **Una capa exterior puede ver a una interior, pero NUNCA al revés.**

| Proyecto | Capa | Propósito | Reglas |
| :--- | :--- | :--- | :--- |
| **`CountrySecure.API`** | **Presentación** | Punto de entrada HTTP. Contiene los **Controladores** y la configuración inicial (CORS, Inyección de Dependencias). | SOLO se comunica con `Application` (para enviar Comandos/Consultas) y `Infrastructure` (para la configuración inicial). |
| **`CountrySecure.Application`** | **Casos de Uso** | Contiene la lógica específica de la aplicación (el "qué se hace"). Define las **Interfaces** de Repositorios y Servicios. | SOLO depende de `Domain`. NUNCA de `Infrastructure`. |
| **`CountrySecure.Infrastructure`** | **Adaptadores** | Contiene la **implementación** de la tecnología externa (el "cómo se hace"). Implementa las interfaces de `Application`. | Depende de `Application` y `Domain`. Contiene Entity Framework Core. |
| **`CountrySecure.Domain`** | **Núcleo de Negocio** | Contiene las **Entidades**, **Value Objects** y reglas de negocio transversales. | **No depende de ningún otro proyecto.** Es el núcleo puro. |

### 📁 Estructura Interna por Capa

| Capa | Carpetas Clave | Uso Principal |
| :--- | :--- | :--- |
| **Domain** | `Entities`, `ValueObjects`, `Enums` | Modelos de la base de datos y la lógica pura. |
| **Application** | `Features/Commands`, `Features/Queries`, `Interfaces`, `DTOs` | Lógica de la aplicación y contratos de código. |
| **Infrastructure** | `Persistence/Context`, `Repositories`, `Services` | Configuración de EF Core, DB Context, e Implementaciones. |
| **API** | `Controllers` | Lógica para recibir peticiones HTTP y devolver respuestas. |

---

## 🚀 Guía de Configuración Local

Para empezar a trabajar con el proyecto, sigue estos pasos:

### 1. Requisitos

* **Visual Studio 2022** (o una versión moderna que soporte .NET 8/9).
* **.NET SDK** instalado.
* **SQL Server** (o la base de datos elegida) instalado y accesible.

### 2. Clonar el Repositorio

```bash
git clone [https://docs-en.keysecure.io/api-references/intro/custody-apis](https://docs-en.keysecure.io/api-references/intro/custody-apis)
cd CountrySecure-API
