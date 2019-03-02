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

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dest;

        if(followMainCamera) {
            dest = Camera.main.transform.position;
        } else {
            dest = target.transform.position;
        }

        agent.SetDestination(dest);
    }
}
