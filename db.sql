-- 1. Tabla de Estado (Nueva entidad solicitada)
CREATE TABLE estado (
                        estado_id SERIAL PRIMARY KEY,
                        nombre_estado VARCHAR(50) NOT NULL UNIQUE
);

-- 2. Tabla de Departamento
CREATE TABLE departamento (
                              departamento_id SERIAL PRIMARY KEY,
                              nombre_departamento VARCHAR(100) NOT NULL UNIQUE
);

-- 3. Tabla de Cargo
CREATE TABLE cargo (
                       cargo_id SERIAL PRIMARY KEY,
                       nombre_cargo VARCHAR(100) NOT NULL UNIQUE
);

-- 4. Tabla de Nivel Educativo
CREATE TABLE nivel_educativo (
                                 nivel_educativo_id SERIAL PRIMARY KEY,
                                 nombre_nivel VARCHAR(100) NOT NULL UNIQUE
);

-- 5. Tabla de Empleado (Tabla principal)
CREATE TABLE empleado (
    -- Clave Primaria (PK)
                          documento BIGINT PRIMARY KEY, -- Usamos BIGINT para números de identificación potencialmente grandes

    -- Información Personal
                          nombres VARCHAR(100) NOT NULL,
                          apellidos VARCHAR(100) NOT NULL,
                          fecha_nacimiento DATE NOT NULL,
                          direccion VARCHAR(255),
                          telefono VARCHAR(20), -- VARCHAR para permitir formatos de número
                          email VARCHAR(150) UNIQUE, -- El email debe ser único

    -- Información Laboral
                          salario NUMERIC(10, 2), -- NUMERIC(total_digitos, decimales) para precisión monetaria
                          fecha_ingreso DATE NOT NULL,
                          perfil_profesional TEXT,

    -- Autenticación de empleados
                          password_hash VARCHAR(255), -- Hash de contraseña para login de empleados

    -- Claves Foráneas (FK)
                          estado_id INTEGER NOT NULL,
                          nivel_educativo_id INTEGER NOT NULL,
                          departamento_id INTEGER NOT NULL,
                          cargo_id INTEGER NOT NULL,

    -- Definición de Restricciones FOREIGN KEY
                          CONSTRAINT fk_estado
                              FOREIGN KEY (estado_id)
                                  REFERENCES estado (estado_id),

                          CONSTRAINT fk_nivel_educativo
                              FOREIGN KEY (nivel_educativo_id)
                                  REFERENCES nivel_educativo (nivel_educativo_id),

                          CONSTRAINT fk_departamento
                              FOREIGN KEY (departamento_id)
                                  REFERENCES departamento (departamento_id),

                          CONSTRAINT fk_cargo
                              FOREIGN KEY (cargo_id)
                                  REFERENCES cargo (cargo_id)
);