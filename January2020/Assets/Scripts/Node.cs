using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Renderer Renderer { get; private set; }

    public List<Node> ConnectNode { get; private set; } = new List<Node>();

    void Start()
    {
        Renderer = GetComponent<Renderer>();
        Renderer.material.color = Color.green;
    }

    void Update()
    {

    }

    public bool NodeAddProbability()
    {
        bool flag = false;
        switch (ConnectNode.Count)
        {
            case 0: flag = true; break;
            case 1: flag = (Random.Range(0, 100) >= 95) ? (true) : (false); break;
            case 2: flag = (Random.Range(0, 100) >= 80) ? (true) : (false); break;
            default: flag = (Random.Range(0, 100) >= 75) ? (true) : (false); break;
        }
        return flag;
    }
}
