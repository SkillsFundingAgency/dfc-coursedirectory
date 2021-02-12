﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.FindACourseApi.Controllers
{
    public class TLevelsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IFeatureFlagProvider _featureFlagProvider;

        public TLevelsController(IMediator mediator, IFeatureFlagProvider featureFlagProvider)
        {
            _mediator = mediator;
            _featureFlagProvider = featureFlagProvider;
        }

        [HttpGet("~/tleveldefinitions")]
        [ProducesResponseType(typeof(Features.TLevelDefinitions.TLevelDefinitionViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTLevelDefinitions()
        {
            if (!_featureFlagProvider.HaveFeature(FeatureFlags.TLevels))
            {
                return NotFound();
            }

            return Ok(await _mediator.Send(new Features.TLevelDefinitions.Query()));
        }

        [HttpGet("~/tleveldetail")]
        [ProducesResponseType(typeof(Features.TLevelDetail.TLevelDetailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TLevelDetail([FromQuery] Features.TLevelDetail.Query request)
        {
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                _ => NotFound(),
                r => Ok(r));
        }
    }
}
