using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillsMover : MonoBehaviour
{
    SkinnedMeshRenderer skinnedRenderer;

    // Start is called before the first frame update
    void Start()
    {
        skinnedRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //skinnedRenderer.SetBlendShapeWeight(0, Utility.SinWithRange(Time.time * 3.14f, 0, 100));
        skinnedRenderer.SetBlendShapeWeight(0, Utility.SinWithRangePeriodOffset(Time.time, 0, 50, period: 3f,   offset:0));
        skinnedRenderer.SetBlendShapeWeight(1, Utility.SinWithRangePeriodOffset(Time.time, 0, 50, period: 3.5f, offset:1f));
    }
}

public class MotionParameters
{
    public int shapeKey;
    public float secondsPerCycle;
    public float bottomValue;
    public float topValue;
    public float secondsOffset;
}