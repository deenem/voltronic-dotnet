using System;
using Newtonsoft.Json;
using System.Text;
using System.Globalization;

namespace inverter.common.model
{

  public enum BatteryType
  {
    AGM = 0,
    Flooded = 1,
    User = 2,
    Pylon = 3
  } 

  public enum InputVoltageRange 
  {
    Appliance = 0,
    UPS = 1
  }

  public enum OutputSourcePriority
  {
    UtilityFirst = 0,
    SolarFirst = 1,
    SBUFirst = 2
  }

  public enum MachineType 
  {
    GridTie = 0,
    OffGrid = 1,
    Hybrid = 10
  }


  public class DeviceRating {

    public static string SUCCESS = "SUCCESS:QPIRI=>(";
    public static string COMMAND = "QPIRI";


    public decimal GridRatingVoltage {get; private set;}
    public decimal GridRatingCurrent {get; private set;}
    public decimal ACOutputRatingVoltage {get; private set;}
    public decimal ACOutputRatingFrequency {get; private set;}
    public decimal ACOutputRatingCurrent {get; private set;}
    public int ACOutputRatingActivePower {get; private set;}
    public int ACOutputRatingApparentPower {get; private set;}
    public decimal BatteryRatingVoltage {get; private set;}
    public decimal BatteryRechargeVoltage {get; private set;}
    public decimal BatteryUnderVoltage {get; private set;}
    public decimal BatteryBulkVoltage {get; private set;}
    public decimal BatteryFloatVoltage {get; private set;}
    public BatteryType BatteryType {get; private set;}
    public int CurrentMaxACChargeCurrent {get; private set;}
    public int CurrentMaxChargeCurrent {get; private set;}
    public InputVoltageRange InputVoltageRange {get; private set;}
    public OutputSourcePriority OutputSourcePriority {get; private set;}
    public int ChargerSourcePriority {get; private set;}
    public int ParallelMaxNum {get; private set;}
    public MachineType MachineType {get; private set;}
    public int Topology {get; private set;}
    public int OutputMode {get; private set;}
    public decimal BatteryRedischargeVoltage {get; private set;}
    public int PVOKConditionForParallel {get; private set;}
    public string PVPowerBalance {get; private set;}



    public DeviceRating(){

      GridRatingVoltage = 0;
      GridRatingCurrent = 0;
      ACOutputRatingVoltage = 0;
      ACOutputRatingFrequency = 0;
      ACOutputRatingCurrent = 0;
      ACOutputRatingActivePower = 0;
      ACOutputRatingApparentPower = 0;
      BatteryRatingVoltage = 0;
      BatteryRechargeVoltage = 0;
      BatteryUnderVoltage = 0;
      BatteryBulkVoltage = 0;
      BatteryFloatVoltage = 0;
      BatteryType = BatteryType.AGM;
      CurrentMaxACChargeCurrent = 0;
      CurrentMaxChargeCurrent = 0;
      InputVoltageRange = InputVoltageRange.Appliance;
      OutputSourcePriority = OutputSourcePriority.UtilityFirst;
      ChargerSourcePriority = 0;
      ParallelMaxNum = 0;
      MachineType = MachineType.GridTie;
      Topology = 0;
      OutputMode = 0;
      BatteryRedischargeVoltage = 0;
      PVOKConditionForParallel = 0;
      PVPowerBalance = "";
    }

    public DeviceRating(string ResultString) {
      if (ResultString.StartsWith(SUCCESS)) 
        ParseResult(ResultString);
    }

    private void ParseResult(string ResultString) {
      string result = ResultString.Substring(SUCCESS.Length);
      string[] values = result.Split(' ');

      for (int i = 0; i < values.Length; ++ i)
      {
        switch (i)
        {
          case 0:
            GridRatingVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 1:
            GridRatingCurrent = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 2:
            ACOutputRatingVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 3:
            ACOutputRatingFrequency = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 4:
            ACOutputRatingCurrent = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 5:
            ACOutputRatingActivePower = int.Parse(values[i]);
            break;
          case 6:
            ACOutputRatingApparentPower = int.Parse(values[i]);
            break;
          case 7:
            BatteryRatingVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 8:
            BatteryRechargeVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 9:
            BatteryUnderVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 10:
            BatteryBulkVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 11:
            BatteryFloatVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 12:
            BatteryType = (BatteryType)int.Parse(values[i]);
            break;
          case 13:
            CurrentMaxACChargeCurrent = int.Parse(values[i]);
            break;
          case 14:
            CurrentMaxChargeCurrent = int.Parse(values[i]);
            break;
          case 15:
            InputVoltageRange = (InputVoltageRange)int.Parse(values[i]);
            break;
          case 16:
            OutputSourcePriority = (OutputSourcePriority)int.Parse(values[i]);
            break;
          case 17:
            ChargerSourcePriority = int.Parse(values[i]);
            break;
          case 18:
            ParallelMaxNum = int.Parse(values[i]);
            break;
          case 19:
            MachineType = (MachineType)int.Parse(values[i]);
            break;
          case 20:
            Topology = int.Parse(values[i]);
            break;
          case 21:
            OutputMode = int.Parse(values[i]);
            break;
          case 22:
            BatteryRedischargeVoltage = Convert.ToDecimal(double.Parse(values[i], CultureInfo.InvariantCulture));
            break;
          case 23:
            PVOKConditionForParallel = int.Parse(values[i]);
            break;
          case 24:
            PVPowerBalance = values[i];
            break;
        }
      }
    }

    public byte[] toJSON () 
    {
      string json = JsonConvert.SerializeObject(this, Formatting.Indented);
      var bytes = Encoding.UTF8.GetBytes(json);
      return bytes;
    }
  }
}