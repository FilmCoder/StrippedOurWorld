using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyBox;

public class FollowSomeone : MonoBehaviour
{
    [Tooltip("Will follow the main camera if this is checked. If not checked, specify a specific game object as a target.")]
    public bool followMainCamera = false;

    [Tooltip("Specific game object to follow.")]
    [ConditionalField("followMainCamera", false)]
    public GameObject target;

    public bool useStopFollowCollider = false;

    [ConditionalField("useStopFollowCollider")]
    [Tooltip("Stop following target if touching any of these triggers")]
    public Collider stopFollowCollider = null;

    [ConditionalField("useStopFollowCollider")]
    [Tooltip("Seconds to wait after touching stopFollowCollider until resuming follow")]
    public float freezeAfterPeriod = 1;

    private NavMeshAgent agent; // agent attached to this same gameobject, used to follow the target
    private float timeOfExitFromStopCollider; // keeps track of when the enemy became free of stop collider  

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.isStopped && Time.time - timeOfExitFromStopCollider > freezeAfterPeriod)
        {
            agent.isStopped = false;
        }

        Vector3 dest;

        if (followMainCamera)
        {
            dest = Camera.main.transform.position;
        }
        else
        {
            dest = target.transform.position;
        }

        agent.SetDestination(dest);
    }

    private void OnTriggerStay(Collider other)
    {
        if (stopFollowCollider && other == stopFollowCollider)
        {
            timeOfExitFromStopCollider = Time.time;
            agent.isStopped = true;
        }
    }
}
