using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TryAR.MarkerTracking;

public class NPCSequence : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;

    public Transform servePoint;   
    public Transform pickupPoint;   
    public GameObject tray;         

    [HideInInspector]
    public bool hasServed = false;  
    private bool isServing = false; 


    public void StartServing()
    {
        if (isServing) return;
        StartCoroutine(ServeRoutine());
    }

    private IEnumerator ServeRoutine()
    {
        isServing = true;
        hasServed = false;

        FindObjectOfType<ArUcoTrackingAppCoordinator>().ForceHideMarkers();

        animator.SetTrigger("Turn");
        yield return StartCoroutine(WaitForAnimation("Turn"));

        agent.SetDestination(pickupPoint.position);
        animator.SetBool("Walking", true);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            yield return null;
        animator.SetBool("Walking", false);

        animator.SetTrigger("PickUp");
        yield return StartCoroutine(WaitForAnimation("PickUp"));

        if (tray != null) tray.SetActive(true);

        //animator.SetTrigger("Turn");
        //yield return StartCoroutine(WaitForAnimation("Turn"));

        agent.SetDestination(servePoint.position);
        animator.SetBool("IsHoldingTray", true);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            yield return null;
        animator.SetBool("IsHoldingTray", false);

        animator.SetTrigger("Place");
        yield return StartCoroutine(WaitForAnimation("Place"));

        if (tray != null) tray.SetActive(false);


        hasServed = true;
        isServing = false;
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }
}
