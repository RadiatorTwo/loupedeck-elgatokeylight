using Loupedeck.ElgatoKeyLightPlugin.Services;

namespace Loupedeck.ElgatoKeyLightPlugin
{
    public static class ElgatoInstances
    {
        public static ElgatoService ElgatoService;

        public static readonly HttpClient HttpClientInstance = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(2000)
        };
    }
}