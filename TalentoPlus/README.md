# TalentoPlus - Sistema de Gestión de Empleados

Sistema completo de gestión de empleados con arquitectura limpia (Clean Architecture) desarrollado con .NET 9, PostgreSQL y Vue.js 3.

## 🏗️ Arquitectura

```
TalentoPlus/
├── TalentoPlus.Core/           # Entidades, Interfaces, DTOs (sin dependencias externas)
├── TalentoPlus.Infrastructure/ # DbContext, Repositorios, Servicios (EF Core, EPPlus, QuestPDF)
├── TalentoPlus.API/            # Controllers, Configuración, JWT Auth
└── client/                     # Frontend Vue.js 3 + Vite
```

## 🚀 Características

### Backend
- **Autenticación Híbrida**: 
  - Administradores: ASP.NET Core Identity con JWT
  - Empleados: Autenticación personalizada con documento + contraseña
- **CRUD completo** de empleados y catálogos
- **Importación de Excel** (.xlsx) con EPPlus - lógica upsert
- **Dashboard con IA** (Gemini) - Consultas en lenguaje natural a SQL
- **Generación de PDF** (QuestPDF) - Hoja de vida del empleado
- **Servicio de Email** (SMTP)

### Frontend
- Vue.js 3 con Composition API
- Vue Router con guards de autenticación
- Pinia para manejo de estado
- Axios con interceptores JWT
- UI responsiva

## 📋 Requisitos Previos

- .NET 9 SDK
- Node.js 18+
- PostgreSQL 16+
- Docker y Docker Compose (opcional)

## 🛠️ Configuración

### 1. Clonar y configurar variables de entorno

```bash
# Backend
cp TalentoPlus/.env.example TalentoPlus/.env

# Frontend
cp client/.env.example client/.env
```

### 2. Configurar el archivo .env

```env
# Database
DATABASE_URL=Host=localhost;Port=5432;Database=talentoplusdb;Username=postgres;Password=tu_password

# JWT
JWT_SECRET=TuClaveSecretaMuySeguraDeAlMenos32Caracteres!
JWT_ISSUER=TalentoPlus
JWT_AUDIENCE=TalentoPlus

# Gemini AI
GEMINI_API_KEY=tu_api_key_de_gemini

# SMTP (opcional)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu_email@gmail.com
SMTP_PASSWORD=tu_app_password
```

### 3. Crear la base de datos

```bash
# Ejecutar el script SQL inicial
psql -U postgres -d postgres -f db.sql
```

### 4. Ejecutar migraciones de Entity Framework

```bash
cd TalentoPlus
dotnet ef migrations add InitialCreate --project TalentoPlus.Infrastructure --startup-project TalentoPlus.API
dotnet ef database update --project TalentoPlus.Infrastructure --startup-project TalentoPlus.API
```

### 5. Ejecutar el backend

```bash
cd TalentoPlus/TalentoPlus.API
dotnet run
```

La API estará disponible en: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### 6. Ejecutar el frontend

```bash
cd client
npm install
npm run dev
```

El frontend estará disponible en: `http://localhost:5173`

## 🐳 Docker

Para ejecutar todo con Docker Compose:

```bash
# Copiar y configurar el archivo .env
cp TalentoPlus/.env.example .env

# Construir y ejecutar
docker-compose up --build

# O en modo detached
docker-compose up -d --build
```

Servicios disponibles:
- API: `http://localhost:5000`
- Frontend: `http://localhost:3000`
- PostgreSQL: `localhost:5432`

## 📚 API Endpoints

### Autenticación
- `POST /api/auth/admin/register` - Registro de administrador
- `POST /api/auth/admin/login` - Login de administrador
- `POST /api/auth/employee-login` - Login de empleado

### Empleados
- `GET /api/empleados` - Listar todos
- `GET /api/empleados/{documento}` - Obtener por documento
- `POST /api/empleados` - Crear (Admin)
- `PUT /api/empleados/{documento}` - Actualizar (Admin)
- `DELETE /api/empleados/{documento}` - Eliminar (Admin)
- `POST /api/empleados/import` - Importar desde Excel (Admin)
- `GET /api/empleados/{documento}/resume` - Descargar PDF

### Dashboard IA
- `POST /api/dashboard/query` - Consulta en lenguaje natural (Admin)

### Catálogos
- `GET /api/catalogs/estados`
- `GET /api/catalogs/departamentos`
- `GET /api/catalogs/cargos`
- `GET /api/catalogs/niveles-educativos`

## 📊 Formato de Excel para importación

| Columna | Campo | Requerido |
|---------|-------|-----------|
| A | Documento | ✓ |
| B | Nombres | ✓ |
| C | Apellidos | ✓ |
| D | FechaNacimiento | ✓ |
| E | Direccion | |
| F | Telefono | |
| G | Email | |
| H | Salario | |
| I | FechaIngreso | ✓ |
| J | PerfilProfesional | |
| K | Estado | ✓ |
| L | NivelEducativo | ✓ |
| M | Departamento | ✓ |
| N | Cargo | ✓ |

## 🤖 Ejemplos de consultas IA

- "¿Cuántos empleados hay?"
- "¿Cuántos empleados hay en el departamento de Tecnología?"
- "¿Cuál es el salario promedio?"
- "Lista los 5 empleados con mayor salario"
- "¿Cuántos empleados hay por departamento?"

## 📝 Licencia

MIT License

