using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

[assembly: OwinStartup(typeof(TodoApp.Startup))] // Reemplaza YourProjectNamespace
namespace TodoApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configuración de SignalR
            app.MapSignalR(); // Esto mapea el Hub de SignalR a /signalr por defecto

            // Configuración de Web API (si usas controladores HTTP junto a SignalR)
            var config = new HttpConfiguration();

            // Habilitar CORS para Web API (si es necesario)
            var cors = new EnableCorsAttribute("*", "*", "*"); // Permite todo (para desarrollo)
                                                               // Ajusta esto para producción
            config.EnableCors(cors);

            config.MapHttpAttributeRoutes(); // Habilita rutas por atributos si las usas

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseWebApi(config); // Usa la configuración de Web API
        }
    }
 }