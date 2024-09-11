namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;

    public class ElgatoKeyLightApplication : ClientApplication
    {
        public ElgatoKeyLightApplication()
        {
        }

        protected override String GetProcessName()
        {
            return "";
        }

        protected override String GetBundleName()
        {
            return "";
        }

        public override ClientApplicationStatus GetApplicationStatus()
        {
            return ClientApplicationStatus.Unknown;
        }
    }
}
