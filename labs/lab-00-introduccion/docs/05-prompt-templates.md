# Prompt Templates

## El problema

Un prompt escrito directamente dentro de un método mezcla estructura y valores:

```csharp
$"Traduce al inglés: {texto}"
```

Cuando el mismo patrón se reutiliza conviene separar:

```text
estructura fija
+
variables
```

## Ejemplo

```text
Traduce al {idioma}.

Texto:
{texto}
```

Los valores cambian, la intención se mantiene.

## En C#

Durante Fundamentos utilizamos raw interpolated strings:

```csharp
return $$"""
Traduce al {{idioma}}.

Texto:
{{texto}}
""";
```

## Qué necesidad resuelve

- hace visible la estructura del prompt;
- evita repetir texto fijo;
- permite parametrizar instrucciones;
- separa mejor responsabilidades.

## Dónde aparece

Lab 04 introduce `PromptTemplates` como una pieza explícita.
