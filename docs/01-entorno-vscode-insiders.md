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

## Criterio de accesibilidad

El costo de una API no debería impedir completar el recorrido educativo.

Por ese motivo:

1. cuando sea razonable, los laboratorios ofrecerán una alternativa online gratuita;
2. la implementación seguirá priorizando el concepto que se desea enseñar;
3. no se agregará complejidad únicamente para soportar muchos proveedores;
4. más adelante podrán documentarse modelos locales mediante Ollama como alternativa adicional.

Esta política es transversal y no modifica el roadmap pedagógico.

## Privacidad

Los niveles gratuitos de servicios externos pueden tener políticas de tratamiento de datos diferentes de los niveles pagos.

Los laboratorios deben utilizar exclusivamente datos ficticios o públicos.

No utilizar:

- datos personales reales;
- credenciales;
- información bancaria;
- código confidencial;
- documentos internos;
- información protegida de clientes.

## Variables y secretos

Las claves de API nunca deberán guardarse en el repositorio.

Utilizar:

- variables de entorno;
- User Secrets de .NET cuando un laboratorio lo justifique;
- archivos locales ignorados por Git.

Ejemplo PowerShell:

```powershell
$env:AI_PROVIDER="gemini"
$env:GEMINI_API_KEY="tu-api-key"
```
