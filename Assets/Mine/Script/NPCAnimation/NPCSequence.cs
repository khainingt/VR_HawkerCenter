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
    public GameObject paper;

    [HideInInspector]
    public bool hasServed = false;
    private bool isServing = false;

    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        animator.applyRootMotion = true;
    }

    public void StartServing()
    {   
        paper.SetActive(false);
        if (isServing) return;
        StartCoroutine(ServeRoutine());
    }

    private IEnumerator ServeRoutine()
    {
        isServing = true;
        hasServed = false;

        FindObjectOfType<ArUcoTrackingAppCoordinator>().ForceHideMarkers();

        Vector3 direction = (pickupPoint.position - transform.position).normalized;
        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 当前朝向
        Quaternion startRotation = transform.rotation;

        // 播放“视觉转身”动画
        animator.SetTrigger("Turn");

        // 在动画持续时间内逐帧插值转向目标方向
        float turnDuration = 1.19f; // 根据动画长度设置
        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终方向准确
        transform.rotation = targetRotation;

        // 等待动画播放完成
        yield return StartCoroutine(WaitForAnimation("Turn"));

        agent.SetDestination(pickupPoint.position);
        animator.SetBool("Walking", true);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 1f)
            yield return null;
        animator.SetBool("Walking", false);

        // 取菜动画
        animator.SetTrigger("PickUp");
        yield return StartCoroutine(WaitForAnimation("PickUp"));


        // 再次转身（面向 servePoint），先播放动画，然后平滑转向
        Vector3 serveDirection = (servePoint.position - transform.position).normalized;
        serveDirection.y = 0f;
        Quaternion serveTargetRotation = Quaternion.LookRotation(serveDirection);
        Quaternion serveStartRotation = transform.rotation;

        animator.SetTrigger("Turn"); // 播放相同的转身动画（或你可以用另一个动画）
        float turnBackDuration = 1.19f; // 根据动画长度设置一致
        float elapsedBack = 0f;
        while (elapsedBack < turnBackDuration)
        {
            transform.rotation = Quaternion.Slerp(serveStartRotation, serveTargetRotation, elapsedBack / turnBackDuration);
            elapsedBack += Time.deltaTime;
            yield return null;
        }
        transform.rotation = serveTargetRotation;

        tray.SetActive(true);

        // 设置移动到服务点
        agent.SetDestination(servePoint.position);
        animator.SetBool("IsHoldingTray", true);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 1f)
            yield return null;
        animator.SetBool("IsHoldingTray", false);
        animator.ResetTrigger("Turn");
        // 放下动画
        animator.SetTrigger("Place");
        yield return StartCoroutine(WaitForAnimation("Place"));

        tray.SetActive(false);
        paper.SetActive(true);

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

    void OnAnimatorMove()
    {
        if (agent.enabled)
        {
            agent.nextPosition = animator.rootPosition;
            transform.position = agent.nextPosition;
            transform.rotation = animator.rootRotation;
        }
    }
}
