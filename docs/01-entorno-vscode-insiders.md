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
AI_MODEL=

OPENAI_API_KEY=
GEMINI_API_KEY=tu-api-key-real
OPENROUTER_API_KEY=
```

Este archivo **no debe versionarse**.

Debe permanecer incluido en `.gitignore`.

### `.env.example`

Documenta las variables necesarias sin contener secretos reales.

Ejemplo:

```env
AI_PROVIDER=gemini
AI_MODEL=

OPENAI_API_KEY=tu-openai-api-key
GEMINI_API_KEY=tu-gemini-api-key
OPENROUTER_API_KEY=tu-openrouter-api-key
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

Para los primeros laboratorios se contemplan:

| Proveedor | Variable de API key | Observación |
|---|---|---|
| OpenAI | `OPENAI_API_KEY` | proveedor utilizado originalmente |
| Gemini | `GEMINI_API_KEY` | dispone de nivel gratuito para determinados modelos |
| OpenRouter | `OPENROUTER_API_KEY` | dispone de modelos gratuitos y del router `openrouter/free` |

La selección se realiza mediante:

```text
AI_PROVIDER=openai
AI_PROVIDER=gemini
AI_PROVIDER=openrouter
```

El modelo puede sobrescribirse mediante:

```text
AI_MODEL=<modelo>
```

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

Los laboratorios deben reutilizar ese archivo.

Más adelante podrá utilizarse `User Secrets` de .NET si algún laboratorio lo justifica, pero no es necesario para los primeros ejercicios.
