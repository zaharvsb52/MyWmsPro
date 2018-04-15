using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using wmsMLC.APS.wmsWebAPI.Attributes;
using wmsMLC.Business.Objects;

namespace wmsMLC.APS.wmsWebAPI
{
    public class CustomHttpControllerSelector : IHttpControllerSelector
    {
        private const string NamespaceKey = "namespace";
        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _controllers;
        private readonly HashSet<string> _duplicates;

        public CustomHttpControllerSelector(HttpConfiguration config)
        {
            _configuration = config;
            _duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _controllers = new Lazy<Dictionary<string, HttpControllerDescriptor>>(InitializeControllerDictionary);
        }

        private void AddController(Type controllerType, string controllerName, Dictionary<string, HttpControllerDescriptor> addToDic)
        {
            var segments = controllerType.Namespace.Split(Type.Delimiter);
            var key = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", segments[segments.Length - 1], controllerName);

            if (addToDic.Keys.Contains(key))
            {
                _duplicates.Add(key);
            }
            else
            {
                addToDic[key] = new HttpControllerDescriptor(_configuration, controllerType.Name, controllerType);
            }
        }

        private Dictionary<string, HttpControllerDescriptor> InitializeControllerDictionary()
        {
            var dictionary = new Dictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);

            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);

            foreach (Type t in controllerTypes)
            {
                AddController(t, t.Name.Remove(t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length), dictionary);
            }

            var genericEntityControllerTypes = GetType().Assembly.GetTypes().Where(t => t.IsDefined(typeof (EntityControllerAttribute))).ToArray();
            var entityTypes = typeof(WMSBusinessObject).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(WMSBusinessObject).IsAssignableFrom(t)).ToArray();

            foreach (var entityType in entityTypes)
            {
                foreach (var genControllerType in genericEntityControllerTypes)
                {
                    var t = genControllerType.MakeGenericType(entityType);
                    AddController(t, entityType.Name, dictionary);
                }                
            }

            foreach (string s in _duplicates)
            {
                dictionary.Remove(s);
            }

            return dictionary;
        }


        // Get a value from the route data, if present.
        private static T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result = null;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Get the namespace and controller variables from the route data.
            string namespaceName = GetRouteVariable<string>(routeData, NamespaceKey);
            if (namespaceName == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            string controllerName = GetRouteVariable<string>(routeData, ControllerKey);
            if (controllerName == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Find a matching controller.
            string key = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", namespaceName, controllerName);

            HttpControllerDescriptor controllerDescriptor;
            if (_controllers.Value.TryGetValue(key, out controllerDescriptor))
            {
                return controllerDescriptor;
            }
            else if (_duplicates.Contains(key))
            {
                throw new HttpResponseException(
                    request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        "Multiple controllers were found that match this request."));
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllers.Value;
        }
    }
}