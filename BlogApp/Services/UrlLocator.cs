using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;

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
            }

            private IUrlHelper _urlHelper;
            public StaticResource Static { get; init; }

            // ******************************************************************************************* //

            public class StaticResource
            {
                private IUrlHelper _urlHelper;
                public StaticResource(IUrlHelper urlHelper) => _urlHelper = urlHelper;

                public string GetFaceEmojiIcon(bool isHappy)
                {
                    return isHappy ? _urlHelper.Content(Constants.StaticImagesURL.FaceHappyIcon) : _urlHelper.Content(Constants.StaticImagesURL.FaceUnhappyIcon);
                }

                public string GetUserProfileIcon(bool isMale)
                {
                    if (isMale) return _urlHelper.Content(Constants.StaticImagesURL.MaleUserProfileIcon);
                    else return _urlHelper.Content(Constants.StaticImagesURL.FemaleUserProfileIcon);
                }

                public string GetBooleanIcon(bool isTrue)
                {
                    return isTrue ? _urlHelper.Content(Constants.StaticImagesURL.BooleanTrueIcon) : _urlHelper.Content(Constants.StaticImagesURL.BooleanFalseIcon);
                }
            }            
        }
    }
}
