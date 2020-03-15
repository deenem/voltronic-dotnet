namespace inverter.common.model
{
  public class DeviceFlags {

    public bool Buzzer{ get; private set;}
    public bool OverloadBypass{ get; private set;}
    public bool PowerSaving{ get; private set;}
    public bool DisplayTimeout{ get; private set;}
    public bool OverloadRestart{ get; private set;}
    public bool OverheatRestart{ get; private set;}
    public bool Backlight{ get; private set;}
    public bool AlarmOnPrimaryInterrupt{ get; private set;}
    public bool FaultCodeRecord{ get; private set;}

    public DeviceFlags(){

    }

    public DeviceFlags(string QFLAGSResult) {

    }
  }
}