using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCommander : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public SkinnedMeshRenderer SkinnedMeshRenderer;

    void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {

    }
}
