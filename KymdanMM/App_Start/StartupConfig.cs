using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using KymdanMM.Data.Infrastructure;
using KymdanMM.Data.Repository;
using KymdanMM.Data.Service;

namespace KymdanMM.App_Start
{
    public class StartupConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerHttpRequest();
            builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerHttpRequest();
            builder.RegisterAssemblyTypes(typeof(IMaterialProposalRepository).Assembly)
                .Where(a => a.Name.EndsWith("Repository")).AsImplementedInterfaces().InstancePerHttpRequest();
            builder.RegisterAssemblyTypes(typeof(IMaterialProposalService).Assembly)
                .Where(a => a.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerHttpRequest();

            builder.RegisterFilterProvider();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}