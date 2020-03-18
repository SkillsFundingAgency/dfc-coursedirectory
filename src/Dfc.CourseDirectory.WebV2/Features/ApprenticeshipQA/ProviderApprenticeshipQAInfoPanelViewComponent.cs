using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA
{
    public class ProviderApprenticeshipQAInfoPanelViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public ProviderApprenticeshipQAInfoPanelViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid providerId)
        {
            var vm = await _mediator.Send(
                new ProviderApprenticeshipQAInfoPanel.Query()
                {
                    ProviderId = providerId
                });

            return View("~/Features/ApprenticeshipQA/ProviderApprenticeshipQAInfoPanel.cshtml", vm);
        }
    }
}
