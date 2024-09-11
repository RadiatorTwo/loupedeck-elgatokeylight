namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;
    using System.Threading.Tasks;

    public class BrightnessAdjustment : PluginDynamicAdjustment
    {
        private const Int32 BRIGHTNESS = 15;
        private Int32 _brightness = 15;

        private static readonly Object lockObject = new Object();
        private Int32 accumulatedDiff = 0;
        private Boolean isProcessing = false;
        private const Int32 throttleDelayMs = 250;

        public BrightnessAdjustment()
            : base(displayName: "Brightness", description: "Adjust Brightness", groupName: "Adjustments", hasReset: true)
        {
        }

        protected override Boolean OnLoad()
        {
            this._brightness = ElgatoInstances.Light.Brightness;
            return base.OnLoad();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            lock (lockObject)
            {
                this.accumulatedDiff += diff;

                if (!this.isProcessing)
                {
                    this.isProcessing = true;
                    Task.Run(() => this.ProcessAdjustment());
                }
            }
        }

        private async Task ProcessAdjustment()
        {
            while (true)
            {
                Int32 currentDiff;

                lock (lockObject)
                {
                    currentDiff = this.accumulatedDiff;
                    this.accumulatedDiff = 0;

                    if (currentDiff == 0)
                    {
                        this.isProcessing = false;
                        return;
                    }
                }

                this._brightness += currentDiff;

                if (this._brightness < 0)
                {
                    this._brightness = 0;
                }
                else if (this._brightness > 100)
                {
                    this._brightness = 100;
                }

                ElgatoInstances.Light.SetBrightness(this._brightness);
                this.AdjustmentValueChanged();

                await Task.Delay(throttleDelayMs);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            this._brightness = BRIGHTNESS;
            ElgatoInstances.Light.SetBrightness(BRIGHTNESS);
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            return this._brightness.ToString();
        }
    }
}
