using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterFixedProps
  {
      public decimal GridRatingVoltage { get; private set; }
      public decimal GridRatingCurrent { get; private set; }
      public decimal ACOutputRatingVoltage { get; private set; }
      public decimal ACOutputRatingFrequency { get; private set; }
      public decimal ACOutputRatingCurrent { get; private set; }
      public int ACOutputRatingActivePower { get; private set; }
      public int ACOutputRatingApparentPower { get; private set; }

    public void Update(DeviceRating deviceRating)
    {
      GridRatingVoltage = deviceRating.GridRatingVoltage;
      GridRatingCurrent = deviceRating.GridRatingCurrent;
      ACOutputRatingVoltage = deviceRating.ACOutputRatingVoltage;
      ACOutputRatingFrequency = deviceRating.ACOutputRatingFrequency;
      ACOutputRatingCurrent = deviceRating.ACOutputRatingCurrent;
      ACOutputRatingActivePower = deviceRating.ACOutputRatingActivePower;
      ACOutputRatingApparentPower = deviceRating.ACOutputRatingApparentPower;

    }
    public void Update(DeviceStatus deviceRating)
    {

    }

    public void Update(DeviceFlags deviceFlags)
    {

    }

  }

  public class BatteryFixedProps
  {
      public decimal BatteryRatingVoltage { get; private set; }

    public void Update(DeviceRating deviceRating)
    {
      BatteryRatingVoltage = deviceRating.BatteryRatingVoltage;

    }
    public void Update(DeviceStatus deviceRating)
    {

    }
    public void Update(DeviceFlags deviceFlags)
    {

    }
  }

  public class SolarFixedProps
  {
    public void Update(DeviceRating deviceRating)
    {
    }
    public void Update(DeviceStatus deviceRating)
    {

    }
    public void Update(DeviceFlags deviceFlags)
    {

    }
  }
  public class EnviromentFixedProps
  {
    public string SerialNumber { get; private set; }
    public int ParallelMaxNum { get; private set; }
    public MachineType MachineType { get; private set; }   

    public void Update(DeviceRating deviceRating)
    {
      ParallelMaxNum = deviceRating.ParallelMaxNum;
      MachineType = deviceRating.MachineType;
    }
    
    public void Update(DeviceStatus deviceRating)
    {

    }
    public void Update(DeviceFlags deviceFlags)
    {

    }
  }

  public class FixedProps
  {

    public static string MESSAGE_TYPE = "Fixed Properties";

    public DateTime EffectiveDate { get; set; }
    public InverterFixedProps inverter { get; private set; }
    public BatteryFixedProps battery { get; private set; }
    public SolarFixedProps solar { get; private set; }
    public EnviromentFixedProps enviroment { get; private set; }

    public FixedProps()
    {
      inverter = new InverterFixedProps();
      battery = new BatteryFixedProps();
      solar = new SolarFixedProps();
      enviroment = new EnviromentFixedProps();
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