using System;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;

namespace inverter.common.model.messages
{
  public class DeviceStatus
  {

    public static string COMMAND = "QPIGS";
    public static string SUCCESS = "SUCCESS:QPIGS=>(";
    public static string NACK = "(NAK";

    public decimal GridVoltage { get; private set; }
    public decimal GridFrequency { get; private set; }
    public decimal ACOutputVoltage { get; private set; }
    public decimal ACOutputFrequency { get; private set; }
    public int ACOutputApparentPower { get; private set; }
    public int ACOutputActivePower { get; private set; }
    public int OutputLoadPercent { get; private set; }
    public int BusVoltage { get; private set; }
    public decimal BatteryVoltage { get; private set; }
    public int BatteryChargingCurrent { get; private set; }
    public int BatteryCapacity { get; private set; }
    public int HeatSinkTemperature { get; private set; }
    public decimal PVInputCurrentForBattery { get; private set; }
    public decimal PVInputVoltage1 { get; private set; }
    public decimal BatteryVoltageFromSCC { get; private set; }
    public int BatteryDischargeCurrent { get; private set; }
    public int DeviceStatusFlags { get; private set; }
    public int BatteryVoltageOffset { get; private set;}
    public int EEPROMVersion { get; private set;}
    public int PVChargingPower { get; private set;}
    public int DeviceStatus2 { get; private set;}

    public DeviceStatus()
    {
      GridVoltage = 0;
      GridFrequency = 0;
      ACOutputVoltage = 0;
      ACOutputFrequency = 0;
      ACOutputApparentPower = 0;
      ACOutputActivePower = 0;
      OutputLoadPercent = 0;
      BusVoltage = 0;
      BatteryVoltage = 0;
      BatteryChargingCurrent = 0;
      BatteryCapacity = 0;
      HeatSinkTemperature = 0;
      PVInputCurrentForBattery = 0;
      PVInputVoltage1 = 0;
      BatteryVoltageFromSCC = 0;
      BatteryDischargeCurrent = 0;
      DeviceStatusFlags = 0;
      BatteryVoltageOffset = 0;
      EEPROMVersion = 0;
      PVChargingPower = 0;
      DeviceStatus2 = 0;
    }

    public DeviceStatus(string Message)
    {
      if (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK))
        ParseResult(Message);
    }

    public static bool CanProcess(string Message)
    {
      return (Message.StartsWith(SUCCESS) && !Message.EndsWith(NACK));
    }

    private int StringFlagsToInt(string input){
      int result = 0;
      for (int i = 0; i < input.Length; ++i){
        if(input[i] != '0') 
          result += (int)Math.Pow(2, i);
      }
      return result;
    }
    private void ParseResult(string ResultString)
    {

      string result = ResultString.Substring(SUCCESS.Length);
      string[] values = result.Split(' ');

      for (int i = 0; i < values.Length; ++i)
      {
        switch (i)
        {
          case 0:
            GridVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 1:
            GridFrequency = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 2:
            ACOutputVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 3:
            ACOutputFrequency = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 4:
            ACOutputApparentPower = int.Parse(values[i]);
            break;
          case 5:
            ACOutputActivePower = int.Parse(values[i]);
            break;
          case 6:
            OutputLoadPercent = int.Parse(values[i]);
            break;
          case 7:
            BusVoltage = int.Parse(values[i]);
            break;
          case 8:
            BatteryVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 9:
            BatteryChargingCurrent = int.Parse(values[i]);
            break;
          case 10:
            BatteryCapacity = int.Parse(values[i]);
            break;
          case 11:
            HeatSinkTemperature = int.Parse(values[i]);
            break;
          case 12:
            PVInputCurrentForBattery = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 13:
            PVInputVoltage1 = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 14:
            BatteryVoltageFromSCC = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 15:
            BatteryDischargeCurrent = int.Parse(values[i]);
            break;
          case 16:
            DeviceStatusFlags = StringFlagsToInt(values[i]);
            break;
          case 17:
            BatteryVoltageOffset = int.Parse(values[i]);
            break;
          case 18:
            EEPROMVersion = int.Parse(values[i]);
            break;
          case 19:
            PVChargingPower = int.Parse(values[i]);
            break;
          case 20:
            DeviceStatus2 = StringFlagsToInt(values[i]);
            break;
          default:
            break;
        }
      }
    }


  }
}