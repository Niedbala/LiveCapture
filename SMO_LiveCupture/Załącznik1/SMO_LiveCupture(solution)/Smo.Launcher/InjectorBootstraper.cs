
using SimpleInjector;
using Smo.Common;
using Smo.Common.Infrastructure;
using SmoReader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smo.Common.Public.Repositories;
using Smo.Startup;

namespace Smo.Startup
{
    internal class InjectorBootstraper : IServiceProvider
    {
        private IAircraftDataProvider _smoAircraftRepository;
        private string _instrumentSettingsXmlPath;

        public InjectorBootstraper(IAircraftDataProvider smoAircraftRepository, string instrumentSettingsXmlPath)
        {           
            _smoAircraftRepository = smoAircraftRepository;
            _instrumentSettingsXmlPath = instrumentSettingsXmlPath;

            Container = Bootstrap();
        }

        public readonly Container Container;


        public object GetService(Type serviceType)
        {
            var service = ((IServiceProvider)Container).GetService(serviceType);
            return service;
        }

        internal Container Bootstrap()
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.ThreadScopedLifestyle();

            container.Register(() => _smoAircraftRepository, Lifestyle.Singleton);

            container.Register<IFilePersistence, FilePersistence>(Lifestyle.Transient);

            container.Register<ILoggingService,LoggingService>(Lifestyle.Singleton);

           
            container.Register<BatchConverter>();

            container.Register<IGlobalPaths>(() => new GlobalPaths()
            {
                InstrumentSettingsXml = _instrumentSettingsXmlPath
               
            });


            container.Verify();
            return container;
        }
    }
}
