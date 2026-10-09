# Instrucciones del repositorio

## Convención de nombres de laboratorios

Respetar estrictamente esta convención al crear, renombrar o documentar directorios de laboratorios.

Laboratorio simple:

```text
lab-##-concepto-descripcion/
```

Ejemplo: `lab-07-embeddings/`.

Laboratorio con sublabs:

```text
lab-##-concepto-descripcion/
├── lab-##a-concepto-descripcion/
├── lab-##b-concepto-descripcion/
└── ...
```

Ejemplo real:

```text
lab-09-rag-basico/
├── lab-09a-rag-explicito/
└── lab-09b-rag-service/
```

Reglas obligatorias:

- Usar minúsculas y guiones.
- Usar un número de laboratorio de dos dígitos.
- Colocar la letra del sublab inmediatamente después del número.
- Mantener los sublabs dentro de la carpeta del laboratorio padre.
- Elegir nombres breves y descriptivos, siguiendo las convenciones existentes.
- Usar los mismos nombres y rutas en documentación, enlaces y comandos.

No utilizar variantes como `Lab09`, `Lab09a`, `Lab09-rag-basico`, `Lab09a-rag-explicito`, `lab09` o `lab_09` para los directorios.

Esta convención corresponde a los directorios de laboratorios y sublabs. Los archivos `.csproj` y los tipos C# conservan sus convenciones existentes.
