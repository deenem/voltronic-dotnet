using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterFixedProps
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

  public class BatteryFixedProps
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