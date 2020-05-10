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

    public DeviceFlags(string Message)
    {
      if (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK))
        ParseResult(Message);
    }

    public static bool CanProcess(string Message)
    {
      return (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK));
    }

    private void ParseResult(string ResultString)
    {

      //EkxyzDabjuv
      bool isEnabled = false;
      ResultString = ResultString.ToLower();

      for (int i = 0; i < ResultString.Length; ++i)
      {

        char ch = ResultString[i];
        if (ch == 'e') isEnabled = true;
        else if (ch == 'd') isEnabled = false;
        else if (ch == 'k') DisplayTimeout = isEnabled;
        else if (ch == 'x') Backlight = isEnabled;
        else if (ch == 'y') AlarmOnPrimaryInterrupt = isEnabled;
        else if (ch == 'z') FaultCodeRecord = isEnabled;
        else if (ch == 'a') Buzzer = isEnabled;
        else if (ch == 'b') OverloadBypass = isEnabled;
        else if (ch == 'j') PowerSaving = isEnabled;
        else if (ch == 'u') OverloadRestart = isEnabled;
        else if (ch == 'v') OverheatRestart = isEnabled;
      }
      // EkxyzDabjuv

    }
  }
}