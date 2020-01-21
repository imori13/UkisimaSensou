using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishAnimation : MonoBehaviour
{
    void Start()
    {
        GetComponent<Animator>().enabled = false;
    }
}
