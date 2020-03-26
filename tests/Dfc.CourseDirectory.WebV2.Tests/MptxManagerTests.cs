using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
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

            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;

            // Act
            var instance = await manager.CreateInstance("TestFlow", typeof(FlowModel), request, Array.Empty<string>());

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

            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;

            // Act
            var instance = await manager.CreateInstance("TestFlow", typeof(FlowModelWithDI), request, Array.Empty<string>());

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

            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;

            // Act
            var instance = await manager.CreateInstance("TestFlow", typeof(FlowModelWithInitializer), request, Array.Empty<string>());

            // Assert
            var state = Assert.IsType<FlowModelWithInitializer>(instance.State);
            Assert.Equal(42, state.Foo);
        }

        [Fact]
        public async Task CreateInstance_AssignsItemsFromQueryParams()
        {
            // Arrange
            var stateProvider = new InMemoryMptxStateProvider();

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var instanceContextFactory = new MptxInstanceContextFactory(stateProvider);

            var manager = new MptxManager(stateProvider, instanceContextFactory, serviceProvider);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "foo", "42" }
            });
            var request = httpContext.Request;

            // Act
            var instance = await manager.CreateInstance(
                "TestFlow",
                typeof(FlowModel),
                request,
                capturesQueryParams: new[] { "foo" });

            // Assert
            Assert.Equal("42", instance.Items["foo"]);
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

            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => manager.CreateInstance("TestFlow", typeof(BadFlowModel), request, Array.Empty<string>()));

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
