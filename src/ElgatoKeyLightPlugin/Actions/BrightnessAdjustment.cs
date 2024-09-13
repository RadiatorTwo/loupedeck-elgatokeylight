namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Loupedeck.ElgatoKeyLightPlugin.Entities;

    public class BrightnessAdjustment : PluginDynamicAdjustment
    {
        private const Int32 BRIGHTNESS = 15;
        private Int32 _brightness = 15;

        private static readonly Object lockObject = new Object();
        private Int32 accumulatedDiff = 0;
        private Boolean isProcessing = false;
        private const Int32 throttleDelayMs = 250;

        public BrightnessAdjustment()
            : base(true)
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
            this.AddParameter(e.DisplayName, "Brightness", e.DisplayName, "Key Lights");
        }

        private void ElgatoService_KeylightDisconnected(Object sender, Entities.Light e)
        {
            this.RemoveParameter(e.DisplayName);
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            lock (lockObject)
            {
                this.accumulatedDiff += diff;

                if (!this.isProcessing)
                {
                    this.isProcessing = true;
                    var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);
                    Task.Run(() => this.ProcessAdjustment(light));
                }
            }
        }

        private async Task ProcessAdjustment(Light light)
        {
            if (light == null)
            {
                return;
            }

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

                light.SetBrightness(this._brightness);
                this.AdjustmentValueChanged();

                await Task.Delay(throttleDelayMs);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);

            if (light == null)
            {
                return;
            }

            this._brightness = BRIGHTNESS;

            light.SetBrightness(BRIGHTNESS);
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            return this._brightness.ToString();
        }
    }
}
