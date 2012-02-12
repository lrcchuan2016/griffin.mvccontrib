﻿using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Griffin.MvcContrib.Areas.Griffin.Controllers;
using Griffin.MvcContrib.Localization;
using Griffin.MvcContrib.VirtualPathProvider;
using SqlServerLocalization.Modules;

namespace SqlServerLocalization
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private IContainer _container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RegisterContainer();
            HostingEnvironment.RegisterVirtualPathProvider(GriffinVirtualPathProvider.Current);

            var embeddedProvider = new EmbeddedViewFileProvider();
            embeddedProvider.Add(new NamespaceMapping(typeof (GriffinHomeController).Assembly, "Griffin.MvcContrib"));
            GriffinVirtualPathProvider.Current.Add(embeddedProvider);

            //ModelMetadataProviders.Current = null;
            ModelValidatorProviders.Providers.Clear();
        }

        private void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterModules(Assembly.GetExecutingAssembly());
            builder.RegisterType<EmbeddedViewFixer>().AsImplementedInterfaces().SingleInstance();

            // register medata providers
            builder.RegisterType<LocalizedModelMetadataProvider>().AsImplementedInterfaces().As<ModelMetadataProvider>()
                .InstancePerHttpRequest();
            builder.RegisterType<LocalizedModelValidatorProvider>().AsImplementedInterfaces().As<ModelValidatorProvider>
                ().InstancePerHttpRequest();


            _container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_container));
        }
    }

    public class ModelMetaDummy : ModelValidatorProvider
    {
        /// <summary>
        /// Gets a list of validators.
        /// </summary>
        /// <returns>
        /// A list of validators.
        /// </returns>
        /// <param name="metadata">The metadata.</param><param name="context">The context.</param>
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            return new List<ModelValidator>();
        }
    }
}