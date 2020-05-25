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

       foreach (var c in ResultString)
       {
         switch (c)
         {
            case 'D':
              toggle = false; break;
            case 'a':
              Buzzer = toggle; break;
            case 'b': 
              OverloadBypass = toggle; break;
            case 'j':
              PowerSaving = toggle; break;
            case 'k':
              DisplayTimeout = toggle; break;
            case 'u':
              OverloadRestart = toggle; break;
            case 'v':
              Backlight = toggle; break;
            case 'x':
              PowerSaving = toggle; break;
            case 'y':
              AlarmOnPrimaryInterrupt = toggle; break;
            case 'z':
              FaultCodeRecord = toggle; break;
            default:
              break;
         }
       }
    }
  }
}