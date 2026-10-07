# Lectura opcional - Similitud coseno

## El cálculo

Para comparar dos vectores de igual dimensión:

```text
producto punto = Σ (a[i] × b[i])
magnitud A = √(Σ a[i]²)
magnitud B = √(Σ b[i]²)

similitud coseno = producto punto / (magnitud A × magnitud B)
```

El producto punto multiplica las componentes correspondientes y suma los resultados. La magnitud mide la longitud de cada vector.

Dividir por ambas magnitudes permite comparar su dirección sin que la longitud por sí sola determine el resultado.

## Interpretación

- cercano a 1: direcciones similares, mayor similitud;
- cercano a 0: poca relación según esta medida;
- negativo: direcciones opuestas.

El rango matemático es de -1 a 1. Un score no es una probabilidad ni un porcentaje de coincidencia.

Los modelos de embeddings aprenden representaciones que permiten relacionar cercanía vectorial con semántica. En el ejercicio esperamos que la consulta tenga mayor similitud con el Documento A.

## Validación y límites

`VectorSimilarity.CosineSimilarity` recibe dos `ReadOnlySpan<float>`. Rechaza dimensiones diferentes, vectores vacíos, componentes no finitas y magnitud cero mediante `ArgumentException` con un mensaje claro. Acumula producto punto y cuadrados en `double`.

La comparación requiere el mismo modelo y configuración, además de igual dimensión. La similitud depende de la calidad del modelo y no prueba que un documento responda una pregunta. No existe un umbral universal de relevancia.

Solo comparamos y ordenamos dos documentos en memoria. No implementamos búsqueda aproximada, filtros ni re-ranking.
