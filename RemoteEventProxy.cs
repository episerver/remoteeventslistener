using EPiServer.Events.ServiceModel;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace EPiServerRemoteEventsListener
{
    internal class RemoteEventProxy : ClientBase<IEventReplication>
    {
        internal RemoteEventProxy(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        internal IEventReplication Interface
        {
            get
            {
                return this.Channel;
            }
        }
    }
}
