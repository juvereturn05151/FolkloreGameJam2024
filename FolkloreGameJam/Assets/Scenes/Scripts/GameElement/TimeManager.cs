using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public event Action<string> OnTimeChanged;

    // Time variables
    [SerializeField]
    private float timeSpeed = 1.0f;  // Speed at which time progresses
    private float currentTime = 18.0f;  // Starting at 6:00 PM (18:00 in 24-hour format)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Simulate time passage
        currentTime += timeSpeed * Time.deltaTime;

        // If time reaches 6:00 AM (30.0), reset to 6:00 PM (18.0)
        if (currentTime >= 30.0f)
        {
            currentTime = 18.0f;  // Reset back to 6:00 PM
        }

        // Notify UIManager about the updated time
        string formattedTime = FormatTime(currentTime);
        OnTimeChanged?.Invoke(formattedTime);
    }

    private string FormatTime(float time)
    {
        // Convert time from float to hours and minutes (24-hour format)
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.FloorToInt((time - hours) * 60);

        string period;
        int displayHour;

        // Handle time between 18:00 (6 PM) and 29:59 (5:59 AM next day)
        if (hours >= 18 && hours < 24)  // PM case: 18:00 to 23:59
        {
            period = "PM";
            displayHour = hours - 12;  // Convert 18:00-23:59 to 6:00-11:59 PM
        }
        else  // AM case: 00:00 (24.0) to 05:59 (29.59)
        {
            period = "AM";
            displayHour = hours >= 24 ? hours - 24 : hours;  // Convert 24:00-29:59 to 12:00-5:59 AM
        }

        // If the display hour is 0, that means it's 12:00 (for both AM and PM)
        if (displayHour == 0)
        {
            displayHour = 12;
        }

        // Return formatted time in 12-hour clock with AM/PM
        return string.Format("{0:D2}:{1:D2} {2}", displayHour, minutes, period);
    }
}