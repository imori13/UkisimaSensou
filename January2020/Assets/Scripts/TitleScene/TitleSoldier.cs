using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSoldier : MonoBehaviour
{
    public Animator Animator { get; private set; }
    void Start()
    {
        Animator = GetComponent<Animator>();
        Animator.CrossFade("TitleSoldierAttack", 0, 0, Random.Range(0f, 1f));
    }
}
