namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;
    using System.Linq;

    using Loupedeck.ElgatoKeyLightPlugin.Entities;

    public class ToggleCommand : PluginDynamicCommand
    {
        public ToggleCommand()
            : base()
        {
        }

        protected override Boolean OnLoad()
        {
            ElgatoInstances.ElgatoService.KeyLightFound += this.ElgatoService_KeyLightFound;
            ElgatoInstances.ElgatoService.KeylightDisconnected += this.ElgatoService_KeylightDisconnected;
            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            ElgatoInstances.ElgatoService.KeyLightFound -= this.ElgatoService_KeyLightFound;
            ElgatoInstances.ElgatoService.KeylightDisconnected -= this.ElgatoService_KeylightDisconnected;

            return base.OnUnload();
        }

        private void ElgatoService_KeyLightFound(Object sender, Entities.Light e)
        {
            this.AddParameter(e.DisplayName, "Toggle On/Off", e.DisplayName, "Key Lights");
        }

        private void ElgatoService_KeylightDisconnected(Object sender, Entities.Light e)
        {
            this.RemoveParameter(e.DisplayName);
        }

        protected override void RunCommand(String actionParameter)
        {
            var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);
            
            if (light == null)
            {
                return;
            }

            light.Toggle();

            this.ActionImageChanged(); 
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);
            
            if (light == null)
            {
                return "Off";
            }

            return light.On ? "On" : "Off";
        }
    }
}
