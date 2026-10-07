# 01 - Entorno de desarrollo

## IDE

El entorno de trabajo previsto es **Visual Studio Code Insiders**.

## Requisitos previstos

- Visual Studio Code Insiders
- .NET 10 SDK
- Git
- acceso a al menos un proveedor de LLM utilizado en los laboratorios

## Extensiones sugeridas

La carpeta `.vscode` contiene recomendaciones mínimas para trabajar con C# y .NET.

Inicialmente:

- C# Dev Kit
- C#

Otras extensiones podrán incorporarse cuando aparezca una necesidad concreta.

## Verificación del entorno

Al comenzar con el primer laboratorio conviene verificar:

```text
dotnet --info
git --version
code-insiders --version
```

---

## Configuración común del repositorio

El proyecto utiliza **un único archivo `.env` en la raíz del repositorio**.

No se utilizará un `.env` independiente por laboratorio salvo que, en el futuro, exista una necesidad concreta que lo justifique.

La estructura esperada es:

```text
dac-learning-ai-dotnet/
├── .env
├── .env.example
├── README.md
├── ROADMAP.md
├── docs/
└── labs/
    ├── lab-01-hello-llm/
    │   ├── Lab01.HelloLlm.csproj
    │   └── Program.cs
    ├── lab-02-chat-history/
    └── ...
```

### Motivo

Centralizar la configuración permite:

- evitar duplicación de API keys;
- mantener una configuración consistente entre laboratorios;
- cambiar de proveedor una sola vez;
- reducir el riesgo de dejar secretos distribuidos en múltiples carpetas.

### `.env`

Contiene la configuración real del desarrollador.

Ejemplo:

```env
AI_PROVIDER=gemini
AI_MODEL=gemini-3.5-flash-lite
AI_EMBEDDING_MODEL=gemini-embedding-001
AI_API_KEY=tu-api-key-real
AI_URL=https://generativelanguage.googleapis.com/v1beta/openai/
```

Este archivo **no debe versionarse**.

Debe permanecer incluido en `.gitignore`.

### `.env.example`

Documenta las variables necesarias sin contener secretos reales.

Ejemplo:

```env
AI_PROVIDER=gemini
AI_MODEL=gemini-3.5-flash-lite
AI_EMBEDDING_MODEL=gemini-embedding-001
AI_API_KEY=tu-api-key
AI_URL=https://generativelanguage.googleapis.com/v1beta/openai/
```

Este archivo **sí debe versionarse**.

Cuando otra persona clone el repositorio podrá copiarlo:

```powershell
Copy-Item .env.example .env
```

y completar únicamente los valores que necesite.

---

## Proveedores de modelos

El proyecto no debe exigir una cuenta paga de un proveedor concreto para poder comenzar.

Para los primeros laboratorios se contemplan OpenAI, Gemini y OpenRouter.

Todos utilizan `AI_API_KEY` para la credencial activa. `AI_PROVIDER` identifica el proveedor; `AI_URL` permite indicar un endpoint alternativo compatible. Los ejemplos concretos están en [el README raíz](../README.md#ejemplos-de-configuración).

La selección se realiza mediante:

```text
AI_PROVIDER=openai
AI_PROVIDER=gemini
AI_PROVIDER=openrouter
```

Los modelos se configuran explícitamente según la capacidad necesaria:

```text
AI_MODEL=<modelo-generativo-chat>
AI_EMBEDDING_MODEL=<modelo-de-embeddings>
```

Hasta Lab 06 basta con el modelo de chat. Lab 07 necesita el modelo de embeddings y no utiliza `AI_MODEL`. Lab 08 funciona offline, sin `.env` ni modelos. Lab 09a y Lab 09b requieren ambos modelos para recuperar fragmentos y generar una respuesta con contexto. No todos los modelos soportan ambas capacidades, aunque sean del mismo proveedor. Esta evolución mantiene la configuración simple: agregamos la distinción al necesitarla.

Los planes gratuitos y sus límites pueden modificarse por decisión de cada proveedor.

---

## Criterio de accesibilidad

El costo de una API no debería impedir completar el recorrido educativo.

Por ese motivo:

1. cuando sea razonable, los laboratorios ofrecerán una alternativa online gratuita;
2. la implementación seguirá priorizando el concepto que se desea enseñar;
3. no se agregará complejidad únicamente para soportar muchos proveedores;
4. más adelante podrán documentarse modelos locales mediante Ollama como alternativa adicional.

Esta política es transversal y no modifica el roadmap pedagógico.

---

## Privacidad

Los laboratorios deben utilizar exclusivamente datos ficticios o públicos.

No utilizar:

- datos personales reales;
- credenciales;
- información bancaria;
- código confidencial;
- documentos internos;
- información protegida de clientes.

---

## Variables y secretos

Las claves de API nunca deberán guardarse en el repositorio.

Para este proyecto la configuración común se centraliza en:

```text
<raíz-del-repositorio>/.env
```

Los laboratorios que cargan configuración de IA reutilizan ese archivo. Lab 08 no lo necesita; Lab 00 conserva sus ejemplos mínimos con un placeholder de API key en el código.

Más adelante podrá utilizarse `User Secrets` de .NET si algún laboratorio lo justifica, pero no es necesario para los primeros ejercicios.
