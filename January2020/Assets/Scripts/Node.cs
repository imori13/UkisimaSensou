using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public static readonly Color PLAYER01_COLOR = new Color(255, 150, 150) / 255f;
    public static readonly Color PLAYER02_COLOR = new Color(150, 150, 255) / 255f;
    public static readonly Color PLAYER03_COLOR = new Color(150, 255, 150) / 255f;
    public static readonly Color SELECT_COLOR = Color.white;
    public static readonly Color CONNECT_COLOR = Color.white;


    // prefab
    [SerializeField] GameObject Commander;

    public Color Normal_Color { get; private set; }   // 国ごとの通常色
    public PlayerEnum playerEnum;

    public Renderer Renderer { get; private set; }

    // 繋がっているノードの参照
    public List<Node> ConnectNode { get; private set; } = new List<Node>();
    // 繋がっている道の参照 (マップの生成時に孤立を削除するときに必要)
    public List<Road> ConnectRoad { get; private set; } = new List<Road>();
    // ノードに属している指揮官の参照
    public List<Commander> ConnectCommander { get; private set; } = new List<Commander>();
    // ノードに属している兵士の参照
    public List<Soldier> ConnectSoldier { get; private set; } = new List<Soldier>();

    void Start()
    {
        // とりあえずランダムで国を決める
        playerEnum = (PlayerEnum)Random.Range(0, (int)PlayerEnum.Count);
        UpdateNodeColor();

        Renderer = GetComponent<Renderer>();
        Renderer.material.color = Normal_Color;

        int[] array = new int[] { 20, 50, 40, 30, 20 };
        for (int i = 0; i < MyMath.GetRandomIndex(array); i++)
        {
            Commander commander = Instantiate(Commander).GetComponent<Commander>();
            float scale = transform.localScale.x / 2f;
            commander.gameObject.transform.position
                = transform.position
                + (Vector3.up * 0.5f)
                + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
            ConnectCommander.Add(commander);
        }

    }

    void Update()
    {

    }

    // 道を追加するときの確率(既につながっている道の数に応じた確率)
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

    public void UpdateNodeColor()
    {
        switch (playerEnum)
        {
            case PlayerEnum.Player01: Normal_Color = PLAYER01_COLOR; break;
            case PlayerEnum.Player02: Normal_Color = PLAYER02_COLOR; break;
            case PlayerEnum.Player03: Normal_Color = PLAYER03_COLOR; break;
        }
    }

    public void Attack(Node node)
    {

    }

    public void Move(Node node)
    {

    }
}
