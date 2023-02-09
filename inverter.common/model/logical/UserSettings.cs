using System;
using Newtonsoft.Json;
using System.Text;

namespace inverter.common.model.messages
{
  public class InverterUserSettings
  {

    public InputVoltageRange InputVoltageRange { get; set; }
    public OutputSourcePriority OutputSourcePriority { get; set; }
    public ChargerSourcePriority ChargerSourcePriority { get; set; }

    public void Update(DeviceRating deviceRating)
    {
      InputVoltageRange = deviceRating.InputVoltageRange;
      OutputSourcePriority = deviceRating.OutputSourcePriority;
      ChargerSourcePriority = deviceRating.ChargerSourcePriority;
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

    public decimal BatteryRechargeVoltage { get; set; }
    public decimal BatteryUnderVoltage { get; set; }
    public decimal BatteryBulkVoltage { get; set; }
    public decimal BatteryFloatVoltage { get; set; }

    public void Update(DeviceRating deviceRating)
    {
      BatteryRechargeVoltage = deviceRating.BatteryRechargeVoltage;
      BatteryUnderVoltage = deviceRating.BatteryUnderVoltage;
      BatteryBulkVoltage = deviceRating.BatteryBulkVoltage;
      BatteryFloatVoltage = deviceRating.BatteryFloatVoltage;

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
    public bool Buzzer;
    public bool OverloadBypass;
    public bool PowerSaving;
    public bool DisplayTimeout;
    public bool OverloadRestart;
    public bool OverheatRestart;
    public bool Backlight;
    public bool AlarmOnPrimaryInterrupt;
    public bool FaultCodeRecord;

    public void Update(DeviceRating deviceRating)
    {

    }
    public void Update(DeviceStatus deviceRating)
    {

    }

    public void Update(DeviceFlags deviceFlags)
    {
      this.Buzzer = deviceFlags.Buzzer;
      this.Backlight = deviceFlags.Backlight;
      this.OverloadBypass = deviceFlags.OverloadBypass;
      this.PowerSaving = deviceFlags.PowerSaving;
      this.DisplayTimeout = deviceFlags.DisplayTimeout;
      this.OverloadRestart = deviceFlags.OverloadRestart;
      this.OverheatRestart = deviceFlags.OverheatRestart;
      this.AlarmOnPrimaryInterrupt = deviceFlags.AlarmOnPrimaryInterrupt;
      this.FaultCodeRecord = deviceFlags.FaultCodeRecord;
    }
  }

  public class UserSettings
  {

    public static string MESSAGE_TYPE = "UserSet";


    public DateTime EffectiveDate { get; set; }
    public InverterUserSettings inverter { get; set; }
    public BatteryUserSettings battery { get; set; }
    public SolarUserSettings solar { get; set; }
    public EnviromentUserSettings enviroment { get; set; }

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