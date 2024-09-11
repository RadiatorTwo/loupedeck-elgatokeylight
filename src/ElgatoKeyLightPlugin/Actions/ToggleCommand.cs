namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;

    using Loupedeck.ElgatoKeyLightPlugin.Entities;

    public class ToggleCommand : PluginDynamicCommand
    {
        public ToggleCommand()
            : base(displayName: "Toggle Light", description: "Turns Light On or Off", groupName: "Commands")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            ElgatoInstances.Light.Toggle();

            this.ActionImageChanged(); 
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (ElgatoInstances.Light == null)
            {
                return String.Empty;
            }

            return ElgatoInstances.Light.On ? "On" : "Off";
        }
    }
}
