# CountrySecure - API 🚀🛡️

**CountrySecure-API** es el núcleo lógico y motor de datos de la plataforma de gestión de seguridad para barrios privados. Se trata de una API RESTful robusta construida con **.NET 8**, diseñada bajo principios de **Clean Architecture** para garantizar la escalabilidad, el mantenimiento y la separación de responsabilidades.

## 📝 Origen del Proyecto y Despliegue

Este backend fue desarrollado como el componente central del proyecto final para el bootcamp de **Devlights**. Durante el programa, la API estuvo desplegada en **Amazon Web Services (AWS)**, interactuando con bases de datos gestionadas y sirviendo datos al frontend mediante dominios específicos de la organización.

> **Nota:** Al concluir el bootcamp, la infraestructura en la nube y los dominios de AWS han sido dados de baja. Este repositorio documenta la arquitectura técnica, los endpoints y la lógica de negocio implementada.

## 🏗️ Arquitectura del Sistema

El proyecto sigue una estructura de **Arquitectura de Capas (Clean Architecture)**:

* **CountrySecure.API:** Capa de presentación (Controllers, Filtros y Configuración).
* **CountrySecure.Application:** Lógica de negocio, Interfaces de servicios, DTOs y Mapeadores.
* **CountrySecure.Domain:** Entidades principales, Enums y constantes (el corazón del sistema).
* **CountrySecure.Infrastructure:** Implementación de persistencia (Entity Framework Core), Repositorios, migraciones y servicios externos (Generación de QR, JWT).

## 🚀 Tecnologías y Herramientas

* **Runtime:** [.NET 8](https://dotnet.microsoft.com/)
* **ORM:** [Entity Framework Core](https://learn.microsoft.com/ef/core/) para el mapeo de datos.
* **Base de Datos:** PostgreSQL (preparado para entornos productivos).
* **Seguridad:** Autenticación y Autorización basada en **JWT (JSON Web Tokens)**.
* **Validación:** [FluentValidation](https://docs.fluentvalidation.net/) para asegurar la integridad de los datos entrantes.
* **Herramientas:** * Generación de códigos QR mediante librerías especializadas.
    * BCrypt para el hashing seguro de contraseñas.
    * Docker support para orquestación de servicios.

## ✨ Funcionalidades Principales

La API expone servicios críticos para el funcionamiento de la comunidad:

### 🔐 Seguridad y Auth
* Gestión de identidades con Roles (Admin, Seguridad, Residente).
* Flujo de Login y registro con validaciones estrictas.
* Emisión y validación de Tokens JWT.

### 🏘️ Gestión de Propiedades y Lotes
* CRUD completo de Lotes y unidades habitacionales.
* Control de estados de disponibilidad de propiedades.

### 📋 Control de Accesos (Core)
* **Autorizaciones:** Gestión de permisos de entrada para visitas y servicios.
* **Validación:** Endpoints para que el personal de seguridad valide ingresos.
* **QR:** Generación de códigos para accesos rápidos y seguros.

### 🛠️ Servicios y Amenidades
* Sistema de gestión de turnos para espacios comunes (SUM, canchas, etc.).
* Seguimiento de órdenes de servicio y solicitudes de mantenimiento.

## 📦 Instalación Local

Para ejecutar la API en tu entorno de desarrollo:

1.  **Requisitos:** SDK de .NET 8 y una instancia de PostgreSQL.
2.  **Clonar el repositorio:**
    ```bash
    git clone [https://github.com/fabiangquintana/countrysecure-api.git](https://github.com/fabiangquintana/countrysecure-api.git)
    cd countrysecure-api
    ```
3.  **Configurar la base de datos:**
    Actualiza la cadena de conexión en `appsettings.json` o utiliza variables de entorno.
4.  **Aplicar Migraciones:**
    ```bash
    dotnet ef database update --project CountrySecure.Infrastructure --startup-project CountrySecure.API
    ```
5.  **Ejecutar la aplicación:**
    ```bash
    dotnet run --project CountrySecure.API
    ```
    La documentación de Swagger estará disponible en: `https://localhost:PORT/swagger` (en modo desarrollo).

---
*Backend desarrollado con estándares de industria para la seguridad y gestión residencial.*
