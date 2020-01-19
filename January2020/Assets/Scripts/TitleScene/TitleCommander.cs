using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCommander : MonoBehaviour
{
    public Animator Animator { get; private set; }
    void Start()
    {
        Animator = GetComponent<Animator>();
        Animator.CrossFade("TitleCommanderAttack", 0, 0, Random.Range(0f, 1f));
    }
}
