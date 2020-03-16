using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterOperatingProps
  {
    public decimal GridVoltage { get; private set; }
    public decimal GridFrequency { get; private set; }
    public decimal ACOutputVoltage { get; private set; }
    public decimal ACOutputFrequency { get; private set; }
    public int ACOutputApparentPower { get; private set; }
    public int ACOutputActivePower { get; private set; }
    public int OutputLoadPercent { get; private set; }

    public void Update(DeviceRating deviceRating)
    {
    }
    public void Update(DeviceStatus deviceStatus)
    {
      GridVoltage = deviceStatus.GridVoltage;
      GridFrequency = deviceStatus.GridFrequency;
      ACOutputVoltage = deviceStatus.ACOutputVoltage;
      ACOutputFrequency = deviceStatus.ACOutputFrequency;
      ACOutputApparentPower = deviceStatus.ACOutputApparentPower;
      ACOutputActivePower = deviceStatus.ACOutputActivePower;
      OutputLoadPercent = deviceStatus.OutputLoadPercent;
    }

    public void Update(DeviceFlags deviceFlags)
    {

    }
  }

  public class BatteryOperatingProps
  {
    public int BusVoltage { get; private set; }
    public decimal BatteryVoltage { get; private set; }
    public int BatteryChargingCurrent { get; private set; }
    public int BatteryCapacity { get; private set; }
    public int BatteryDischargeCurrent { get; private set; }

    public void Update(DeviceRating deviceRating)
    {

    }
    public void Update(DeviceStatus deviceStatus)
    {
      BusVoltage = deviceStatus.BusVoltage;
      BatteryVoltage = deviceStatus.BatteryVoltage;
      BatteryChargingCurrent = deviceStatus.BatteryChargingCurrent;
      BatteryCapacity = deviceStatus.BatteryCapacity;
      BatteryDischargeCurrent = deviceStatus.BatteryDischargeCurrent;
    }

    public void Update(DeviceFlags deviceFlags)
    {

    }

  }

  public class SolarOperatingProps
  {
      public decimal PVInputCurrentForBattery { get; private set; }
      public decimal PVInputVoltage1 { get; private set; }
      public decimal BatteryVoltageFromSCC { get; private set; }

    public void Update(DeviceRating deviceRating)
    {

    }
    public void Update(DeviceStatus deviceStatus)
    {
      PVInputCurrentForBattery = deviceStatus.PVInputCurrentForBattery;
      PVInputVoltage1 = deviceStatus.PVInputVoltage1;
      BatteryVoltageFromSCC = deviceStatus.BatteryVoltageFromSCC;

    }

    public void Update(DeviceFlags deviceFlags)
    {

    }

  }
  public class EnviromentOperatingProps
  {
    public int HeatSinkTemperature { get; private set; }


    public void Update(DeviceRating deviceRating)
    {

    }
    public void Update(DeviceStatus deviceStatus)
    {
      HeatSinkTemperature = deviceStatus.HeatSinkTemperature;
    }

    public void Update(DeviceFlags deviceFlags)
    {

    }


  }

  public class OperatingProps
  {
    public static string MESSAGE_TYPE = "Operating Properties";


    public DateTime EffectiveDate { get; set; }
    public InverterOperatingProps inverter { get; private set; }
    public BatteryOperatingProps battery { get; private set; }
    public SolarOperatingProps solar { get; private set; }
    public EnviromentOperatingProps enviroment { get; private set; }

    public OperatingProps()
    {
      inverter = new InverterOperatingProps();
      battery = new BatteryOperatingProps();
      solar = new SolarOperatingProps();
      enviroment = new EnviromentOperatingProps();
    }

    public void Update(DeviceRating deviceRating)
    {
      inverter.Update(deviceRating);
      battery.Update(deviceRating);
      solar.Update(deviceRating);
      enviroment.Update(deviceRating);
      EffectiveDate = DateTime.Now;
    }
    public void Update(DeviceStatus deviceStatus)
    {
      inverter.Update(deviceStatus);
      battery.Update(deviceStatus);
      solar.Update(deviceStatus);
      enviroment.Update(deviceStatus);
      EffectiveDate = DateTime.Now;
    }

    public void Update(DeviceFlags deviceFlags)
    {
      inverter.Update(deviceFlags);
      battery.Update(deviceFlags);
      solar.Update(deviceFlags);
      enviroment.Update(deviceFlags);
      EffectiveDate = DateTime.Now;
    }

    public byte[] toJSON()
    {
      string json = JsonConvert.SerializeObject(this, Formatting.Indented);
      var bytes = Encoding.UTF8.GetBytes(json);
      return bytes;
    }
  }
}