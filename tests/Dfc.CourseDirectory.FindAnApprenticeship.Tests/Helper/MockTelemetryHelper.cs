using System;
using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Dfc.CourseDirectory.FindAnApprenticeship.Tests.Helper
{
    public static class MockTelemetryHelper
    {
        public static TelemetryClient Initialize()
        {
            var mockTelemetryChannel = new MockTelemetryChannel();
            var mockTelemetryConfig = new TelemetryConfiguration
            {
                TelemetryChannel = mockTelemetryChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };

            var mockTelemetryClient = new TelemetryClient(mockTelemetryConfig);
            return mockTelemetryClient;
        }

        public class MockTelemetryChannel : ITelemetryChannel
        {
            public ConcurrentBag<ITelemetry> Telemetry = new ConcurrentBag<ITelemetry>();
            public bool IsFlushed { get; private set; }
            public bool? DeveloperMode { get; set; }
            public string EndpointAddress { get; set; }

            public void Send(ITelemetry item)
            {
                this.Telemetry.Add(item);
            }

            public void Flush()
            {
                this.IsFlushed = true;
            }

            public void Dispose()
            {

            }
        }
    }


}
