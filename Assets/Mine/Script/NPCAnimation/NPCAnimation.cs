using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    public Transform PlayerController; 
    public float waveDistance = 7f;    

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (PlayerController == null)
            PlayerController = GameObject.FindWithTag("PlayerController")?.transform; 
    }

    void Update()
    {
        if (PlayerController != null)
        {
            float dist = Vector3.Distance(transform.position, PlayerController.position);

            if (dist <= waveDistance)
            {
                animator.SetBool("IsWaving", true);
            }
            else
            {
                animator.SetBool("IsWaving", false);
            }
        }
    }
}

