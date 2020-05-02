using Customer.API.App_Start;
using Microsoft.Web.Http.Routing;
using Microsoft.Web.Http.Versioning;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Customer.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            AutofacConfig.Register();

            config.AddApiVersioning(cfg =>
            {
                // Versioning 1.1 
                //cfg.DefaultApiVersion = new Microsoft.Web.Http.ApiVersion(1, 1);
                // No need to pass in query string
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                // Version can be seen in headers 
                cfg.ReportApiVersions = true;
                // pasing vesion in header
                //cfg.ApiVersionReader = new HeaderApiVersionReader("X-Version");

                // URL vesion 
                cfg.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // Change case of JSON
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();


            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap =
                {
                    ["apiVersion"] = typeof(ApiVersionRouteConstraint)
                }
            };

            // Web API routes
            config.MapHttpAttributeRoutes(constraintResolver);

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }
    }
}
