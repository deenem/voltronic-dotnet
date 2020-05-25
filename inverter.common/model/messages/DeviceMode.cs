//Power On Mode P 
//Standby Mode S 
//Line Mode L 
//Battery Mode B 
//Fault Mode F 
//Power saving Mode H 

namespace inverter.common.model.messages
{

    public enum DeviceModes
    {
        PowerOn,
        Standby,
        Line,
        Battery,
        Fault,
        PowerSaving
    }

  public class DeviceMode
  {
    public static string COMMAND = "QMOD";
    public static string SUCCESS = "SUCCESS:QMOD=>(";
    public static string NACK = "(NAK";

    public DeviceModes deviceMode {get; private set;}
    public string deviceModeString {get; private set;}

    public DeviceMode()
    {

    }

    public DeviceMode(string ResultString)
    {
      if (ResultString.StartsWith(SUCCESS) && !ResultString.EndsWith(NACK))
        ParseResult(ResultString);
    }

    public static bool CanProcess(string ResultString)
    {
      return (ResultString.StartsWith(SUCCESS) && !ResultString.EndsWith(NACK));
    }

    private void ParseResult(string ResultString){

      string result = ResultString.Substring(SUCCESS.Length, 1);

        if (result == "P"){
            deviceMode  = DeviceModes.PowerOn;
            deviceModeString = "Power On";
        } else if (result == "S"){
            deviceMode  = DeviceModes.Standby;
            deviceModeString = "Standby";
        } else if (result == "L"){
            deviceMode  = DeviceModes.Line;
            deviceModeString = "Line in";
        } else if (result == "B"){ 
            deviceMode  = DeviceModes.Battery;
            deviceModeString = "Battery";
        } else if (result == "F"){
            deviceMode  = DeviceModes.Fault;
            deviceModeString = "Fault";
        } else if (result == "H"){
            deviceMode  = DeviceModes.PowerSaving;
            deviceModeString = "Power Saving";
        }

    }  
  }
}