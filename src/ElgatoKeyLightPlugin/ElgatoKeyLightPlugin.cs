namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;

    using Loupedeck.ElgatoKeyLightPlugin.Services;

    public class ElgatoKeyLightPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;

        public override Boolean HasNoApplication => true;

        public ElgatoKeyLightPlugin()
        {
            PluginLog.Init(this.Log);

            PluginResources.Init(this.Assembly);

            ElgatoInstances.ElgatoService = new ElgatoService();
            ElgatoInstances.ElgatoService.ProbeForElgatoDevices();
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
            ElgatoInstances.ElgatoService.Dispose();
        }
    }
}
