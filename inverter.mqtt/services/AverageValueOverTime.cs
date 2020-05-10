using System;

public class AverageValueOverTime
{
    public enum TimePeriod
    {
        Minute,
        Hour,
        Day
    }
    public class AverageOverTimeUpdate
    {
        public string valueName { get; set; }
        public TimePeriod timePeriod { get; set; }
        public decimal updatedValue { get; set; } 
    }

    private long minuteCnt = 0, hourCnt = 0, dayCnt = 0;
    private decimal minuteAvg = 0, hourAvg = 0, dayAvg = 0;
    private int currentMinute, currentHour, currentDay;
    private string valueName;
    private Action<AverageOverTimeUpdate> onPeriodUpdate;


    public AverageValueOverTime(
        string ValueName, 
        Action<AverageOverTimeUpdate> onPeriodUpdate)
    {
        var now = DateTime.Now;
        currentMinute = now.Minute;
        currentHour = now.Hour;
        currentDay = now.DayOfYear;

        this.valueName = ValueName;
        this.onPeriodUpdate = onPeriodUpdate;
    }

    private decimal round(decimal value)
    {
        int v = (int)(value * 100);
        return v / 100;

    }
    public void addValue(decimal value)
    {

        minuteAvg = ((minuteAvg * minuteCnt) + value) / (minuteCnt + 1);
        hourAvg = ((hourAvg * hourCnt) + value) / (hourCnt + 1);
        dayAvg = ((dayAvg * dayCnt) + value) / (dayCnt + 1);

        minuteCnt += 1;
        hourCnt += 1;
        dayCnt += 1; 

        var now = DateTime.Now;

        if (onPeriodUpdate != null)
        {
            if (currentMinute != now.Minute)
            {
                onPeriodUpdate(new AverageOverTimeUpdate { timePeriod = TimePeriod.Minute, valueName = this.valueName, updatedValue = round(minuteAvg) });
                minuteAvg = minuteCnt = 0;
                currentMinute = now.Minute;
            }
            if (currentHour != now.Hour)
            {
                onPeriodUpdate(new AverageOverTimeUpdate { timePeriod = TimePeriod.Hour, valueName = this.valueName, updatedValue = round(hourAvg) });
                hourAvg = hourCnt = 0;
                currentHour = now.Hour;
            }
            if (currentMinute != now.DayOfYear)
            {
                onPeriodUpdate(new AverageOverTimeUpdate { timePeriod = TimePeriod.Day, valueName = this.valueName, updatedValue = round(dayAvg) });
                dayAvg = dayCnt = 0;
                currentDay = now.Day;
            }
        }
    }

}