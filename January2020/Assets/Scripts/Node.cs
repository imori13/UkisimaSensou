using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    // prefab
    [SerializeField] GameObject CommanderPrefab;
    [SerializeField] GameObject SoldierPrefab;

    [SerializeField] Mesh NodeMesh;
    [SerializeField] Mesh BaseMesh;
    [SerializeField] Mesh TowerMesh;
    [SerializeField] Material Player01Material;
    [SerializeField] Material Player01BaseMaterial;
    [SerializeField] Material Player01TowerMaterial;
    [SerializeField] Material Player02Material;
    [SerializeField] Material Player02BaseMaterial;
    [SerializeField] Material Player02TowerMaterial;
    [SerializeField] Material Player03Material;
    [SerializeField] Material Player03BaseMaterial;
    [SerializeField] Material Player03TowerMaterial;
    [SerializeField] Material Player04Material;
    [SerializeField] Material Player04BaseMaterial;
    [SerializeField] Material Player04TowerMaterial;

    public Color Normal_Color { get; set; }   // 国ごとの通常色
    public PlayerEnum PlayerEnum { get; set; } = PlayerEnum.None;

    public MeshFilter MeshFilter { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }

    // MAPクラス
    public MapManager Map { get; set; }
    // 繋がっているノードの参照
    public List<Node> ConnectNode { get; private set; } = new List<Node>();
    // 繋がっている道の参照 (マップの生成時に孤立を削除するときに必要)
    public List<Road> ConnectRoad { get; private set; } = new List<Road>();
    // ノードに属している指揮官の参照
    public List<Commander> Commander { get; private set; } = new List<Commander>();
    // ノードに属している兵士の参照
    public List<Soldier> Soldier { get; private set; } = new List<Soldier>();

    public GameManager GameManager { get; set; }
    // 本拠地か？
    public bool IsBaseNode { get; set; }
    // 本拠地用モデル
    GameObject Tower = null;
    // ルート検索用コスト
    public float Cost { get; set; } = float.MaxValue;
    public bool Done { get; set; } = false;
    public Node PrevNode { get; set; } = null;
    public bool MovePermission { get; set; } = true;    // このノードに移動可能か
    public float Scale { get; set; }    // オブジェクト自体の大きさの直径

    // 自分のノードに向かっている敵味方区別しないMoveBoxリスト
    public List<MoveBox> HeadingMovebox { get; private set; } = new List<MoveBox>();
    public int CommanderCount { get { return Commander.Count + HeadingMovebox.Sum(h => h.Commander.Count()); } }
    public int SoldierCount { get { return Soldier.Count + HeadingMovebox.Sum(h => h.Soldier.Count()); } }
    // 本拠地とつながっているかどうか
    public bool IsConnectMainBase
    {
        get
        {
            if (PlayerEnum == PlayerEnum.None) return false;

            List<Node> openList = new List<Node> { this };
            Hoge(ref openList, this);
            void Hoge(ref List<Node> list, Node node)
            {
                foreach (var connect in node.ConnectNode)
                {
                    if (PlayerEnum != connect.PlayerEnum) continue;
                    if (list.Contains(connect)) continue;

                    openList.Add(connect);
                    Hoge(ref list, connect);
                }
            }
            return openList.Contains(Map.PlayerBaseNode[(int)PlayerEnum]);
        }
    }

    public void Initialize()
    {
        if (PlayerEnum == PlayerEnum.None) return;

        Commander commander = Instantiate(CommanderPrefab).GetComponent<Commander>();
        float scale = transform.localScale.x / 2f;
        commander.gameObject.transform.position
            = transform.position
            + (Vector3.up * 0.5f)
            + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
        commander.UpdateNode(this);
        Commander.Add(commander);

        //if(PlayerEnum == PlayerEnum.Player01)
        //{
        //    for(int i = 0; i < 50; i++)
        //    {
        //        Commander commander1 = Instantiate(CommanderPrefab).GetComponent<Commander>();
        //        float scale2 = transform.localScale.x / 2f;
        //        commander1.gameObject.transform.position
        //            = transform.position
        //            + (Vector3.up * 0.5f)
        //            + (new Vector3(Random.Range(-scale2, scale2), 0, Random.Range(-scale2, scale2)));
        //        commander1.UpdateNode(this);
        //        Commander.Add(commander1);

        //        Soldier soldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
        //        float scale1 = transform.localScale.x / 2f;
        //        soldier.gameObject.transform.position
        //            = transform.position
        //            + (Vector3.up * 0.5f)
        //            + (new Vector3(Random.Range(-scale1, scale1), 0, Random.Range(-scale1, scale1)));
        //        soldier.UpdateNode(this);
        //        Soldier.Add(soldier);
        //    }
        //}
    }

    void Start()
    {
        MovePermission = true;

        MeshRenderer = GetComponent<MeshRenderer>();
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer.material.color = Normal_Color;

        //int[] array1 = new int[] { 0, 20, 50, 40, 30, 20 };
        //for (int i = 0; i < MyMath.GetRandomIndex(array1); i++)
        //{
        //    Commander commander = Instantiate(CommanderPrefab).GetComponent<Commander>();
        //    float scale = transform.localScale.x / 2f;
        //    commander.gameObject.transform.position
        //        = transform.position
        //        + (Vector3.up * 0.5f)
        //        + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
        //    commander.UpdateNode(this);
        //    Commander.Add(commander);
        //}

        //int[] array2 = new int[] { 010, 20, 30, 40, 50, 40, 30, 20, 10 };
        //for (int i = 0; i < MyMath.GetRandomIndex(array2); i++)
        //{
        //    Soldier soldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
        //    float scale = transform.localScale.x / 2f;
        //    soldier.gameObject.transform.position
        //        = transform.position
        //        + (Vector3.up * 0.5f)
        //        + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
        //    soldier.UpdateNode(this);
        //    Soldier.Add(soldier);
        //}
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

    GameObject CreateTower(Material material)
    {
        GameObject instance = new GameObject("Tower");
        instance.transform.SetParent(transform);
        instance.transform.position = transform.position;
        instance.transform.localScale = Vector3.one * 0.5f;
        instance.transform.Rotate(0, Random.Range(0, 360f), 0);
        instance.AddComponent<MeshFilter>().mesh = TowerMesh;
        instance.AddComponent<MeshRenderer>().material = material;

        return instance;
    }

    public void UpdateNodeColor()
    {
        if (MeshRenderer == null) { MeshRenderer = GetComponent<MeshRenderer>(); }
        if (MeshFilter == null) { MeshFilter = GetComponent<MeshFilter>(); }
        switch (PlayerEnum)
        {
            case PlayerEnum.Player01:
                if (IsBaseNode)
                {
                    if (Tower == null)
                        Tower = CreateTower(Player01TowerMaterial);
                    MeshFilter.mesh = BaseMesh;
                    MeshRenderer.material = Player01BaseMaterial;
                }
                else
                {
                    MeshFilter.mesh = NodeMesh;
                    MeshRenderer.material = Player01Material;
                }
                Normal_Color = Color.green;
                break;
            case PlayerEnum.Player02:
                if (IsBaseNode)
                {
                    if (Tower == null)
                        Tower = CreateTower(Player02TowerMaterial);
                    MeshFilter.mesh = BaseMesh;
                    MeshRenderer.material = Player02BaseMaterial;
                }
                else
                {
                    MeshFilter.mesh = NodeMesh;
                    MeshRenderer.material = Player02Material;
                }
                Normal_Color = Color.white;
                break;
            case PlayerEnum.Player03:
                if (IsBaseNode)
                {
                    if (Tower == null)
                        Tower = CreateTower(Player03TowerMaterial);
                    MeshFilter.mesh = BaseMesh;
                    MeshRenderer.material = Player03BaseMaterial;
                }
                else
                {
                    MeshFilter.mesh = NodeMesh;
                    MeshRenderer.material = Player03Material;
                }
                Normal_Color = Color.white;
                break;
            case PlayerEnum.Player04:
                if (IsBaseNode)
                {
                    if (Tower == null)
                        Tower = CreateTower(Player04TowerMaterial);
                    MeshFilter.mesh = BaseMesh;
                    MeshRenderer.material = Player04BaseMaterial;
                }
                else
                {
                    MeshFilter.mesh = NodeMesh;
                    MeshRenderer.material = Player04Material;
                }
                Normal_Color = Color.white;
                break;
            case PlayerEnum.Player05:
                break;
            case PlayerEnum.Player06:
                break;
            case PlayerEnum.Player07:
                break;
            case PlayerEnum.Player08:
                break;
            case PlayerEnum.None:
                MeshFilter.mesh = NodeMesh;
                MeshRenderer.material = Player01Material;
                Normal_Color = Color.gray;
                break;
        }

        if (IsBaseNode)
        {
            Vector3 vec3 = (ConnectNode[0].transform.position - transform.position);
            vec3.y = 0;
            transform.rotation = Quaternion.LookRotation(vec3, Vector3.up);
        }

        if (GameManager != null && GameManager.SelectNode != null)
        {
            if (GameManager.SelectNode == this || GameManager.SelectNode.ConnectNode.Contains(this)) { Normal_Color *= 1.5f; }
        }
        if (!IsBaseNode && Tower != null) { Destroy(Tower.gameObject); }

        MeshRenderer.material.color = Normal_Color;
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
        node.MeshRenderer.material.color = node.Normal_Color;
        node.MeshRenderer.material.color += node.Normal_Color / 4f;

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