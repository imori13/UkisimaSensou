using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public static Color NORMALCOLOR = new Color(50, 255, 100) / 255f;

    public Renderer Renderer { get; private set; }

    // 繋がっているノードの参照
    public List<Node> ConnectNode { get; private set; } = new List<Node>();
    // 繋がっている道の参照 (マップの生成時に孤立を削除するときに必要)
    public List<Road> ConnectRoad { get; private set; } = new List<Road>();

    void Start()
    {
        Renderer = GetComponent<Renderer>();
        Renderer.material.color = NORMALCOLOR;
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
            case 1: flag = (Random.Range(0, 100) < 98) ? (true) : (false); break;
            case 2: flag = (Random.Range(0, 100) < 90) ? (true) : (false); break;
            default: flag = (Random.Range(0, 100) < 85) ? (true) : (false); break;
        }
        return flag;
    }
}
