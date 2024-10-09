using Loupedeck.ElgatoKeyLightPlugin.Entities;

namespace Loupedeck.ElgatoKeyLightPlugin
{
    public class TemperatureAdjustment : PluginDynamicAdjustment
    {
        private const Int32 TEMPERATURE = 285;
        private Int32 _temperature = 285;

        private static readonly Object lockObject = new Object();
        private Int32 accumulatedDiff = 0;
        private Boolean isProcessing = false;
        private const Int32 throttleDelayMs = 10;

        public TemperatureAdjustment()
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
            this.AddParameter(e.DisplayName, "Temperature", e.DisplayName, "Key Lights");
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
                    if (light == null)
                    {
                        return;
                    }
                    Task.Run(() => this.ProcessAdjustment(light));
                }
            }
        }

        private async Task ProcessAdjustment(Light light)
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

                this._temperature += currentDiff;

                if (this._temperature < 143)
                {
                    this._temperature = 143;
                }
                else if (this._temperature > 344)
                {
                    this._temperature = 344;
                }

                light.SetTemperature(this._temperature);
                this.AdjustmentValueChanged();

                await Task.Delay(throttleDelayMs);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            this._temperature = TEMPERATURE;
            var light = ElgatoInstances.ElgatoService.GetKeyLight(actionParameter);

            if (light == null)
            {
                return;
            }

            light.SetTemperature(TEMPERATURE);
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            return this._temperature.ToString();
        }
    }
}