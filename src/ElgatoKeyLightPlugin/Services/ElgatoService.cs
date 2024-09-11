namespace Loupedeck.ElgatoKeyLightPlugin.Services
{
    using System;
    using System.Threading;

    using Loupedeck.ElgatoKeyLightPlugin.Entities;

    public class ElgatoService : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private CancellationToken CancellationToken => this._cancellationTokenSource.Token;

        public ElgatoService() => this._cancellationTokenSource = new CancellationTokenSource();

        public void ProbeForElgatoDevices()
        {
            if (ElgatoInstances.Light != null)
            {
                return;
            }

            var lightInstance = new Light("Elgato Key Light Mini", 9123, "192.168.1.8");
            ElgatoInstances.Light = lightInstance;
            ElgatoInstances.Light.InitDevice();
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

            this._cancellationTokenSource.Cancel();
        }
    }
}
