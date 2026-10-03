# Configuración del taller

## Por qué Lab 00 no usa configuración externa

Lab 00 busca demostrar la llamada mínima posible.

Por eso los ejemplos contienen un placeholder directamente en el código:

```csharp
const string apiKey = "TU_API_KEY";
```

Es una excepción pedagógica y no debe convertirse en una práctica del resto del proyecto.

## Configuración común

A partir del Lab 01 el proyecto utiliza un `.env` común en la raíz.

La convención simplificada es:

```env
AI_PROVIDER=
AI_MODEL=
AI_API_KEY=
AI_URL=
```

## Variables

### AI_PROVIDER

Identifica el proveedor activo.

Ejemplos:

```text
openai
gemini
openrouter
```

### AI_MODEL

Nombre exacto del modelo a utilizar.

Es obligatorio para evitar defaults ocultos en el código del laboratorio.

### AI_API_KEY

Credencial para acceder al servicio.

Es obligatoria para los proveedores remotos utilizados actualmente por el taller.

### AI_URL

Endpoint alternativo.

Puede quedar vacío cuando el cliente utiliza su endpoint por defecto.

Es necesario cuando accedemos a un proveedor mediante un endpoint compatible distinto del endpoint estándar.

## Objetivo pedagógico

El código de un laboratorio debería mostrar principalmente el concepto que ese laboratorio enseña.

La infraestructura de configuración debe quedar encapsulada en una pieza auxiliar.

```text
Código necesario para ejecutar
        ≠
Código necesario para comprender el concepto
```
