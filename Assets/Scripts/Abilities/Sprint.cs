using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Sprint : UnitAbility
{
    public float speedMultiplier = 1.5f;
    public float sprintTime = 2f;
    float baseSpeed;

    private void Start()
    {
        baseSpeed = GetComponent<NavMeshAgent>().speed;
    }

    public override void Execute()
    {
        GetComponent<NavMeshAgent>().speed *= speedMultiplier;
        StartCoroutine(ResetSpeed());
        base.Execute();
    }

    public override void Queue()
    {
        base.Queue();
    }

    IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(sprintTime);
        GetComponent<NavMeshAgent>().speed = baseSpeed;
    }
}
