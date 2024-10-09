namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;

    // This class can be used to connect the Loupedeck plugin to an application.

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
