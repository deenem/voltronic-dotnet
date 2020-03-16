using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterOperatingProps
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

  public class BatteryOperatingProps
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

  public class SolarOperatingProps
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
  public class EnviromentOperatingProps
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