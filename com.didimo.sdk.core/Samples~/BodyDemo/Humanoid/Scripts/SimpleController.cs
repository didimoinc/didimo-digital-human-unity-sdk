using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SimpleController : MonoBehaviour
{
    public ControlType controlType = ControlType.AI;

    [Header("Movement")]
    [Range(0.1f, 10)] public float walkSpeed = 2;
    [Range(0.1f, 10)] public float runSpeed = 6;
    [Range(0.1f, 10)] public float lookForward = 3;

    [Header("AI Only")]
    [Range(0.1f, 10)] public float waitTime = 5;
    [Range(0, 1)] public float waitChance = 0.3f;

    public float currentSpeed = 0;

    NavMeshAgent agent => GetComponentInChildren<NavMeshAgent>();
    Animator animator => GetComponentInChildren<Animator>();

    void Start()
    {
        switch (controlType)
        {
            case ControlType.AI: StartCoroutine(AIControl()); break;
            case ControlType.Player: StartCoroutine(PlayerControl()); break;
        }
    }

    void LateUpdate() => updateSpeed();

    IEnumerator AIControl()
    {
        while (enabled)
        {
            var positions = FindObjectsOfType<POIMove>().Select(poi => poi.transform.position);
            var position = positions.Where(p => Vector3.Distance(p, transform.position) > agent.stoppingDistance + 1).OrderBy(p => Random.value).First();
            agent.SetDestination(position);

            yield return null;

            while (agent.remainingDistance > agent.stoppingDistance)
                yield return new WaitForSeconds(0.25f);

            if (Random.value < waitChance)
                yield return new WaitForSeconds(Random.value * waitTime);
        }
    }

    IEnumerator PlayerControl()
    {
        while (enabled)
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if (input.magnitude != 0)
            {
                agent.isStopped = false;
                NavMeshPath path = new NavMeshPath();
                var o = input.normalized * lookForward;
                if (NavMesh.SamplePosition(transform.position + o, out NavMeshHit hit, o.magnitude, NavMesh.AllAreas))
                    if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path) && path.status != NavMeshPathStatus.PathInvalid)
                        agent.SetPath(path);
            }
            else agent.isStopped = true;
            yield return null;
        }
    }

    void updateSpeed()
    {
        currentSpeed = agent.velocity.magnitude;
        animator?.SetFloat("Speed", currentSpeed);
    }

    public enum ControlType
    {
        Player, AI
    }

}
