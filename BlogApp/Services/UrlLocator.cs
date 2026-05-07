using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BlogApp.Services
{
    public class UrlLocator
    {
        public UrlLocator(IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            ActionContext actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());

            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
            Images = new ImagesResource(urlHelper);
        }

        public ImagesResource Images { get; init; }



        // ******************************************************************************************* //

        public class ImagesResource
        {
            public ImagesResource(IUrlHelper urlHelper)
            {
                _urlHelper = urlHelper;
                Static = new StaticResource(urlHelper);
                Dynamic = new DynamicResource(urlHelper);
            }

            private IUrlHelper _urlHelper;
            public StaticResource Static { get; init; }
            public DynamicResource Dynamic { get; init; }

            // ******************************************************************************************* //

            public class StaticResource
            {
                private IUrlHelper _urlHelper;
                public StaticResource(IUrlHelper urlHelper) => _urlHelper = urlHelper;

                public string GetFaceEmojiIcon(bool isHappy)
                {
                    return isHappy ? _urlHelper.Content("images/static/icon-happy-face.png") : _urlHelper.Content("images/static/icon-unhappy-face.png");
                }

                public string GetUserProfileIcon(bool isMale)
                {
                    if (isMale) return _urlHelper.Content("images/static/icon-male.svg");
                    else return _urlHelper.Content("images/static/icon-female.svg");
                }

                public string GetBooleanIcon(bool isTrue)
                {
                    return isTrue ? _urlHelper.Content("images/static/icon-true.svg") : _urlHelper.Content("images/static/icon-false.svg");
                }
            }
            public class DynamicResource
            {
                private IUrlHelper _urlHelper;
                public DynamicResource(IUrlHelper urlHelper) => _urlHelper = urlHelper;
            }
        }
    }
}
