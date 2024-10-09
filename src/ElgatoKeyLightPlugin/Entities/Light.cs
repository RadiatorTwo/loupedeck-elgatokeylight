namespace Loupedeck.ElgatoKeyLightPlugin.Entities
{
    using System;
    using System.IO;
    using System.Threading;
    using Newtonsoft.Json.Linq;
    using System.Net;
    using System.Linq;
    using System.Text;

    public sealed class Light : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public String DisplayName { get; }

        private Int32 Port { get; }

        private String Address { get; }

        private String Uri => $"http://{this.Address}:{this.Port}/elgato/lights";

        private CancellationToken CancellationToken => this._cancellationTokenSource.Token;

        public Boolean On { get; set; }
        public Int32 Brightness { get; set; } = 10;
        public Int32 Temperature { get; set; } = 200;
        public Int32 Hue { get; set; }
        public Int32 Saturation { get; set; }
        public Boolean Ready { get; set; }

        public Light(String displayName, Int32 port, String address)
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this.DisplayName = displayName;
            this.Port = port;
            this.Address = address;
        }

        public async Task InitDeviceAsync()
        {
            var light = this;

            int retries = 3;

            while (!light.Ready && retries > 0)
            {
                retries--;

                try
                {
                    var response = await ElgatoInstances.HttpClientInstance.GetAsync(this.Uri);

                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();

                    light.SetLightData(JObject.Parse(responseContent));

                    light.Ready = true;
                }
                catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
                {
                    // Timeout aufgetreten
                    if (retries == 0)
                    {
                        throw new Exception("Request was canceled or timed out after retries", ex);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Andere HTTP-Anfragefehler behandeln
                    if (retries == 0)
                    {
                        throw new Exception("An error occurred during the web request after retries", ex);
                    }
                }
            }
        }

        public void Toggle()
        {
            this.SetDeviceState(!this.On);
        }

        public void SetBrightness(Int32 brightness)
        {
            this.SetDeviceBrightness(brightness);
        }

        public void SetTemperature(Int32 temperature)
        {
            this.SetDeviceTemperature(temperature);
        }

        public void SetHue(Int32 hue)
        {
            this.SetDeviceHue(hue);
        }

        public void SetSaturation(Int32 saturation)
        {
            this.SetDeviceSaturation(saturation);
        }

        private void SetDeviceState(Boolean on)
        {
            if (this.On == on)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"on\":{Convert.ToInt32(on)}}}]}}";

            this.SendPutRequestAsync(jsonData).GetAwaiter().GetResult();

            this.On = on;
        }

        private void SetDeviceBrightness(Int32 brightness)
        {
            if (this.Brightness == brightness)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"brightness\":{brightness}}}]}}";

            this.SendPutRequestAsync(jsonData).GetAwaiter().GetResult();

            this.Brightness = brightness;
        }

        private void SetDeviceTemperature(Int32 temperature)
        {
            if (this.Temperature == temperature)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"temperature\":{temperature}}}]}}";

            this.SendPutRequestAsync(jsonData).GetAwaiter().GetResult();

            this.Temperature = temperature;
        }

        private void SetDeviceHue(Int32 hue)
        {
            if (this.Hue == hue)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"hue\":{hue}}}]}}";

            this.SendPutRequestAsync(jsonData).GetAwaiter().GetResult();

            this.Hue = hue;
        }

        private void SetDeviceSaturation(Int32 saturation)
        {
            if (this.Saturation == saturation)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"hue\":{saturation}}}]}}";

            this.SendPutRequestAsync(jsonData).GetAwaiter().GetResult();

            this.Saturation = saturation;
        }

        public async Task SendPutRequestAsync(string jsonData)
        {
            // Erstellen des Inhalts mit JSON-Daten
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                // Senden der PUT-Anfrage
                var response = await ElgatoInstances.HttpClientInstance.PutAsync(Uri, content);

                // Überprüfen des Antwortstatuscodes (optional)
                response.EnsureSuccessStatusCode();

                // Verarbeitung der Antwort (optional)
                var responseBody = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);
            }
            catch
            {
            }
        }

        private void SetLightData(JObject json)
        {
            var light = json["lights"].First();

            this.On = light?["on"] != null && (Int32)light["on"] == 1;

            this.Brightness = light?["brightness"] != null ? (Int32)light["brightness"] : 0;
            this.Temperature = light?["temperature"] != null ? (Int32)light["temperature"] : 0;
            this.Hue = light?["hue"] != null ? (Int32)light["hue"] : 0;
            this.Saturation = light?["saturation"] != null ? (Int32)light["saturation"] : 0;
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
