
using System;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Events;
using EPiServer.Framework.TypeScanner;
using System.Collections.Generic;
using System.Linq;

namespace EPiServerRemoteEventsListener
{
    internal class InitializationHelper : EPiServer.ServiceLocation.IConfigurableModule
    {
        public InitializationHelper()
        {
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<EventsServiceKnownTypesLookup>();
            context.Services.AddSingleton<ITypeScannerLookup, EmptyTypeScanner>();
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
            
        }
    }

    internal class EmptyTypeScanner : ITypeScannerLookup
    {
        public IEnumerable<Type> AllTypes { get; } = Enumerable.Empty<Type>();
    }
}