using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterUserSettings
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

  public class BatteryUserSettings
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

  public class SolarUserSettings
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
  public class EnviromentUserSettings
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

  public class UserSettings
  {

    public static string MESSAGE_TYPE = "User Settings";


    public DateTime EffectiveDate { get; set; }
    public InverterUserSettings inverter { get; private set; }
    public BatteryUserSettings battery { get; private set; }
    public SolarUserSettings solar { get; private set; }
    public EnviromentUserSettings enviroment { get; private set; }

    public UserSettings()
    {
      inverter = new InverterUserSettings();
      battery = new BatteryUserSettings();
      solar = new SolarUserSettings();
      enviroment = new EnviromentUserSettings();
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