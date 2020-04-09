using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class MptxManagerTests
    {
        [Fact]
        public void CreateInstance_CreatesNewStateForTypeNotInServices()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = manager.CreateInstance(typeof(FlowModel), contextItems: null);

            // Assert
            Assert.IsType<FlowModel>(instance.State);
        }

        [Fact]
        public void CreateInstance_CreatesNewStateWithDependencies()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            services.AddSingleton<Dependency>();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = manager.CreateInstance(typeof(FlowModelWithDI), contextItems: null);

            // Assert
            Assert.IsType<FlowModelWithDI>(instance.State);
        }

        [Fact]
        public void CreateInstance_StateTypeDoesNotInheritFromIMptxStateThrowsInvalidOperationException()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(
                () => manager.CreateInstance(typeof(BadFlowModel), contextItems: null));

            Assert.Equal($"State type must implement {typeof(IMptxState).FullName}. (Parameter 'stateType')", ex.Message);
        }

        private class BadFlowModel { }

        private class FlowModel : IMptxState
        {
        }

        private class Dependency { }

        private class FlowModelWithDI : IMptxState
        {
            public FlowModelWithDI(Dependency dependency)
            {
            }
        }

        private class FlowModelWithInitializer : IMptxState
        {
            public int Foo { get; set; }
        }

        private class FlowModelInitializer
        {
            public Task Initialize(FlowModelWithInitializer state)
            {
                state.Foo = 42;
                return Task.CompletedTask;
            }
        }
    }
}
