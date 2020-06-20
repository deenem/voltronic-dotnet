namespace inverter.common.model
{
    public class MQTT
    {
        public string server { get; set; }
        public string port { get; set; }
        public string topic { get; set; }
        public string devicename { get; set; }
        public string username { get; set; }
        public string password { get; set; }

    }

    public class RabbitMQ
    {
        public string server { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public class Inverter
    {
        public int BatteryAmpHours { get; set; }
        public int WRating { get; set; }
        public decimal DoD { get; set; }
    }

    public class AppSettings
    {
        public string LibVoltronicPath{ get; set; }
        public RabbitMQ RabbitMQ { get; set; }
        public MQTT MQTT { get; set; }
        public Inverter Inverter { get; set;}
    }
}