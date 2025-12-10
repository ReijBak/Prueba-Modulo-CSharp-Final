# 🏢 TalentoPlus - Sistema de Gestión de Empleados

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Vue.js](https://img.shields.io/badge/Vue.js-3.5-4FC08D?style=for-the-badge&logo=vue.js&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4.1-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)

**Sistema completo de gestión de recursos humanos con autenticación híbrida, importación de datos desde Excel, generación de reportes PDF e integración con IA.**

[Características](#-características) •
[Tecnologías](#-tecnologías) •
[Arquitectura](#-arquitectura) •
[Instalación](#-instalación) •
[Uso](#-uso) •
[API](#-documentación-api) •
[Contribuir](#-contribuir)

</div>

---

## 📋 Descripción

**TalentoPlus** es un sistema web completo para la gestión de empleados en organizaciones. Permite administrar información del personal, importar datos masivamente desde archivos Excel, generar hojas de vida en PDF, y consultar información mediante lenguaje natural utilizando inteligencia artificial.

El sistema implementa una **arquitectura limpia (Clean Architecture)** con separación de responsabilidades en capas, siguiendo las mejores prácticas de desarrollo de software empresarial.

---

## ✨ Características

### 🔐 Autenticación Híbrida
- **Administradores**: Sistema de autenticación mediante ASP.NET Core Identity con registro y login por email/contraseña.
- **Empleados**: Login mediante número de documento y contraseña, con JWT para autorización.

### 👥 Gestión de Empleados
- CRUD completo de empleados con validaciones.
- Información detallada: datos personales, laborales, educación y perfil profesional.
- Estados de empleados con indicadores visuales (Activo/Inactivo/Otros).
- Control de acceso basado en roles (Admin puede ver todos, Empleado solo su información).

### 📊 Importación Masiva desde Excel
- Carga de empleados mediante archivos `.xlsx`.
- Validación automática de datos.
- Lógica de upsert: actualiza si existe, crea si es nuevo.
- Generación automática de contraseñas.
- Eliminación automática de tildes en emails.

### 📄 Generación de PDF
- Hojas de vida profesionales generadas automáticamente.
- Diseño elegante con QuestPDF.
- Descarga directa desde la aplicación.

### 🤖 Dashboard con IA
- Consultas en lenguaje natural sobre la base de datos.
- Integración con OpenAI GPT para traducir preguntas a SQL.
- Respuestas inteligentes basadas en datos reales.

### 📧 Notificaciones por Email
- Envío de credenciales de acceso a nuevos empleados.
- Integración con SMTP (Gmail compatible).
- Plantillas HTML profesionales.

---

## 🛠 Tecnologías

### Backend
| Tecnología | Versión | Descripción |
|------------|---------|-------------|
| **.NET** | 9.0 | Framework principal |
| **ASP.NET Core Web API** | 9.0 | Framework para APIs REST |
| **Entity Framework Core** | 9.0.4 | ORM para acceso a datos |
| **PostgreSQL** | 16 | Base de datos relacional |
| **ASP.NET Core Identity** | 9.0.4 | Sistema de autenticación |
| **JWT Bearer** | 9.0.0 | Autenticación con tokens |
| **EPPlus** | 8.3.1 | Lectura de archivos Excel |
| **QuestPDF** | 2025.7.4 | Generación de documentos PDF |
| **MailKit** | 4.14.1 | Envío de emails SMTP |
| **Swagger/OpenAPI** | 7.2.0 | Documentación de API |
| **DotNetEnv** | 3.1.1 | Manejo de variables de entorno |

### Frontend
| Tecnología | Versión | Descripción |
|------------|---------|-------------|
| **Vue.js** | 3.5 | Framework JavaScript reactivo |
| **Vite** | 7.2.4 | Build tool y dev server |
| **Vue Router** | 4.6.3 | Enrutamiento SPA |
| **Pinia** | 3.0.4 | Estado global |
| **Axios** | 1.13.2 | Cliente HTTP |
| **Tailwind CSS** | 4.1.17 | Framework CSS utility-first |

### DevOps & Testing
| Tecnología | Descripción |
|------------|-------------|
| **Docker** | Contenedorización |
| **Docker Compose** | Orquestación de servicios |
| **Vitest** | Testing unitario |
| **Playwright** | Testing E2E |
| **ESLint** | Linting de código |
| **Prettier** | Formateo de código |

---

## 🏗 Arquitectura

El proyecto sigue una **Arquitectura Limpia (Clean Architecture)** simplificada:

```
TalentoPlus/
├── TalentoPlus.Core/           # Capa de dominio (sin dependencias externas)
│   ├── Entities/               # Entidades del dominio
│   ├── DTOs/                   # Data Transfer Objects
│   └── Interfaces/             # Contratos de servicios y repositorios
│
├── TalentoPlus.Infrastructure/ # Capa de infraestructura
│   ├── Data/                   # DbContext y configuraciones EF
│   ├── Repositories/           # Implementación de repositorios
│   ├── Services/               # Servicios externos (Email, PDF, Excel, IA)
│   └── Migrations/             # Migraciones de base de datos
│
└── TalentoPlus.API/            # Capa de presentación
    ├── Controllers/            # Controladores REST
    ├── Extensions/             # Extensiones y configuración
    └── Properties/             # Configuración de lanzamiento
```

### Diagrama de Base de Datos

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│   estado    │     │  departamento │     │    cargo    │
├─────────────┤     ├──────────────┤     ├─────────────┤
│ estado_id   │◄────│              │     │ cargo_id    │
│ nombre      │     │ departamento_│◄────│ nombre      │
└─────────────┘     │   id         │     └─────────────┘
       ▲            │ nombre       │            ▲
       │            └──────────────┘            │
       │                   ▲                    │
       │                   │                    │
       │    ┌──────────────┴───────────────┐   │
       │    │          empleado             │   │
       │    ├───────────────────────────────┤   │
       └────│ documento (PK)                │───┘
            │ nombres, apellidos            │
            │ fecha_nacimiento              │
            │ direccion, telefono, email    │
            │ salario, fecha_ingreso        │
            │ perfil_profesional            │
            │ password_hash                 │
            │ estado_id (FK)                │
            │ nivel_educativo_id (FK)       │
            │ departamento_id (FK)          │
            │ cargo_id (FK)                 │
            └───────────────────────────────┘
                           │
                           ▼
                 ┌─────────────────┐
                 │ nivel_educativo │
                 ├─────────────────┤
                 │ nivel_educativo │
                 │   _id           │
                 │ nombre          │
                 └─────────────────┘
```

---

## 📦 Instalación

### Prerrequisitos

- **Node.js** >= 20.19.0 o >= 22.12.0
- **.NET SDK** 9.0
- **PostgreSQL** 16+ (o Docker)
- **Docker & Docker Compose** (opcional, recomendado)

### Opción 1: Usando Docker (Recomendado) 🐳

1. **Clonar el repositorio:**
```bash
git clone https://github.com/ReijBak/Prueba-Modulo-CSharp-Final.git
cd Prueba-Modulo-CSharp-Final
```

2. **Configurar variables de entorno:**
```bash
cp .env.example .env
```

3. **Editar el archivo `.env`:**
```env
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=tu_password_seguro
POSTGRES_DB=talentoplusdb

# JWT
JWT_SECRET=tu_clave_secreta_muy_larga_minimo_32_caracteres
JWT_ISSUER=TalentoPlus
JWT_AUDIENCE=TalentoPlus

# OpenAI (para Dashboard con IA)
OPENAI_API_KEY=sk-tu_api_key_de_openai

# SMTP (para envío de emails)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu_email@gmail.com
SMTP_PASSWORD=tu_app_password
SMTP_FROM_EMAIL=tu_email@gmail.com
SMTP_FROM_NAME=TalentoPlus
```

4. **Iniciar los servicios:**
```bash
docker-compose up -d
```

5. **Acceder a la aplicación:**
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Opción 2: Instalación Manual 🔧

#### Backend

1. **Navegar al directorio del backend:**
```bash
cd TalentoPlus
```

2. **Crear archivo `.env` en `TalentoPlus.API/`:**
```env
DATABASE_URL=Host=localhost;Port=5432;Database=talentoplusdb;Username=postgres;Password=tu_password
JWT_SECRET=tu_clave_secreta_muy_larga_minimo_32_caracteres
JWT_ISSUER=TalentoPlus
JWT_AUDIENCE=TalentoPlus
OPENAI_API_KEY=sk-tu_api_key
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu_email@gmail.com
SMTP_PASSWORD=tu_app_password
SMTP_FROM_EMAIL=tu_email@gmail.com
```

3. **Restaurar dependencias:**
```bash
dotnet restore
```

4. **Crear la base de datos:**
```bash
# Ejecutar el script db.sql en PostgreSQL
psql -U postgres -f ../db.sql
```

5. **Ejecutar migraciones:**
```bash
cd TalentoPlus.API
dotnet ef database update --project ../TalentoPlus.Infrastructure
```

6. **Ejecutar el backend:**
```bash
dotnet run
```

El API estará disponible en `http://localhost:5062`

#### Frontend

1. **Navegar al directorio del frontend:**
```bash
cd Frontend
```

2. **Instalar dependencias:**
```bash
npm install
```

3. **Configurar API URL (si es diferente):**
Editar `src/services/api.js` si la URL del backend es diferente.

4. **Ejecutar en modo desarrollo:**
```bash
npm run dev
```

El frontend estará disponible en `http://localhost:5173`

---

## 🚀 Uso

### Primer Inicio

1. **Registrar un administrador:**
   - Ir a `/register`
   - Crear cuenta con email y contraseña

2. **Iniciar sesión como admin:**
   - Ir a `/login`
   - Seleccionar "Administrador"
   - Ingresar credenciales

3. **Importar empleados (opcional):**
   - Ir a "Importar Excel"
   - Subir archivo `.xlsx` con el formato correcto

### Formato del Excel para Importación

| Columna | Campo | Tipo | Requerido |
|---------|-------|------|-----------|
| A | Documento | Número | ✅ |
| B | Nombres | Texto | ✅ |
| C | Apellidos | Texto | ✅ |
| D | Fecha Nacimiento | Fecha | ✅ |
| E | Dirección | Texto | ❌ |
| F | Teléfono | Texto | ❌ |
| G | Email | Texto | ❌ |
| H | Cargo | Texto | ✅ |
| I | Salario | Número | ❌ |
| J | Fecha Ingreso | Fecha | ✅ |
| K | Estado | Texto | ✅ |
| L | Nivel Educativo | Texto | ✅ |
| M | Perfil Profesional | Texto | ❌ |
| N | Departamento | Texto | ✅ |

### Login de Empleados

Los empleados pueden iniciar sesión con:
- **Usuario:** Su número de documento
- **Contraseña:** Su número de documento (por defecto)

---

## 📚 Documentación API

### Endpoints Principales

#### Autenticación
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/auth/admin/register` | Registro de administrador |
| `POST` | `/api/auth/admin/login` | Login de administrador |
| `POST` | `/api/auth/employee-login` | Login de empleado |

#### Empleados
| Método | Endpoint | Descripción | Acceso |
|--------|----------|-------------|--------|
| `GET` | `/api/empleados` | Listar empleados | Admin: todos, Empleado: solo él |
| `GET` | `/api/empleados/{documento}` | Obtener empleado | Admin: cualquiera, Empleado: solo él |
| `POST` | `/api/empleados` | Crear empleado | Solo Admin |
| `PUT` | `/api/empleados/{documento}` | Actualizar empleado | Solo Admin |
| `DELETE` | `/api/empleados/{documento}` | Eliminar empleado | Solo Admin |
| `POST` | `/api/empleados/import` | Importar desde Excel | Solo Admin |
| `GET` | `/api/empleados/{documento}/resume` | Descargar PDF | Admin: cualquiera, Empleado: solo él |

#### Catálogos
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/catalogs/estados` | Listar estados |
| `GET` | `/api/catalogs/departamentos` | Listar departamentos |
| `GET` | `/api/catalogs/cargos` | Listar cargos |
| `GET` | `/api/catalogs/niveles-educativos` | Listar niveles educativos |

#### Dashboard IA
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/dashboard/query` | Consulta en lenguaje natural |
| `GET` | `/api/dashboard/stats` | Estadísticas generales |

### Documentación Swagger

Con el backend corriendo, accede a:
- **Swagger UI:** `http://localhost:5062/swagger`

---

## 🧪 Testing

### Backend
```bash
cd TalentoPlus
dotnet test
```

### Frontend
```bash
cd Frontend

# Tests unitarios
npm run test:unit

# Tests E2E
npm run test:e2e
```

---

## 📁 Estructura del Proyecto

```
Prueba-Modulo-CSharp-Final/
│
├── 📂 TalentoPlus/                 # Backend .NET
│   ├── 📂 TalentoPlus.API/         # Capa de presentación
│   ├── 📂 TalentoPlus.Core/        # Capa de dominio
│   ├── 📂 TalentoPlus.Infrastructure/ # Capa de infraestructura
│   └── 📄 Dockerfile
│
├── 📂 Frontend/                    # Frontend Vue.js
│   ├── 📂 src/
│   │   ├── 📂 components/          # Componentes reutilizables
│   │   ├── 📂 views/               # Vistas/Páginas
│   │   ├── 📂 services/            # Servicios API
│   │   ├── 📂 stores/              # Estado global (Pinia)
│   │   ├── 📂 router/              # Configuración de rutas
│   │   └── 📂 assets/              # Recursos estáticos
│   └── 📄 Dockerfile
│
├── 📄 db.sql                       # Script de base de datos
├── 📄 docker-compose.yml           # Orquestación Docker
├── 📄 Empleados.xlsx               # Archivo de ejemplo para importación
└── 📄 README.md                    # Este archivo
```

---

## 🔒 Seguridad

- ✅ Autenticación JWT con expiración configurable
- ✅ Contraseñas hasheadas con ASP.NET Core Identity
- ✅ Variables sensibles en archivos `.env`
- ✅ CORS configurado
- ✅ Validación de datos en backend y frontend
- ✅ Control de acceso basado en roles (RBAC)

---

## 🤝 Contribuir

1. Fork el proyecto
2. Crea tu rama de feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 👥 Autor

**Juan Steven Cardona Grisales**
- GitHub: [@ReijBak](https://github.com/ReijBak)

---

<div align="center">

**⭐ Si este proyecto te fue útil, considera darle una estrella en GitHub ⭐**

Hecho con ❤️ usando .NET 9, Vue.js 3 y PostgreSQL

</div>

