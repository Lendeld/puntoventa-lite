using PuntoVenta.Domain.Entities;

namespace PuntoVenta.UnitTests.Domain;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Entity_ShouldGenerateId_WhenCreatedWithoutId()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entity_ShouldUseProvidedId_WhenCreatedWithId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var entity = new TestEntity(expectedId);

        // Assert
        Assert.Equal(expectedId, entity.Id);
    }

    [Fact]
    public void Entity_ShouldBeEqual_WhenSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void Entity_ShouldNotBeEqual_WhenDifferentIds()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
    }
}
