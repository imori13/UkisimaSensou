using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCommander : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public SkinnedMeshRenderer SkinnedMeshRenderer;

    void Start()
    {
        Animator = GetComponent<Animator>();
        Animator.CrossFade(0, 0, 0, Random.Range(0f, 1f));
    }

    void Update()
    {

    }
}
