using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCSequence : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;

    public GameObject foodItem;
    public Transform handPivot;
    public Transform dropSpot;

    public float pickupDuration = 2f;
    public float dropDuration = 2f;

    private enum State { Idle, GoingToPickUp, PickingUp, GoingToDrop, Dropping }
    private State currentState = State.Idle;

    public bool hasServed { get; private set; } = false;

    private void Start()
    {
        agent.stoppingDistance = 0.1f;
    }

    public void StartSequence()
    {
        hasServed = false; // 重置标志位
        StartCoroutine(NPCSequenceRoutine());
    }

    IEnumerator NPCSequenceRoutine()
    {
        // Step 1: Move to food item
        currentState = State.GoingToPickUp;
        agent.SetDestination(foodItem.transform.position);
        animator.SetBool("IsWalking", true);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        animator.SetBool("IsWalking", false);

        // Step 2: Pick up food
        currentState = State.PickingUp;
        animator.SetTrigger("IsPickingUp");
        yield return new WaitForSeconds(pickupDuration);
        AttachFoodToHand();

        // Step 3: Move to drop spot
        currentState = State.GoingToDrop;
        agent.SetDestination(dropSpot.position);
        animator.SetBool("IsWalking", true);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        animator.SetBool("IsWalking", false);

        // Step 4: Drop food
        currentState = State.Dropping;
        animator.SetTrigger("IsDropping");
        yield return new WaitForSeconds(dropDuration);
        //DropFood();

        hasServed = true; // 设置为已上菜

        // Step 5: Idle
        currentState = State.Idle;
        animator.SetTrigger("IsIdle");
    }

    private void AttachFoodToHand()
    {
        foodItem.SetActive(true);
        foodItem.transform.SetParent(handPivot, false);
        foodItem.transform.localPosition = Vector3.zero;
        foodItem.transform.localRotation = Quaternion.identity;
    }

    /*private void DropFood()
    {
        foodItem.transform.SetParent(dropSpot, false);
        foodItem.transform.localPosition = Vector3.zero;
        foodItem.transform.localRotation = Quaternion.identity;
    }*/
}
