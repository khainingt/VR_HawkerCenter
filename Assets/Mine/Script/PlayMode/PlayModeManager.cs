using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayModeManager : MonoBehaviour
{
    [Header("References")]
    public GameObject mainMenuUI;
    public GameObject blackScreenPanel;
    public GameObject tipText;
    public ArmSwingLocomotion armSwingScript;
    public Transform player;
    public Vector3 resetPosition = new Vector3(19f, 0f, -66f);

    private bool isRoamingMode = false;
    private bool waitingForAKey = false;

    void Update()
    {
        // 等待 A键 开始漫游
        if (waitingForAKey && OVRInput.GetDown(OVRInput.Button.One))  // A键
        {
            waitingForAKey = false;
            StartCoroutine(FadeFromBlackAndStartRoaming());
        }

        // Roaming 模式中按下 B键 退出
        if (isRoamingMode && OVRInput.GetDown(OVRInput.Button.Two))  // B键
        {
            StartCoroutine(ExitRoamingMode());
        }
    }

    public void EnterRoamingMode()
    {
        StartCoroutine(StartRoamingMode());
    }

    private IEnumerator StartRoamingMode()
    {
        // 1. 隐藏主菜单
        mainMenuUI.SetActive(false);

        // 2. 黑屏 + 提示
        blackScreenPanel.SetActive(true);
        tipText.SetActive(true);

        // 3. 等待 A键
        waitingForAKey = true;
        yield return null;
    }

    private IEnumerator FadeFromBlackAndStartRoaming()
    {
        // 关闭黑屏和提示
        blackScreenPanel.SetActive(false);
        tipText.SetActive(false);

        // 启用移动脚本
        armSwingScript.enableMovement = true;
        isRoamingMode = true;

        yield return null;
    }

    private IEnumerator ExitRoamingMode()
    {
        // 渐黑 + 停止移动
        blackScreenPanel.SetActive(true);
        tipText.SetActive(false);

        yield return new WaitForSeconds(1f);

        // 重置位置
        player.position = resetPosition;

        armSwingScript.enableMovement = false;
        isRoamingMode = false;

        // 显示主菜单
        mainMenuUI.SetActive(true);
        blackScreenPanel.SetActive(false);
    }
}
