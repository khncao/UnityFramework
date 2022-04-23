using UnityEngine;

namespace m4k {
[System.Serializable]
public class GameTime : Singleton<GameTime> {
    [System.Serializable]
    public class TimeProfile {
        public string profileName;
        [Range(0.1f, 5f)]
        public float ticksPerSecond = 1f;
        public float timeMult = 1f;
        public float secondsPerDay = 1440f;
        public float secondsPerHour = 60f;
        // public int hoursPerDay = 24;
        public int daysInWeek = 7;
        public int daysInMonth = 30;
        public int monthsInYear = 12;
        public float dayStartTime = 360f;
        public float nightStartTime = 960f;
    }

    public struct GameSeason {
        public string name;
    }
    public struct GameMonth {
        public string name;
        public int days;
    }
    public struct DayPeriod { // Midnight, dawn, noon, evening, dusk
        public string name;
        public int startTime;
    }
    public bool paused;

    public long ticks;
    public int day;

    public FloatSO timeOfDaySO;

    public TimeProfile timeProfile;

    public GameMonth[] gameMonths;
    public DayPeriod[] dayPeriods;

    public float timeOfDay { 
        get { return _timeOfDay; }
        set {
            _timeOfDay = value;
            if(timeOfDaySO) timeOfDaySO.value = value;
        }
    }

    public int week { get; private set; } 
    public int month { get; private set; }
    public int year { get; private set; }

    public System.Action<int> onTickTime, onDayPeriod, hourly, daily, weekly, monthly, yearly;
    public System.Action<long> onTick;
    
    float _timeOfDay;
    float _tickTimer = 0f;
    int _currentDayPeriod = 0;

    public void SetGameMonths(GameMonth[] gameMonths) {
        this.gameMonths = gameMonths;
    }

    public void SetDayPeriods(DayPeriod[] dayPeriods) {
        this.dayPeriods = dayPeriods;
    }

    public void SetTimeScale(float t) {
        Time.timeScale = t;
    }

    // void Start() {
        
    // }

    void Update() {
        timeOfDay = (timeOfDay + Time.deltaTime * timeProfile.timeMult);

        if(timeOfDay > timeProfile.secondsPerDay) {
            timeOfDay = 0;
            day++;
            daily?.Invoke(day);
            CheckDaily();
        }

        _tickTimer += Time.deltaTime;

        if(_tickTimer > timeProfile.ticksPerSecond) {
            ticks++;
            _tickTimer = 0;
            onTick?.Invoke(ticks);
            onTickTime?.Invoke((int)timeOfDay);

            if(timeOfDay % timeProfile.secondsPerHour < timeProfile.timeMult) {
                hourly?.Invoke((int)timeOfDay);
            }
        }
    }

    void CheckDaily() {
        if(day % (timeProfile.daysInWeek + 1) == 0) {
            week++;
            weekly?.Invoke(week);
        }

        if(gameMonths != null && gameMonths.Length > 0) {
            if(day % (gameMonths[month].days + 1) == 0) {
                month++;

                if(month % (gameMonths.Length + 1) == 0) {
                    month = 0;
                    year++;
                    yearly?.Invoke(year);
                }
                monthly?.Invoke(month);
            }
        }
        else {
            if(day % (timeProfile.daysInMonth + 1) == 0) {
                month++;

                if(month % (timeProfile.monthsInYear + 1) == 0) {
                    month = 0;
                    year++;
                    yearly?.Invoke(year);
                }
                monthly?.Invoke(month);
            }

        }
    }

    public string DateToString() {
        var monthDisplay = gameMonths != null ? gameMonths[month].name : month.ToString();
        return $"{day}, {monthDisplay}, {year}";
    }

    public string TimeToString() {
        return System.String.Format("{0:00}:{1:00}", (timeOfDay / timeProfile.secondsPerHour), (timeOfDay % timeProfile.secondsPerHour));
    }
}
}