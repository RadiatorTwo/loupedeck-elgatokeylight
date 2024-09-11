namespace Loupedeck.ElgatoKeyLightPlugin.Entities
{
    using System;
    using System.IO;
    using System.Threading;
    using Newtonsoft.Json.Linq;
    using System.Net;
    using System.Linq;

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

        public void InitDevice()
        {
            var light = this;

            WebResponse response = null;
            Stream body = null;
            StreamReader reader = null;

            var retries = 3;

            while (!light.Ready && retries > 0)
            {
                retries--;
                var request = WebRequest.Create(this.Uri);
                request.Timeout = 2000;

                try
                {
                    response = request.GetResponse();
                    body = response.GetResponseStream();
                    reader = new StreamReader(body);

                    var endAsync = reader.ReadToEnd();

                    light.SetLightData(JObject.Parse(endAsync));

                    light.Ready = true;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout || ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    if (retries == 0)
                    {
                        throw new Exception("Request was canceled or timed out after retries", ex);
                    }
                }
                catch (WebException ex)
                {
                    // Handle other web request errors
                    if (retries == 0)
                    {
                        throw new Exception("An error occurred during the web request after retries", ex);
                    }
                }
                finally
                {
                    reader?.Dispose();
                    body?.Dispose();
                    response?.Dispose();
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

            this.SendRequest(jsonData);

            this.On = on;
        }

        private void SetDeviceBrightness(Int32 brightness)
        {
            if (this.Brightness == brightness)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"brightness\":{brightness}}}]}}";

            this.SendRequest(jsonData);

            this.Brightness = brightness;
        }

        private void SetDeviceTemperature(Int32 temperature)
        {
            if (this.Temperature == temperature)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"temperature\":{temperature}}}]}}";

            this.SendRequest(jsonData);

            this.Temperature = temperature;
        }

        private void SetDeviceHue(Int32 hue)
        {
            if (this.Hue == hue)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"hue\":{hue}}}]}}";

            this.SendRequest(jsonData);

            this.Hue = hue;
        }

        private void SetDeviceSaturation(Int32 saturation)
        {
            if (this.Saturation == saturation)
            {
                return;
            }

            var jsonData = $"{{\"lights\":[{{\"hue\":{saturation}}}]}}";

            this.SendRequest(jsonData);

            this.Saturation = saturation;
        }

        private void SendRequest(String jsonData)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.ContentLength = jsonData.Length;
            request.Timeout = 1000;
            request.ReadWriteTimeout = 1000;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
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
