using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class AppendMptxInstanceTagHelperComponent : TagHelperComponent
    {
        private const string AttributeName = "append-mptx-id";

        private readonly MptxInstanceContextProvider _mptxInstanceContextProvider;

        public AppendMptxInstanceTagHelperComponent(MptxInstanceContextProvider mptxInstanceContextProvider)
        {
            _mptxInstanceContextProvider = mptxInstanceContextProvider;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.ContainsName(AttributeName))
            {
                return;
            }

            var mptxInstanceContext = _mptxInstanceContextProvider.GetContext();
            if (mptxInstanceContext == null)
            {
                throw new InvalidOperationException("No active MPTX instance.");
            }

            var mptxInstanceId = mptxInstanceContext.InstanceId;

            if (output.Attributes.ContainsName("href"))
            {
                HandleHref();
            }
            else if (output.Attributes.ContainsName("action"))
            {
                HandleAction();
            }
            else
            {
                throw new NotSupportedException($"Cannot determine how to append provider context.");
            }

            output.Attributes.RemoveAll(AttributeName);

            void HandleAction()
            {
                var method = output.Attributes["action"]?.Value.ToString() ?? "post";

                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    var input = new TagBuilder("input");
                    input.Attributes.Add("type", "hidden");
                    input.Attributes.Add("name", Constants.InstanceIdQueryParameter);
                    input.Attributes.Add("value", mptxInstanceId);
                    output.PreElement.AppendHtml(input);
                }
                else
                {
                    var currentAction = output.Attributes["action"].Value.ToString();

                    var newHref = QueryHelpers.AddQueryString(
                        currentAction,
                        Constants.InstanceIdQueryParameter,
                        mptxInstanceId);

                    output.Attributes.SetAttribute("action", newHref);
                }
            }

            void HandleHref()
            {
                var currentHref = output.Attributes["href"].Value.ToString();

                var newHref = QueryHelpers.AddQueryString(
                    currentHref,
                    Constants.InstanceIdQueryParameter,
                    mptxInstanceId);

                output.Attributes.SetAttribute("href", newHref);
            }
        }
    }
}
