using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 649警告を無視する
#pragma warning disable 649

public class MapManager : MonoBehaviour
{
    [SerializeField] GameObject Nodes;
    [SerializeField] GameObject Roads;
    [SerializeField] Node Node;
    [SerializeField] Road Road;
    [SerializeField] Button StartButton;
    [SerializeField] Button GenerateButton;

    // prefab
    [SerializeField] GameObject CommanderPrefab;
    [SerializeField] GameObject SoldierPrefab;

    public List<Node> MapNode { get; private set; } = new List<Node>();  // 全ノードを格納するリスト
    public List<Road> MapRoad { get; private set; } = new List<Road>();  // 全部の道を格納するリスト

    Vector3 GenerateSize = new Vector3(80, 0, 40);   // 生成範囲
    public static readonly int PlayerCount = 8;
    static readonly int CreateNodeCount = 80;
    static readonly int MinPlayerNodeCount = 5;
    static readonly int RemoveLineDistance = 5;

    public Node[] PlayerBaseNode { get; private set; } = new Node[PlayerCount];   // 各プレイヤーの本拠地
    float[] time = new float[PlayerCount];
    float[] limit = new float[PlayerCount];

    public GameManager GameManager { get; private set; }

    // 生成が終わったか

    void Start()
    {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        // マップを生成
        GenerateMap();
    }

    void Update()
    {
        GenerateCharacters();
    }

    // マップを生成
    public void GenerateMap()
    {
        StartButton.interactable = false;
        GenerateButton.interactable = false;
        StartCoroutine("Initialize");
    }

    IEnumerator Initialize()
    {
        foreach (var road in MapRoad)
        {
            Destroy(road.gameObject);
            yield return null;
        }
        MapRoad.Clear();
        foreach (var node in MapNode)
        {
            Destroy(node.gameObject);
            yield return null;
        }
        MapNode.Clear();

        StartCoroutine("CreateNodes");
        yield break;
    }

    // ノードを作成
    IEnumerator CreateNodes()
    {
        // ノードを作成
        while (MapNode.Count < CreateNodeCount)
        {
            Node instance = Instantiate(Node);
            instance.transform.position = (MapNode.Count <= 0) ? (Vector3.zero) : (MyMath.RandomGenerateSize(GenerateSize));
            float scale = Random.Range(4f, 8f);
            instance.transform.localScale = (new Vector3(1, 0, 1) * scale) + (Vector3.up * instance.transform.localScale.y);
            instance.Scale = scale;

            yield return null;

            // すでに追加されているノードと比べてどれか近かったら追加しない
            bool flag = false;
            foreach (var node in MapNode)
            {
                if (Vector3.SqrMagnitude(MyMath.ConversionVector2(instance.transform.position) - MyMath.ConversionVector2(node.transform.position)) <= ((node.Scale + 2) * (node.Scale + 2)) ||
                    MapRoad.Any(r => MyMath.IsLineIntersectedCircle(r.PosS, r.PosE, MyMath.ConversionVector2(instance.transform.position), RemoveLineDistance)))
                {
                    Destroy(instance.gameObject);
                    flag = true;
                    break;
                }
            }
            if (flag) { continue; }

            yield return null;

            // 他ノードと近くなければ追加
            MapNode.Add(instance);
            instance.transform.SetParent(Nodes.transform);
            instance.Map = this;
        }

        StartCoroutine("CreateRoads");
        yield break;
    }

    // ノードをつなげる道を作成
    IEnumerator CreateRoads()
    {
        // 橋を作成
        for (int i = 0; i < MapNode.Count; i++)
        {
            for (int j = 0; j < MapNode.Count; j++)
            {
                // 重複はスキップ
                if (i >= j) continue;

                Vector3 distance = MapNode[j].transform.position - MapNode[i].transform.position;
                // 距離が離れすぎているものは橋を繋げない
                if (Vector3.SqrMagnitude(distance) >= (15 * 15)) { continue; }
                // どちらかの道の数が一定以上ならこれ以上は繋げない
                if (MapNode[j].ConnectNode.Count >= 4 || MapNode[i].ConnectNode.Count >= 4) { continue; }

                // 橋を生成
                Road road = Instantiate(Road).GetComponent<Road>();
                road.transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
                road.transform.position = (MapNode[i].transform.position) + (road.transform.forward * distance.magnitude / 2f);
                road.transform.localScale = new Vector3(road.transform.localScale.x, 0.1f, distance.magnitude);
                road.transform.SetParent(Roads.transform);
                road.StartNode = MapNode[i];
                road.EndNode = MapNode[j];
                road.PosS = MyMath.ConversionVector2(MapNode[i].transform.position);
                road.PosE = MyMath.ConversionVector2(MapNode[j].transform.position);

                yield return null;

                // すでにある道と交差していたら、スキップ
                bool flag1 = false;
                foreach (var r in MapRoad)
                {
                    if (MyMath.JudgeIentersected(road.PosS, road.PosE, r.PosS, r.PosE))
                    {
                        Destroy(road.gameObject);
                        flag1 = true;
                        continue;
                    }
                }
                if (flag1) continue;

                yield return null;

                // 始点ノードと終点ノードとは違う他ノードと線が衝突していたら、削除
                bool flag2 = false;
                foreach (var node in MapNode)
                {
                    if (node == road.StartNode || node == road.EndNode) continue;

                    bool flag = MyMath.IsLineIntersectedCircle(road.PosS, road.PosE, MyMath.ConversionVector2(node.transform.position), RemoveLineDistance);
                    if (!flag) continue;

                    road.GetComponent<Renderer>().material.color = Color.red;
                    flag2 = true;
                    Destroy(road.gameObject);
                    break;
                }
                if (flag2) continue;

                yield return null;

                // 重なっていないなら追加
                MapRoad.Add(road);

                // つながっているノードの参照をお互いに渡す
                MapNode[i].ConnectNode.Add(MapNode[j]);
                MapNode[j].ConnectNode.Add(MapNode[i]);

                // 繋がっている道の参照をお互いに渡す
                MapNode[i].ConnectRoad.Add(road);
                MapNode[j].ConnectRoad.Add(road);
            }
        }

        StartCoroutine("RemoveIsolation");
        yield break;
    }

    // 孤立を削除
    IEnumerator RemoveIsolation()
    {
        List<Node> OpenList = new List<Node>();

        // 最初に追加されたノードを親とする
        OpenList.Add(MapNode[0]);
        // 親から繋がっているノードを次々にリストに追加していく
        RemoveIsolation_Method(ref OpenList, MapNode[0]);

        // リストに入っていないのであれば孤立しているとみなす
        foreach (var node in MapNode)
        {
            if (!OpenList.Contains(node))
            {
                // 消すノードの繋がっている道の削除フラグを立てる
                node.ConnectRoad.ForEach(r => { r.RemoveFlag = true; });
                Destroy(node.gameObject);

                yield return null;
            }
        }
        MapNode.RemoveAll(m => !OpenList.Contains(m));

        // 孤立した道を削除
        MapRoad.ForEach(m => { if (m.RemoveFlag) { Destroy(m.gameObject); } });
        MapRoad.RemoveAll(m => m.RemoveFlag);

        // もう一度処理を行う
        if (MapNode.Count < CreateNodeCount)
        {
            StartCoroutine("CreateNodes");
        }
        // 次の処理へ
        else
        {
            StartCoroutine("SetPlayerNode");
        }
        yield break;
    }

    // 孤立を削除する際の再帰処理
    void RemoveIsolation_Method(ref List<Node> OpenList, Node node)
    {
        // 繋がっているノードを次々にリストに入れていく
        foreach (var connectNode in node.ConnectNode)
        {
            // すでに入っているならスキップ
            if (OpenList.Contains(connectNode)) { continue; }
            // 追加
            OpenList.Add(connectNode);
            // 繋がっているノードからさらに繋がっているノードをリストに入れていく
            RemoveIsolation_Method(ref OpenList, connectNode);
        }
    }

    // ノードに国を割り当てていく
    IEnumerator SetPlayerNode()
    {
        // MapNodeの要素のPlayerEnumをNoneで初期化
        foreach (var node in MapNode)
        {
            node.PlayerEnum = PlayerEnum.None;
            node.UpdateNodeColor();
            yield return null;
        }

        while (true)
        {
            // 各プレイヤーランダムに拠点を決める
            for (int i = 0; i < PlayerCount; i++)
            {
                while (true)
                {
                    Node baseNode = MapNode[Random.Range(0, MapNode.Count)];

                    // すでにそこ誰かの本拠地ならスキップ
                    if (baseNode.IsBaseNode) continue;
                    // 隣に誰かの本拠地があるならスキップ
                    //bool flag = false;
                    //PlayerBaseNode.ToList().ForEach(b => { if (baseNode.ConnectNode.Contains(b)) { flag = true; } });
                    //if (flag) continue;

                    List<Node> openList = new List<Node> { baseNode };
                    int Num = 0;
                    Hoge(ref openList, baseNode, Num);
                    void Hoge(ref List<Node> list, Node node, int numm)
                    {
                        foreach (var connect in node.ConnectNode)
                        {
                            if (list.Contains(connect)) continue;
                            openList.Add(connect);
                            numm++;
                            if (numm >= 2) { continue; }
                            Hoge(ref list, connect, numm);
                        }
                    }

                    bool flagg = false;
                    PlayerBaseNode.ToList().ForEach(b => { if (openList.Contains(b)) { flagg = true; } });
                    if (flagg) continue;

                    // プレイヤーの本拠地に設定する
                    baseNode.PlayerEnum = (PlayerEnum)i;
                    baseNode.IsBaseNode = true;
                    PlayerBaseNode[i] = baseNode;
                    baseNode.UpdateNodeColor();
                    yield return null;

                    break;
                }
            }

            List<Node>[] openNode = new List<Node>[PlayerCount];

            for (int i = 0; i < PlayerCount; i++)
            {
                openNode[i] = new List<Node>();
                openNode[i].Add(PlayerBaseNode[i]);
            }

            int num = 0;
            while (MapNode.Any(n => n.PlayerEnum == PlayerEnum.None))
            {
                // もしプレイヤーがどこにも開けるノードがないなら、次のプレイヤーに見送ってスキップ
                if (!openNode[num].Any(l => l.ConnectNode.Any(n => n.PlayerEnum == PlayerEnum.None)))
                {
                    num++;
                    if (num >= PlayerCount) { num = 0; }
                    continue;
                }

                // プレイヤーの開けるリストから、ランダムに一つ選ぶ
                Node parentNode = openNode[num][Random.Range(0, openNode[num].Count)];
                // もし接続先がないならスキップ
                if (parentNode.ConnectNode.Count <= 0) continue;

                // もし接続先に開けるノードがないなら、現在のノードをリストから外してスキップする
                if (!parentNode.ConnectNode.Any(n => n.PlayerEnum == PlayerEnum.None))
                {
                    openNode[num].Remove(parentNode);
                    continue;
                }

                // 接続先の中からランダムで一つ選ぶ
                Node newNode = parentNode.ConnectNode[Random.Range(0, parentNode.ConnectNode.Count)];
                if (newNode.PlayerEnum != PlayerEnum.None) continue;    // もしすでに誰かの領土ならスキップ

                // 領土を占有
                newNode.PlayerEnum = (PlayerEnum)num;
                newNode.UpdateNodeColor();

                // リストに新しく開く場所を追加
                openNode[num].Add(newNode);

                // 接続先が全部開けなくなったらリストから削除
                openNode[num].RemoveAll(n => !n.ConnectNode.Any(c => c.PlayerEnum == PlayerEnum.None));

                // 次のプレイヤーに
                num++;
                if (num >= PlayerCount) { num = 0; }
            }

            yield return null;

            // もしどれかのプレイヤーのノード数が条件以下の数だったら
            // または、だれかの本拠地の隣がすぐ敵陣地だったら、やり直す
            // もう一度本拠地を決めるところから処理をやり直す
            bool flag = false;
            for (int i = 0; i < PlayerCount; i++)
            {
                // もしプレイヤーのノードの数が一定以下なら
                if (MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count() <= MinPlayerNodeCount ||
                    PlayerBaseNode[i].ConnectNode.Any(c => PlayerBaseNode[i].PlayerEnum != c.PlayerEnum))
                {
                    // 所属を初期化
                    flag = true;
                    MapNode.ForEach(n => n.PlayerEnum = PlayerEnum.None);
                    for (int j = 0; j < PlayerCount; j++)
                    {
                        PlayerBaseNode[j].IsBaseNode = false;
                        PlayerBaseNode[j] = null;
                    }
                    break;
                }
            }
            if (!flag) { break; }
        }

        MapNode.ForEach(n => n.UpdateNodeColor());

        StartButton.interactable = true;
        GenerateButton.interactable = true;

        yield break;
    }

    void GenerateCharacters()
    {
        // まだゲームが開始状態じゃなかったらリターン
        if (!GameManager.IsStart) return;

        for (int i = 0; i < PlayerCount; i++)
        {
            time[i] += Time.deltaTime;

            float countMax = 50;
            float countMin = 5;
            float count = MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count();
            count = Mathf.Clamp(count, countMin, countMax);

            float limitMax = 0.6f;
            float limitMin = 0.3f;
            // (count-5) / (50-5) => [ 5/50 => 0/45 ] [ 50/50 => 45/45 ]
            float rate = (count - countMin) / (countMax - countMin);
            limit[i] = Mathf.Lerp(limitMax, limitMin, rate);

            if (time[i] >= limit[i])
            {
                time[i] = 0;
                CreateChara((PlayerEnum)i);
            }
        }
    }

    void CreateChara(PlayerEnum playerEnum)
    {
        if (PlayerBaseNode[(int)playerEnum] == null) return;

        // 指揮官を生成
        // 自分の領土かつ、その領土が指揮官５以下なら
        List<Node> list1 = MapNode.Where(n =>
        {
            // 自分の所属ノードかつ、そのノードのキャラのカウントが一定以下なら
            return (n.PlayerEnum == playerEnum && n.CommanderCount < 5 && n.IsConnectMainBase);
        }).ToList();

        if (list1.Count > 0)
        {
            Commander commander = Instantiate(CommanderPrefab).GetComponent<Commander>();
            Node node1 = list1[Random.Range(0, list1.Count)];
            node1.Commander.Add(commander);
            commander.UpdateNode(node1);
        }

        // 兵士を生成
        // 本拠地に一体
        for (int i = 0; i < 4; i++)
        {
            if (PlayerBaseNode[(int)playerEnum].SoldierCount < 5)
            {
                Soldier soldier1 = Instantiate(SoldierPrefab).GetComponent<Soldier>();
                PlayerBaseNode[(int)playerEnum].Soldier.Add(soldier1);
                soldier1.UpdateNode(PlayerBaseNode[(int)playerEnum]);
            }
        }
        // 自分の領土ランダムに一体
        List<Node> list2 = MapNode.Where(n =>
        {
            // 自分の所属ノードかつ、そのノードのキャラのカウントが一定以下なら
            return (n.PlayerEnum == playerEnum && n.SoldierCount < 5 && n.IsConnectMainBase);
        }).ToList();

        if (list2.Count > 0)
        {
            Soldier soldier2 = Instantiate(SoldierPrefab).GetComponent<Soldier>();
            Node node2 = list2[Random.Range(0, list2.Count)];
            node2.Soldier.Add(soldier2);
            soldier2.UpdateNode(node2);
        }
    }

    public void AllUpdateNodeColor()
    {
        MapNode.ForEach(n => n.UpdateNodeColor());
    }
}
