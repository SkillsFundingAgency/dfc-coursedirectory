using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses
{
    [Authorize]
    public class CoursesDataManagementHub : Hub
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ILogger<CoursesDataManagementHub> _logger;

        public CoursesDataManagementHub(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            IFileUploadProcessor fileUploadProcessor,
            ILogger<CoursesDataManagementHub> logger)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _fileUploadProcessor = fileUploadProcessor;
            _logger = logger;
        }

        public ChannelReader<UploadStatus> StatusUpdates(CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var channel = Channel.CreateUnbounded<UploadStatus>();

            _ = Task.Run(async () =>
            {
                try
                {
                    Guid latestVenueUploadId;

                    using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
                    {
                        var latestVenueUpload = await dispatcher.ExecuteQuery(
                            new GetLatestVenueUploadForProviderWithStatus()
                            {
                                ProviderId = providerId,
                                Statuses = new[]
                                {
                                    UploadStatus.Created,
                                    UploadStatus.InProgress,
                                    UploadStatus.Processed
                                }
                            });

                        if (latestVenueUpload == null)
                        {
                            channel.Writer.Complete();
                            return;
                        }

                        latestVenueUploadId = latestVenueUpload.VenueUploadId;
                    }

                    var obs = _fileUploadProcessor.GetVenueUploadStatusUpdates(latestVenueUploadId)
                        .TakeWhile(v => v == UploadStatus.Created || v == UploadStatus.InProgress);

                    var subscription = obs.Subscribe(
                        v => channel.Writer.WriteAsync(v),
                        onCompleted: () => channel.Writer.Complete());

                    cancellationToken.Register(() => subscription.Dispose());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed querying venue upload status updates.");

                    channel.Writer.Complete();
                }
            });

            return channel.Reader;
        }
    }
}
