using EPiServer.Events;
using EPiServer.Events.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPiServerRemoteEventsListener
{
    class RemoteEventListener : IEventReplication
    {
        private static Object _lockObject;

        Dictionary<Guid, string> _mappedGuidName = new Dictionary<Guid, string>
            {
                { new Guid("4e755664-8fd9-4906-88ca-476842076f98"), "CacheObjectStore-ObjectStoreCache" },
                { new Guid("1ee4c0b5-ca95-4bdb-b0d4-e5d9e91189aa"), "CacheEventNotifier-CacheEventNotifier" },
                { new Guid("c1b94788-2410-4da1-af51-4927abe5da94"), "PermanentLinkMapStore-Change" },
                { new Guid("414cda81-8720-41f1-bad2-7d6155f419dc"), "PermanentLinkMapStore-Remove" },
                { new Guid("ff174e9c-e3c4-4072-9e1f-7eaf59c5f54f"), "VirtualRoleReplication-Register" },
                { new Guid("546bf805-e87a-46d7-95d1-423ed87662bd"), "VirtualRoleReplication-UnRegister" },
                { new Guid("15fc0951-4510-49ae-9fa3-6cd4762d4101"), "VirtualRoleReplication-Clear" },
                { new Guid("f67ab721-faa5-4e37-b894-75aaffa7665d"), "VisitorGroupEvents-Saved" },
                { new Guid("719a93e5-727e-441f-8167-70cca8639803"), "VisitorGroupEvents-Deleted" },
                { new Guid("eaac6db8-c224-4558-8e07-4454a15f8d71"), "RuntimeCacheEvents-BlockedCache" },
                { new Guid("69793f9f-c106-4b71-a524-8ed7af051077"), "RuntimeCacheEvents-FlushStoredCache" },
                { new Guid("d464d910-68ef-402a-98c6-72a2c95dcdba"), "ContentLanguageSettingsHandler-ClearSettings" },
                { new Guid("0ccfcc21-2e73-4901-a559-a4484d7bd472"), "ContentLanguageSettingsHandler-ClearTreeMap" },
                { new Guid("c6fb4f08-069c-4ee6-8588-4d5cc806b654"), "BroadcastOperations-Workflow" },
                { new Guid("b6f0e39a-93ef-4dee-a728-a49dac5501fa"), "ChangeLogSystem-Start" },
                { new Guid("a416a5af-6469-48f7-ad9e-c358fe506916"), "ChangeLogSystem-Stop" },
                { new Guid("55a261f2-9b2a-47dc-87b7-29c4eab895eb"), "ChangeLogSystem-StateChange" },
                { new Guid("9484e34b-b419-4e59-8fd5-3277668a7fce"), "RemoteCacheSynchronization-RemoveFromCache" },
                { new Guid("51da5053-6af8-4a10-9bd4-8417e48f38bd"), "ServerStateService-State" },
                { new Guid("184468e9-9f0d-4460-aecd-3c08f652c73c"), "ScheduledJob-StopJob" },
                { new Guid("CCDF4F50-8216-4919-B8C1-21D9F1932BAF"), "MetaDataEventManager-MetaDataUpdated" },
                { new Guid("8B448BC6-F7E1-4833-BDC7-CA338B77ADFA"), "ProductEventManager-CommerceProductUpdated" },
                { new Guid("B10915E6-0C84-4a6a-8707-FF6F357A1099"), "BlogModule-BlogReplication" },
                { new Guid("F6F0147E-F60F-4801-8647-66270D10AFB9"), "ForumModule-ForumReplication" },
                { new Guid("F6742777-6F38-46a1-AA38-9985715089A2"), "ImageGalleryModule-ImageGalleryReplication" },
                { new Guid("F160271E-7972-447a-81D0-152A35FD77BD"), "OnlineStatusModule-OnlineStatusReplication" },
                { new Guid("BE4D5630-8307-4361-9366-71C0032E8A84"), "SiteDefinitionRepository-Saved" },
                { new Guid("B8635A4C-7D77-4C69-B1EB-674D4AB442FC"), "PermanentLinkMapStore-ClearCache" },
                { new Guid("96728921-417C-4061-B278-C5621BD4F995"), "UI push notification" }
            };

        static RemoteEventListener()
        {
            _lockObject = new Object();
        }

        #region IEventReplication Members

        public void RaiseEvent(EventMessage msg)
        {
            // And event has been received from the event broadcaster

            // This is because it is this class that is named in the config
            // <system.serviceModel>
            //   <services>
            //     <service name="EPiServerRemoteEventsListener.RemoteEventListener" .../>
            //   </service>
            // </system.serviceModel>

            // If you want to forward the event to another end point this is where you will do it

            lock (_lockObject)
            {
                Console.WriteLine(" Event Id        : " + TranslateGuid(msg.EventId));
                Console.WriteLine(" Param           : " + msg.Parameter?.ToString());
                Console.WriteLine(" Received        : " + DateTime.Now.ToString() + " at " + Environment.MachineName);
                Console.WriteLine(" Sent            : " + (msg.Sent.HasValue ? msg.Sent.Value.ToLocalTime().ToString() : "N/A") + " from " + (msg.ServerName ?? "N/A") + " (" + (msg.ApplicationName ?? "N/A") + ")");
                Console.WriteLine(" Debug           : seq=" + msg.SequenceNumber.ToString() + ", site=" + msg.SiteId + ", raiser=" + msg.RaiserId);
                if (msg.Sent.HasValue && (DateTime.UtcNow - msg.Sent.Value) > TimeSpan.FromSeconds(10))
                {
                    Console.WriteLine();
                    Console.WriteLine(" WARNING! There is more than 10 seconds delay from the message");
                    Console.WriteLine(" was sent to it was received. Possible reasons could be that system clocks");
                    Console.WriteLine(" are not in sync or the network or servers are overloaded. Could also");
                    Console.WriteLine(" be a TCP connection that drops SYN packages (fixed in CMS.Core 10.10)");
                }
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
            }
        }

        private string TranslateGuid(Guid guid)
        {
            string text;
            if(_mappedGuidName.TryGetValue(guid, out text))
            {
                return text;
            }
            return guid.ToString();
        }

        #endregion
    }
}
