namespace Loupedeck.ElgatoKeyLightPlugin
{
    using System;
    using System.Threading.Tasks;

    public class TemperatureAdjustment : PluginDynamicAdjustment
    {
        private const Int32 TEMPERATURE = 285;
        private Int32 _temperature = 285;

        private static readonly Object lockObject = new Object();
        private Int32 accumulatedDiff = 0;
        private Boolean isProcessing = false;
        private const Int32 throttleDelayMs = 250;

        public TemperatureAdjustment()
            : base(displayName: "Temperature", description: "Adjust Temperature", groupName: "Adjustments", hasReset: true)
        {
        }

        protected override Boolean OnLoad()
        {
            this._temperature = ElgatoInstances.Light.Temperature;
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

                this._temperature += currentDiff;

                if (this._temperature < 143)
                {
                    this._temperature = 143;
                }
                else if (this._temperature > 344)
                {
                    this._temperature = 344;
                }

                ElgatoInstances.Light.SetTemperature(this._temperature);
                this.AdjustmentValueChanged();

                await Task.Delay(throttleDelayMs);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            this._temperature = TEMPERATURE;
            ElgatoInstances.Light.SetTemperature(TEMPERATURE);
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            return this._temperature.ToString();
        }
    }
}
