using System.Net.NetworkInformation;

using Loupedeck.ElgatoKeyLightPlugin.Entities;

using Zeroconf;

namespace Loupedeck.ElgatoKeyLightPlugin.Services
{
    public class ElgatoService : IDisposable
    {
        public event EventHandler<Light> KeyLightFound = (sender, light) => { };

        public event EventHandler<Light> KeylightDisconnected = (sender, light) => { };

        private readonly Dictionary<String, Light> Lights;

        public ElgatoService() => this.Lights = new Dictionary<String, Light>();

        public void ProbeForElgatoDevices()
        {
            var listener = ZeroconfResolver.CreateListener("_elg._tcp.local.", 4000, 2, TimeSpan.FromSeconds(1), 2, 2000);
            listener.ServiceFound += this.Listener_ServiceFound;
            listener.ServiceLost += this.Listener_ServiceLost;

            Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => listener.Dispose());
        }

        private void Listener_ServiceFound(Object sender, IZeroconfHost e)
        {
            var lightInstance = new Light(e.DisplayName, e.Services.Values.First<IService>().Port, e.IPAddress);
            lightInstance.InitDeviceAsync().GetAwaiter().GetResult();

            this.Lights.Add(e.DisplayName, lightInstance);
            this.KeyLightFound(sender, lightInstance);
        }

        private void Listener_ServiceLost(Object sender, IZeroconfHost e)
        {
            var light = this.GetKeyLight(e.DisplayName);
            this.Lights.Remove(e.DisplayName);
            this.KeylightDisconnected(sender, light);
        }

        public Light GetKeyLight(String name)
        {
            if (String.IsNullOrWhiteSpace(name) || !this.Lights.ContainsKey(name))
            {
                return null;
            }
            else
            {
                return this.Lights[name];
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(Boolean disposing)
        {
            if (!disposing)
            {
                return;
            }
        }

        public NetworkInterface[] GetPhysicalNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                             ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                             ni.OperationalStatus == OperationalStatus.Up)
                .ToArray();
        }
    }
}