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

## Prácticas para implementaciones de referencia

`reference/` complementa los labs con aplicaciones ejecutables, pequeñas y mantenibles. No usa numeración de Lab ni modifica sus milestones. Usar carpetas en minúsculas, con guiones y nombres descriptivos.

Código maduro no significa arquitectura enterprise. Aplicar KISS, buen diseño y testabilidad:

- Separar responsabilidades naturales; `Program.cs` compone dependencias y ejecuta comandos.
- Usar nombres claros, métodos acotados, tipos adecuados y nullable reference types.
- Evitar duplicación, estado global mutable y constantes relevantes sin nombre.
- Validar entradas y configuración externa; no registrar credenciales ni datos sensibles.
- Propagar `CancellationToken` en operaciones asíncronas donde aporte valor y liberar recursos con `using`/`await using` cuando corresponda.
- Manejar explícitamente errores relevantes con mensajes útiles; capturar `Exception` genéricamente sólo en el borde de la aplicación.
- Utilizar DI y logging cuando mejoren composición, testabilidad o diagnóstico; no agregarlos por demostrar un patrón ni reemplazar la salida pedagógica con ruido técnico.
- No crear interfaces con una única implementación sin una razón concreta, factories triviales, repositorios genéricos, capas vacías, herencia artificial ni múltiples proyectos innecesarios.

Antes de elegir dependencias, verificar APIs, versiones, estado estable/preview y compatibilidad en documentación oficial. Primero el problema, después la herramienta. Incorporar frameworks sólo cuando resuelvan una responsabilidad visible.

Incluir tests útiles y deterministas de lógica propia: transformaciones, IDs, chunking, configuración, contexto, selección de fuentes y caminos sin evidencia. Preferir funciones pequeñas y abstracciones existentes antes que interfaces creadas sólo para mockear. Los unit tests deben poder ejecutarse sin LLM, red, Docker ni infraestructura externa cuando sea razonable.

No intentar probar internamente SDKs o bases vectoriales ni afirmar textos exactos de un LLM o scores sin garantía contractual. Si se agregan tests de integración, separarlos e indicar cómo ejecutarlos y qué infraestructura requieren; los unit tests no deben depender de ellos.

Cada referencia debe cumplir las [reglas de documentación](reference/README.md#documentación-obligatoria-de-cada-implementación): explicar problema, labs previos, arquitectura, dependencias y su motivo, preparación del entorno, configuración, infraestructura, ejecución, recorrido del código, tests, diferencias y limitaciones. Enlazar fuentes oficiales y distinguir planificación de implementación.

Verificar realmente `dotnet restore`, `dotnet build` y `dotnet test`; corregir errores y revisar warnings nuevos sin silenciarlos injustificadamente. Ejecutar y comprobar la infraestructura y los flujos reales cuando el entorno lo permita. Informar exactamente lo que se ejecutó y lo que quedó sin verificar.
