using UnityEngine;

public class Utility
{
    public static readonly float PI = 3.14159f;

    public static float SinWithRange(float value, float bottom, float top)
    {
        float distance = top - bottom;
        float output = (Mathf.Sin(value) + 1f) / 2f * distance + bottom;
        //Debug.Log("SinWithRange: " + output);
        return output;
    }

    public static float SinWithRangePeriodOffset(float value, float bottom, float top, float period=1, float offset=0)
    {
        float distance = top - bottom;
        float output = (Mathf.Sin(value*PI/period + offset*PI) + 1f) / 2f * distance + bottom;
        //Debug.Log("SinWithRange: " + output);
        return output;
    }
}
