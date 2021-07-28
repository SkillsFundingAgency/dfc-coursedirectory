using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses
{
    [Authorize]
    public class ApprenticeshipsDataManagementHub : Hub
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ApprenticeshipsDataManagementHub(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public ChannelReader<UploadStatus> StatusUpdates(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<UploadStatus>();

            var obs = _fileUploadProcessor.GetApprenticeshipUploadStatusUpdatesForProvider(_providerContextProvider.GetProviderId())
                    .TakeWhile(v => v == UploadStatus.Created || v == UploadStatus.Processing);

            var subscription = obs.Subscribe(
                v => channel.Writer.WriteAsync(v),
                onCompleted: () => channel.Writer.Complete());

            cancellationToken.Register(() => subscription.Dispose());

            return channel.Reader;
        }
    }
}
