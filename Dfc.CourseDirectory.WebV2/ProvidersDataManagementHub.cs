using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dfc.CourseDirectory.WebV2
{
    [Authorize]
    public class ProvidersDataManagementHub : Hub
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public ProvidersDataManagementHub(
            IFileUploadProcessor fileUploadProcessor)
        {
            _fileUploadProcessor = fileUploadProcessor;
        }

        public ChannelReader<UploadStatus> StatusUpdates(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<UploadStatus>();
            var providerUploadId = new Guid(Context.GetHttpContext().Request.Query["providerUploadId"].ToString());

            var obs = _fileUploadProcessor.GeLatesttProviderUploadStatus(providerUploadId)
                    .TakeWhile(v => v == UploadStatus.Created || v == UploadStatus.Processing || v == UploadStatus.ProcessedSuccessfully);

            var subscription = obs.Subscribe(
                v => channel.Writer.WriteAsync(v),
                onCompleted: () => channel.Writer.Complete());

            cancellationToken.Register(() => subscription.Dispose());

            return channel.Reader;
        }
       
    }
}
