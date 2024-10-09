namespace Loupedeck.ElgatoKeyLightPlugin
{
    public class ToggleCommand : PluginDynamicCommand
    {
        public ToggleCommand()
            : base()
        {
        }

        protected override bool OnLoad()
        {
            ElgatoInstances.ElgatoService.KeyLightFound += this.ElgatoService_KeyLightFound;
            ElgatoInstances.ElgatoService.KeylightDisconnected += this.ElgatoService_KeylightDisconnected;
            return base.OnLoad();
        }

        protected override bool OnUnload()
        {
            ElgatoInstances.ElgatoService.KeyLightFound -= this.ElgatoService_KeyLightFound;
            ElgatoInstances.ElgatoService.KeylightDisconnected -= this.ElgatoService_KeylightDisconnected;

            return base.OnUnload();
        }

        private void ElgatoService_KeyLightFound(object sender, Entities.Light e)
        {
            this.AddParameter(e.DisplayName, "Toggle On/Off", e.DisplayName, "Key Lights");
        }

        private void ElgatoService_KeylightDisconnected(object sender, Entities.Light e)
        {
            this.RemoveParameter(e.DisplayName);
        }

        protected override void RunCommand(string actionParameter)
        {
            var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);

            if (light == null)
            {
                return;
            }

            light.Toggle();

            this.ActionImageChanged();
        }

        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize)
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