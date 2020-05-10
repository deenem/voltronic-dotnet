namespace inverter.common.model.messages
{
  public class DeviceFlags
  {
    public static string COMMAND = "QFLAG";
    public static string SUCCESS = "SUCCESS:QFLAG=>(";
    public static string NACK = "(NAK";


    public bool Buzzer { get; private set; }
    public bool OverloadBypass { get; private set; }
    public bool PowerSaving { get; private set; }
    public bool DisplayTimeout { get; private set; }
    public bool OverloadRestart { get; private set; }
    public bool OverheatRestart { get; private set; }
    public bool Backlight { get; private set; }
    public bool AlarmOnPrimaryInterrupt { get; private set; }
    public bool FaultCodeRecord { get; private set; }

    public DeviceFlags()
    {

    }

    public DeviceFlags(string QFLAGSResult)
    {
      if (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK))
        ParseResult(ResultString);
    }

    public static bool CanProcess(string Message)
    {
      return (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK));
    }

    private void ParseResult(string ResultString){

      bool enabledFlags = false;
      bool disabledFlags = false;

      for (int i = o; i < ResultString.len)
      // EkxyzDabjuv

    }
  }
}