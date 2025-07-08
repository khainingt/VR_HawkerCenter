using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject Food;
    void Awake()
    {
        if (Food != null) Food.SetActive(false);
    }

    public void OnSelect()
    {
        if (Food != null) Food.SetActive(true);
    }
}
