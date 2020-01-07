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
    [SerializeField] GameObject CommanderPrefab;
    [SerializeField] GameObject SoldierPrefab;

    public Color Normal_Color { get; set; }   // 国ごとの通常色
    public PlayerEnum PlayerEnum { get; set; } = PlayerEnum.None;

    public Renderer Renderer { get; private set; }

    // 繋がっているノードの参照
    public List<Node> ConnectNode { get; private set; } = new List<Node>();
    // 繋がっている道の参照 (マップの生成時に孤立を削除するときに必要)
    public List<Road> ConnectRoad { get; private set; } = new List<Road>();
    // ノードに属している指揮官の参照
    public List<Commander> Commander { get; private set; } = new List<Commander>();
    // ノードに属している兵士の参照
    public List<Soldier> Soldier { get; private set; } = new List<Soldier>();

    // 本拠地か？
    public bool IsBaseNode { get; set; }

    void Start()
    {
        // とりあえずランダムで国を決める
        //PlayerEnum = (PlayerEnum)Random.Range(0, (int)PlayerEnum.Count);
        //UpdateNodeColor();

        Renderer = GetComponent<Renderer>();
        Renderer.material.color = Normal_Color;

        int[] array1 = new int[] { 0, 20, 50, 40, 30, 20 };
        for (int i = 0; i < MyMath.GetRandomIndex(array1); i++)
        {
            Commander commander = Instantiate(CommanderPrefab).GetComponent<Commander>();
            float scale = transform.localScale.x / 2f;
            commander.gameObject.transform.position
                = transform.position
                + (Vector3.up * 0.5f)
                + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
            commander.UpdateNode(this);
            Commander.Add(commander);
        }

        int[] array2 = new int[] { 010, 20, 30, 40, 50, 40, 30, 20, 10 };
        for (int i = 0; i < MyMath.GetRandomIndex(array2); i++)
        {
            Soldier soldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
            float scale = transform.localScale.x / 2f;
            soldier.gameObject.transform.position
                = transform.position
                + (Vector3.up * 0.5f)
                + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
            soldier.UpdateNode(this);
            Soldier.Add(soldier);
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
        switch (PlayerEnum)
        {
            case PlayerEnum.Player01: Normal_Color = PLAYER01_COLOR; break;
            case PlayerEnum.Player02: Normal_Color = PLAYER02_COLOR; break;
            case PlayerEnum.Player03: Normal_Color = PLAYER03_COLOR; break;
            case PlayerEnum.None: Normal_Color = Color.black; break;
        }

        if (IsBaseNode) { Normal_Color *= 2; }
    }

    public void Attack(Node node)
    {
        // 指揮官が二人未満なら早期リターン
        if (Commander.Count < 2) return;

        // 兵士がいないなら早期リターン
        if (Soldier.Count <= 0) return;

        // バトル
        bool win = Battle(node);

        // 勝ったら
        if (win)
            AttackWin(node);
        // 負けたら
        else
            AttackLose(node);
    }

    public void Move(Node node)
    {
        // 兵士がいないなら早期リターン
        if (Soldier.Count <= 0) return;

        // 指揮官を選択したノードから右クリックしたノードに移す
        node.Soldier.AddRange(Soldier);
        // 親を更新
        Soldier.ForEach(c => c.UpdateNode(this));
        node.Soldier.ForEach(c => c.UpdateNode(node));

        // 現在選択しているノードの指揮官をクリア
        Soldier.Clear();
    }

    bool Battle(Node node)
    {
        return true;
    }

    void AttackWin(Node node)
    {
        // インデックス最初のやつを保持
        Commander firstCommander = Commander[0];
        // インデックスの最初のやつをリストから一時的に外す
        Commander.RemoveAt(0);

        // 相手の指揮官を消して自分の兵士を指揮官させる
        node.Commander.ForEach(n => Destroy(n.gameObject));
        node.Commander.Clear();
        node.Commander.AddRange(Commander);
        Commander.Clear();

        // 自分の兵士を１人消す
        Destroy(Soldier[0].gameObject);
        Soldier.RemoveAt(0);
        // 相手の兵士を消して自分の兵士を移動させる
        node.Soldier.ForEach(n => Destroy(n.gameObject));
        node.Soldier.Clear();
        node.Soldier.AddRange(Soldier);
        Soldier.Clear();

        // 相手のノードの占有者を変える
        node.PlayerEnum = PlayerEnum;

        // ノードの色を更新
        node.UpdateNodeColor();
        UpdateNodeColor();
        node.Renderer.material.color = node.Normal_Color;
        node.Renderer.material.color += node.Normal_Color / 4f;

        // 一時的に保持していた指揮官をリストに戻す
        Commander.Add(firstCommander);

        // 親を更新
        Commander.ForEach(c => c.UpdateNode(this));
        node.Commander.ForEach(c => c.UpdateNode(node));
        Soldier.ForEach(c => c.UpdateNode(this));
        node.Soldier.ForEach(c => c.UpdateNode(node));
    }

    // 負けたら指揮官が１人に、兵士が消える
    void AttackLose(Node node)
    {
        // インデックス最初のやつを保持
        Commander firstCommander = Commander[0];
        // インデックスの最初のやつをリストから一時的に外す
        Commander.RemoveAt(0);
        // リストの中にいる指揮官をすべて削除
        Commander.ForEach(c => Destroy(c.gameObject));
        Commander.Clear();
        // 一時的に外していた指揮官を再び追加
        Commander.Add(firstCommander);
    }
}
