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
        public async Task CreateInstance_CreatesNewStateForTypeNotInServices()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = await manager.CreateInstance("TestFlow", typeof(FlowModel), contextItems: null);

            // Assert
            Assert.IsType<FlowModel>(instance.State);
        }

        [Fact]
        public async Task CreateInstance_CreatesNewStateWithDependencies()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            services.AddSingleton<Dependency>();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = await manager.CreateInstance(
                "TestFlow",
                typeof(FlowModelWithDI),
                contextItems: null);

            // Assert
            Assert.IsType<FlowModelWithDI>(instance.State);
        }

        [Fact]
        public async Task CreateInstance_InitializesStateWithInitializerFromServices()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            services.AddSingleton<IInitializeMptxState<FlowModelWithInitializer>, FlowModelInitializer>();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = await manager.CreateInstance(
                "TestFlow",
                typeof(FlowModelWithInitializer),
                contextItems: null);

            // Assert
            var state = Assert.IsType<FlowModelWithInitializer>(instance.State);
            Assert.Equal(42, state.Foo);
        }

        [Fact]
        public async Task CreateInstance_StateTypeDoesNotInheritFromIMptxStateThrowsInvalidOperationException()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => manager.CreateInstance("TestFlow", typeof(BadFlowModel), contextItems: null));

            Assert.Equal($"State type must implement {typeof(IMptxState).FullName}. (Parameter 'stateType')", ex.Message);
        }

        [Fact]
        public async Task CreateInstance_WithProvidedState_CallsInitializer()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            services.AddSingleton<IInitializeMptxState<FlowModelWithInitializer>, FlowModelInitializer>();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            // Act
            var instance = await manager.CreateInstance("TestFlow", new FlowModelWithInitializer());

            // Assert
            Assert.Equal(42, instance.State.Foo);
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

        private class FlowModelInitializer : IInitializeMptxState<FlowModelWithInitializer>
        {
            public Task Initialize(MptxInstanceContext<FlowModelWithInitializer> context)
            {
                context.Update(s => s.Foo = 42);
                return Task.CompletedTask;
            }
        }
    }
}
