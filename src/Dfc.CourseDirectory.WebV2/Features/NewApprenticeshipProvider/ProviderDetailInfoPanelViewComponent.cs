using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class ProviderDetailInfoPanelViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public ProviderDetailInfoPanelViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid providerId)
        {
            var vm = await _mediator.Send(
                new ProviderDetailInfoPanel.Query()
                {
                    ProviderId = providerId
                });

            return View("~/Features/NewApprenticeshipProvider/ProviderDetailInfoPanel.cshtml", vm);
        }
    }
}
