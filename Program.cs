using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.Security.Cryptography;
using EPiServer.Events;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Framework;

namespace EPiServerRemoteEventsListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EPiServerRemoteEventsListener starting..");

            var helper = new InitializationHelper();
            var init = new InitializationEngine(new IInitializableModule[] { new ServiceContainerInitialization(), helper }, HostType.Service);
            init.Initialize();

            if (args.Length > 0 && args[0].Length > 0 && args[0].StartsWith("send"))
            {
                Send(args);
            }
            else
            {
                ServiceHost svcHost = new ServiceHost(typeof(RemoteEventListener));
                svcHost.Open();
                Console.WriteLine("Listening. Press any key to exit.....");
                Console.WriteLine();
                Console.ReadLine();
            }
        }

        static void Send(string[] args)
        {
            string siteId = "Test Site";
            Guid raiserId = Guid.NewGuid();
            byte[] siteSecret = null;

            // process the args, possible values are
            // -raiserId xxxxxxx
            // -siteSecret xxxxxx
            // -siteId xxxxx
            for (int index = 1; index < args.Length; index+=2)
            {
                if (string.Compare(args[index], "-raiserId", true) == 0)
                {
                    raiserId = new Guid(args[index+1]);
                }
                else if (string.Compare(args[index], "-siteSecret", true) == 0)
                {
                    siteSecret = Convert.FromBase64String(args[index + 1]);
                }
                else if (string.Compare(args[index], "-siteId", true) == 0)
                {
                    siteId = args[index + 1];
                }
            }

            
            RemoteEventProxy proxy = new RemoteEventProxy("RemoteEventServiceClientEndPoint");
            Console.WriteLine("Ready to send. Enter the data to send then press ENTER or press ENTER only to exit....");

            string param = "";
            int sequenceNumber = 1;
            Guid eventId = new Guid("C1A2EF00-4A9F-4263-B991-465A05D86F69");
            string input = "";

            while ((input = Console.ReadLine()).Length > 0)
            {
                // parse the input
                // possible values are 
                // -sequence xxxx
                // -eventId xxxx
                // -param xxxx
                // or just xxxx where xxxx is param

                string[] inputParts = input.Split(' ');

                for (int index = 0; index < inputParts.Length; index += 2)
                {
                    if (string.Compare(inputParts[index], "-sequence", true) == 0)
                    {
                        sequenceNumber = int.Parse(inputParts[index+1]);
                    }
                    else if (string.Compare(inputParts[index], "-eventId", true) == 0)
                    {
                        eventId = new Guid(inputParts[index + 1]);
                    }
                    else if (string.Compare(inputParts[index], "-param", true) == 0)
                    {
                        param = inputParts[index + 1];
                    }
                    else
                    {
                        param = inputParts[index];
                    }
                }

                try
                {
                    byte[] verificationData = null;

                    if (siteSecret != null)
                    {
                        verificationData = CreateEventVerificationData(siteSecret, raiserId, siteId, sequenceNumber, eventId, param);
                    }

                    var msg = new EventMessage() { RaiserId = raiserId, SiteId = siteId, SequenceNumber = ++sequenceNumber, VerificationData = verificationData, EventId = eventId, Parameter = input, ServerName = Environment.MachineName, Sent = DateTime.UtcNow, ApplicationName = "EPiServerRemoteEventsListener" };
                    proxy.Interface.RaiseEvent(msg);

                    Console.WriteLine("Successfully transmitted:");
                    Console.WriteLine(" Received        : " + DateTime.Now.ToString());
                    Console.WriteLine(" Sent            : " + msg.Sent.Value.ToLocalTime().ToString());
                    Console.WriteLine(" Sent from       : " + msg.ServerName + " (" + msg.ApplicationName + ")");
                    Console.WriteLine(" Raiser Id       : " + msg.RaiserId.ToString());
                    Console.WriteLine(" Site Id         : " + msg.SiteId?.ToString());
                    Console.WriteLine(" Sequence Number : " + msg.SequenceNumber.ToString());
                    Console.WriteLine(" Event Id        : " + msg.EventId.ToString());
                    Console.WriteLine(" Param           : " + msg.Parameter?.ToString());

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static byte[] CreateEventVerificationData(byte[] siteSecret, Guid raiserId, string siteId, int sequenceNumber, Guid eventId, object param)
        {
            // Write all of the paramteres (except for the site secret) to a buffer for hashing
            MemoryStream byteStream = new MemoryStream();

            byte[] buffer = raiserId.ToByteArray();
            byteStream.Write(buffer, 0, buffer.Length);

            buffer = new UnicodeEncoding().GetBytes(siteId);
            byteStream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(sequenceNumber);
            byteStream.Write(buffer, 0, buffer.Length);

            buffer = eventId.ToByteArray();
            byteStream.Write(buffer, 0, buffer.Length);

            // Use HMACSHA256 to create the verification data
            // The key to this will be the site secret
            HMACSHA256 hMac = new HMACSHA256(siteSecret);

            // NOTE: It appears that the overload of the ComputeHash method that takes
            // a stream appears to have a bug as the hash it calculates always appears
            // to be the same regardless of the stream content
            return hMac.ComputeHash(byteStream.ToArray());
        }
    }
}
