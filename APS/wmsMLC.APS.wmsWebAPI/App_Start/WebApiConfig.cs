using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using wmsMLC.APS.wmsWebAPI.Attributes;

namespace wmsMLC.APS.wmsWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            config.Formatters.Remove(config.Formatters.JsonFormatter);
            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            config.Filters.Add(new AuthorizeAttribute());
            config.Filters.Add(new LoggingExceptionFilterAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            // пути для запуска методов БП
            config.Routes.MapHttpRoute("BpApi", "{namespace}/bp/{action}"
                , defaults: new { controller = "bp" });

            config.Routes.MapHttpRoute(
                name: "EntityApi",
                routeTemplate: "{namespace}/{controller}/{id}",
                defaults: new { controller = "Entity", id = RouteParameter.Optional }
            );

            // добавляем своего ActionSelector-а для возможности отладки (без него оч. трудно искать почему не работает route)
            config.Services.Replace(typeof(IHttpActionSelector), new CustomApiControllerActionSelector());
            config.Services.Replace(typeof(IHttpControllerSelector), new CustomHttpControllerSelector(config));
        }
    }
}
