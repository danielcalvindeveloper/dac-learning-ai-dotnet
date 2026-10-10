# Qué describe una Tool y qué ejecuta C#

Una Tool permite que el modelo solicite una capacidad de la aplicación. En Lab 11a ofrecemos `CalcularTotalConIva` mediante `AIFunctionFactory.Create`.

## Descripción e implementación

El modelo necesita conocer el nombre de la operación, una descripción de cuándo utilizarla y los parámetros que debe enviar.

La factory obtiene el esquema de parámetros a partir de la firma C#. Ambos parámetros son `decimal`: `importe` es el valor sin impuesto y `porcentaje` expresa el IVA, por ejemplo `21` para el 21%.

No agregamos atributos porque `Create` permite indicar directamente el nombre y la descripción. La implementación permanece como una función local de `Program.cs`.

## Argumentos recibidos

El modelo puede solicitar la función con argumentos equivalentes a:

```json
{
  "importe": 1000,
  "porcentaje": 21
}
```

Esos valores llegan en `FunctionCallContent.Arguments`. Son datos de una solicitud, no una ejecución. El programa muestra los valores y verifica que el nombre corresponda a la única Tool disponible.

## Invocación explícita

```csharp
AIFunctionArguments arguments = new(functionCall.Arguments);
object? result = await tool.InvokeAsync(arguments);
```

`AIFunction` adapta esos argumentos a los parámetros del método y ejecuta el delegado C# dentro del proceso .NET. El método devuelve un `decimal`; por defecto, la función creada por `AIFunctionFactory` lo representa como un `JsonElement` en el resultado de `InvokeAsync`. Por eso el código recibe `object?`, sin convertirlo manualmente para devolverlo al modelo.

Esto no automatiza Function Calling. La aplicación todavía debe detectar la solicitud, crear el mensaje de resultado y reenviar el historial.

## Responsabilidad de la aplicación

El modelo no recibe el cuerpo del método ni puede ejecutarlo dentro del LLM. Ofrecer una Tool tampoco implica que cualquier solicitud deba ejecutarse. Sólo aceptamos una llamada al nombre conocido. La validación de negocio y la recuperación ante argumentos inválidos quedan para más adelante.

La función es local: no realiza una llamada HTTP ni utiliza otro servicio. Tenerla disponible no convierte el ejercicio en un agente.

[Volver al laboratorio](../README.md).
