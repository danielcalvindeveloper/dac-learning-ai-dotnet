# Lab 02 - Chat History

## Objetivo

Comprender la diferencia entre realizar llamadas independientes a un modelo y mantener el historial de una conversación.

Este laboratorio replica el mismo ejercicio conceptual utilizado en la versión Java con LangChain4j.

La comparación se realiza con exactamente los mismos mensajes:

```text
Mi nombre es Daniel
¿Cómo me llamo?
```

---

## Caso 1 — Sin memoria

En este primer bloque se realizan dos llamadas completamente independientes.

```text
=== SIN MEMORIA ===

Usuario: Mi nombre es Daniel
Asistente: ...

Usuario: ¿Cómo me llamo?
Asistente: ...
```

El modelo recibe cada llamada por separado.

Conceptualmente:

```text
Llamada 1
Usuario: Mi nombre es Daniel
          ↓
         LLM

Llamada 2
Usuario: ¿Cómo me llamo?
          ↓
         LLM
```

La segunda llamada no contiene el mensaje anterior.

Por eso el modelo no debería poder saber que el nombre informado era Daniel.

El código es:

```csharp
Console.WriteLine($"Usuario: {firstMessage}");

ChatResponse r1 =
    await chatClient.GetResponseAsync(firstMessage);

Console.WriteLine($"Asistente: {r1.Text}");
```

y luego otra llamada independiente:

```csharp
Console.WriteLine($"Usuario: {secondMessage}");

ChatResponse r2 =
    await chatClient.GetResponseAsync(secondMessage);

Console.WriteLine($"Asistente: {r2.Text}");
```

---

## Caso 2 — Con memoria

Ahora mantenemos explícitamente el historial de mensajes:

```csharp
List<ChatMessage> history = [];
```

La salida queda visible como diálogo:

```text
=== CON MEMORIA ===

Usuario: Mi nombre es Daniel
Asistente: ...

Usuario: ¿Cómo me llamo?
Asistente: Te llamas Daniel.
```

Cada nuevo mensaje del usuario se agrega al historial:

```csharp
history.Add(
    new ChatMessage(ChatRole.User, message));
```

Luego se envía todo el historial al modelo:

```csharp
ChatResponse response =
    await chatClient.GetResponseAsync(history);
```

y finalmente se agrega también la respuesta del asistente:

```csharp
foreach (ChatMessage responseMessage in response.Messages)
{
    history.Add(responseMessage);
}
```

Conceptualmente:

```text
Usuario: Mi nombre es Daniel
            ↓
        historial
            ↓
           LLM
            ↓
Asistente: ...

Usuario: ¿Cómo me llamo?
            ↓
      historial completo
            ↓
           LLM
            ↓
Asistente: Te llamas Daniel
```

---

## Comparación directa

```text
SIN MEMORIA
───────────
Usuario: Mi nombre es Daniel
Asistente: Mucho gusto, Daniel.

Usuario: ¿Cómo me llamo?
Asistente: No tengo forma de saberlo.


CON MEMORIA
───────────
Usuario: Mi nombre es Daniel
Asistente: Mucho gusto, Daniel.

Usuario: ¿Cómo me llamo?
Asistente: Te llamas Daniel.
```

La diferencia no está en el modelo.

La diferencia está en **qué contexto envía la aplicación en cada llamada**.

---

## Relación con la versión Java

La versión Java utiliza:

```java
System.out.println("=== SIN MEMORIA ===");

String r1 = model.chat(
        "Mi nombre es Daniel"
);

String r2 = model.chat(
        "¿Como me llamo?"
);
```

y luego:

```java
System.out.println("=== CON MEMORIA ===");

Assistant assistant =
        AiServices.builder(Assistant.class)
                .chatModel(model)
                .chatMemory(
                        MessageWindowChatMemory.withMaxMessages(20)
                )
                .build();

String r3 = assistant.chat(
        "Mi nombre es Daniel"
);

String r4 = assistant.chat(
        "¿Como me llamo?"
);
```

En .NET todavía no introducimos una abstracción equivalente a `AiServices`.

Para que el mecanismo quede visible, mantenemos el historial explícitamente:

```csharp
List<ChatMessage> history = [];
```

La equivalencia conceptual es:

| LangChain4j | .NET |
|---|---|
| `model.chat(...)` | `IChatClient.GetResponseAsync(...)` |
| llamadas independientes | llamadas independientes |
| `MessageWindowChatMemory` | `List<ChatMessage>` |
| `assistant.chat(...)` | `ChatWithMemoryAsync(...)` |
| memoria gestionada por LangChain4j | historial gestionado por la aplicación |

---

## Estructura

```text
lab-02-chat-history/
├── Lab02.ChatHistory.csproj
├── Program.cs
└── README.md
```

El `.env` se mantiene en la raíz del repositorio:

```text
dac-learning-ai-dotnet/
├── .env
├── .env.example
└── labs/
    ├── lab-01-hello-llm/
    └── lab-02-chat-history/
```

No es necesario crear un `.env` por laboratorio.

---

## Configuración

Ejemplo:

```env
AI_PROVIDER=gemini
AI_MODEL=

OPENAI_API_KEY=
GEMINI_API_KEY=tu-api-key
OPENROUTER_API_KEY=
```

El laboratorio carga la configuración común mediante:

```csharp
Env.TraversePath().Load();
```

---

## Ejecutar

Desde:

```powershell
cd labs\lab-02-chat-history
```

ejecutar:

```powershell
dotnet restore
dotnet build
dotnet run
```

Una salida típica será:

```text
Proveedor: Gemini
Modelo: gemini-3.5-flash-lite

=== SIN MEMORIA ===
Usuario: Mi nombre es Daniel
Asistente: ¡Mucho gusto, Daniel!

Usuario: ¿Cómo me llamo?
Asistente: No tengo forma de saber tu nombre.

=== CON MEMORIA ===
Usuario: Mi nombre es Daniel
Asistente: ¡Mucho gusto, Daniel!

Usuario: ¿Cómo me llamo?
Asistente: Te llamas Daniel.
```

La redacción exacta depende del modelo.

---

## Qué queremos demostrar

### Sin memoria

```text
mensaje actual
      ↓
     modelo
      ↓
   respuesta
```

Cada llamada es independiente.

### Con memoria

```text
historial + mensaje nuevo
          ↓
         modelo
          ↓
       respuesta
          ↓
  historial actualizado
```

El modelo conoce el nombre porque la aplicación volvió a enviar el mensaje anterior.

---

## Punto importante

El modelo no conserva automáticamente el estado de nuestra aplicación entre llamadas.

La memoria conversacional existe porque la aplicación vuelve a enviar el contexto anterior.

En este laboratorio ese mecanismo queda deliberadamente visible.

Más adelante veremos abstracciones que permiten encapsular esta responsabilidad.

---

## Qué NO hacemos todavía

No incorporamos:

- Dependency Injection;
- servicios propios;
- persistencia;
- límite de ventana;
- resumen de conversaciones;
- Semantic Kernel;
- RAG;
- tools.

Tampoco implementamos todavía el equivalente exacto de:

```java
MessageWindowChatMemory.withMaxMessages(20)
```

Eso aparecerá cuando trabajemos memoria conversacional más avanzada.

---

## Resultado esperado

Al finalizar el laboratorio deberías poder explicar:

1. por qué dos llamadas independientes no comparten contexto;
2. qué representa `List<ChatMessage>`;
3. qué información se agrega al historial;
4. por qué la segunda llamada con memoria conoce el nombre;
5. por qué el historial pertenece a la aplicación y no al modelo;
6. cuál es la equivalencia conceptual con el ejercicio Java.

---

## Siguiente laboratorio

**Lab 03 - Services y Dependency Injection**

Después de comprender el mecanismo explícito, el siguiente paso será separar la configuración del cliente y la lógica de conversación del `Program.cs`.
