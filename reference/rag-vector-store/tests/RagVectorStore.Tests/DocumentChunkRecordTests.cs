using Microsoft.Extensions.VectorData;
using Xunit;

namespace RagVectorStore.Tests;

public sealed class DocumentChunkRecordTests
{
    [Fact]
    public void SameDocumentAndPositionUpdateSameIdEvenWhenTextChanges()
    {
        DocumentChunkRecord original = DocumentChunkRecord.FromChunk("a.md", new("before", "a.md", 0), new float[] { 1, 2 });
        DocumentChunkRecord updated = DocumentChunkRecord.FromChunk("a.md", new("after", "a.md", 0), new float[] { 3, 4 });
        DocumentChunkRecord next = DocumentChunkRecord.FromChunk("a.md", new("before", "a.md", 1), new float[] { 1, 2 });
        DocumentChunkRecord other = DocumentChunkRecord.FromChunk("b.md", new("before", "b.md", 0), new float[] { 1, 2 });

        Assert.Equal(original.Id, updated.Id);
        Assert.NotEqual(original.Id, next.Id);
        Assert.NotEqual(original.Id, other.Id);
        Assert.Equal(3, new[] { original, updated, next, other }.Select(record => record.Id).Distinct().Count());
    }

    [Fact]
    public void MappingPreservesMetadataTextAndVector()
    {
        DocumentChunkRecord record = DocumentChunkRecord.FromChunk("document", new("content", "guide.md", 4), new float[] { 1, 2 });
        Assert.Equal("document", record.DocumentId);
        Assert.Equal("guide.md", record.Source);
        Assert.Equal(4, record.ChunkIndex);
        Assert.Equal("content", record.Text);
        Assert.Equal([1f, 2f], record.Embedding.ToArray());
    }

    [Fact]
    public void SchemaUsesActualDimensionAndCosine()
    {
        VectorStoreCollectionDefinition definition = DocumentChunkRecord.Definition(768);
        Assert.Single(definition.Properties.OfType<VectorStoreKeyProperty>());
        Assert.Equal(4, definition.Properties.OfType<VectorStoreDataProperty>().Count());
        VectorStoreVectorProperty vector = Assert.Single(definition.Properties.OfType<VectorStoreVectorProperty>());
        Assert.Equal(768, vector.Dimensions);
        Assert.Equal(DistanceFunction.CosineSimilarity, vector.DistanceFunction);
        Assert.Equal(IndexKind.Hnsw, vector.IndexKind);
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentChunkRecord.Definition(0));
    }
}
