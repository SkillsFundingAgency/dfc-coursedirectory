using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Security;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using System.ComponentModel.DataAnnotations;

namespace Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Home
{
    public class Query : IRequest<ViewModel>
    {
    }

     public class ViewModel
    {
        [Required(ErrorMessage = "Select the provider report type")]
        public ProviderUploadType? ProviderUploadType { get; set; }
    }

    public enum ProviderUploadType
    {
        Active,
        Inactive
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ViewModel());
        }
    }
}
