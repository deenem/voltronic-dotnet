namespace inverter.common.model.messages
{
  public class DeviceFlags
  {
    public static string COMMAND = "QFLAG";
    public static string SUCCESS = "SUCCESS:QFLAG=>(";
    public static string NACK = "(NAK";


    public bool Buzzer { get; private set; } //A
    public bool OverloadBypass { get; private set; } //B
    public bool PowerSaving { get; private set; } //J
    public bool DisplayTimeout { get; private set; } //K
    public bool OverloadRestart { get; private set; } //U 
    public bool OverheatRestart { get; private set; } //V
    public bool Backlight { get; private set; } //X
    public bool AlarmOnPrimaryInterrupt { get; private set; } //Y
    public bool FaultCodeRecord { get; private set; } //Z

    public DeviceFlags()
    {

    }

    public DeviceFlags(string ResultString)
    {
      if (ResultString.StartsWith(SUCCESS) && !ResultString.EndsWith(NACK))
        ParseResult(ResultString);
    }

    public static bool CanProcess(string ResultString)
    {
      return (ResultString.StartsWith(SUCCESS) && !ResultString.EndsWith(NACK));
    }

    private void ParseResult(string ResultString)
    {


      //EkxyzDabjuv
      bool toggle = true;
      ResultString = ResultString.ToLower();

      foreach (var ch in ResultString)
      {

        if (ch == 'e') toggle = true;
        else if (ch == 'd') toggle = false;
        else if (ch == 'k') DisplayTimeout = toggle;
        else if (ch == 'x') Backlight = toggle;
        else if (ch == 'y') AlarmOnPrimaryInterrupt = toggle;
        else if (ch == 'z') FaultCodeRecord = toggle;
        else if (ch == 'a') Buzzer = toggle;
        else if (ch == 'b') OverloadBypass = toggle;
        else if (ch == 'j') PowerSaving = toggle;
        else if (ch == 'u') OverloadRestart = toggle;
        else if (ch == 'v') OverheatRestart = toggle;
      }

    }
  }
}