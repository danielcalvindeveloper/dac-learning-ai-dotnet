using Microsoft.Extensions.AI;
using Xunit;

namespace RagSemanticChunking.Tests;

/// <summary>Prueba decisiones semánticas con vectores conocidos, separando el algoritmo del proveedor.</summary>
public sealed class SemanticTests
{
    private static ChunkingSettings Settings => new(MaxSize: 80, MinSize: 10, Overlap: 5, UnitMaxSize: 40);
    private static DocumentText ThreeParagraphs => new("a.md", new string('a', 30) + "\n\n" + new string('b', 30) + "\n\n" + new string('c', 30));

    [Fact]
    public async Task ControlledVectorsExposeAdjacentSequenceAndTopicBoundary()
    {
        // Dos vectores iguales tienen coseno 1; vectores ortogonales tienen coseno 0.
        // Esta secuencia provoca un cambio de tema conocido sin depender de resultados de un modelo real.
        using ControlledGenerator generator = new([[1, 0], [1, 0], [0, 1]]);
        DocumentText document = ThreeParagraphs;
        ChunkingResult result = await new EmbeddingSemanticChunkingStrategy(Settings, generator).ChunkAsync(document, TestContext.Current.CancellationToken);
        Assert.Equal([1d, 0d], result.AdjacentSimilarities);
        Assert.Equal(3, result.AdditionalEmbeddings);
        Assert.Equal(2, result.Chunks.Count);
        Assert.Equal(64, result.Chunks[0].End);
        TextPartition.ValidateCoverage(document, result.Chunks, 80, false);
    }

    [Fact]
    public void ThresholdIsStrictAndMinimumRejectsPrematureBoundary()
    {
        DocumentText document = ThreeParagraphs;
        IReadOnlyList<TextRange> units = document.Units(40);
        IReadOnlyList<TextRange> strict = EmbeddingSemanticChunkingStrategy.Partition(document, units, [.72, 1], Settings);
        Assert.Equal(64, strict[0].End); // 0.72 no es menor que 0.72: corta por máximo, no por tema.
        Assert.Equal("max size", strict[0].Reason);
        ChunkingSettings largerMinimum = new(MaxSize: 100, MinSize: 40, Overlap: 5, UnitMaxSize: 50);
        IReadOnlyList<TextRange> grouped = EmbeddingSemanticChunkingStrategy.Partition(document, document.Units(50), [0, 0], largerMinimum);
        Assert.Single(grouped); // Ninguna división permite dos segmentos de al menos 40.
    }

    [Fact]
    public void ForcedMaximumLeavesNoTinyTail()
    {
        DocumentText document = new("a.md", new string('a', 350));
        IReadOnlyList<TextRange> units = document.Units(40);
        IReadOnlyList<TextRange> result = EmbeddingSemanticChunkingStrategy.Partition(document, units,
            Enumerable.Repeat(1d, units.Count - 1).ToArray(), Settings);
        Assert.All(result, r => Assert.InRange(r.Length, 10, 80));
        TextPartition.ValidateCoverage(document, document.Build(result), 80, false);
    }

    [Fact]
    public void InvalidVectorsAndSimilaritySequenceAreClearErrors()
    {
        Assert.Throws<ArgumentException>(() => EmbeddingSemanticChunkingStrategy.Cosine([0, 0], [1, 0]));
        Assert.Throws<ArgumentException>(() => EmbeddingSemanticChunkingStrategy.Cosine([1], [1, 0]));
        Assert.Throws<ArgumentException>(() => EmbeddingSemanticChunkingStrategy.Cosine([float.NaN], [1]));
        Assert.Throws<ArgumentException>(() => EmbeddingSemanticChunkingStrategy.Partition(ThreeParagraphs, ThreeParagraphs.Units(40), [1], Settings));
    }

    [Fact]
    public async Task ProviderShapeIsValidatedBeforeChunkConstruction()
    {
        using ControlledGenerator generator = new([[1, 0]]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new EmbeddingSemanticChunkingStrategy(Settings, generator)
            .ChunkAsync(ThreeParagraphs, TestContext.Current.CancellationToken));
    }
}

/// <summary>Doble de prueba de la abstracción existente; devuelve exactamente los vectores suministrados.</summary>
/// <remarks>Puede devolver una forma inválida deliberadamente para comprobar la validación de la aplicación.</remarks>
internal sealed class ControlledGenerator(float[][] vectors) : IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(vectors.Select(v => new Embedding<float>(v))));
    }
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
