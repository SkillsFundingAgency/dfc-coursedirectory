using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;

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

            if (vm.Value is NotFound)
            {
                throw new InvalidOperationException("Specified provider does not exist.");
            }

            return View("~/Features/ApprenticeshipQA/ProviderApprenticeshipQAInfoPanel.cshtml", vm.AsT1);
        }
    }
}
