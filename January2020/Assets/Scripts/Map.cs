using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 649警告を無視する
#pragma warning disable 649

public class Map : MonoBehaviour
{
    [SerializeField] GameObject Nodes;
    [SerializeField] GameObject Roads;
    [SerializeField] Node Node;
    [SerializeField] Road Road;

    List<Node> MapNode = new List<Node>();  // 全ノードを格納するリスト
    List<Road> MapRoad = new List<Road>();  // 全部の道を格納するリスト
    Vector3 GenerateSize = new Vector3(50, 0, 30);   // 生成範囲

    Node[] PlayerBaseNode = new Node[3];   // 各プレイヤーの本拠地

    static readonly int CreateNodeCount = 70;

    void Start()
    {
        // ノードを作成
        int count = 0;
        while (MapNode.Count < CreateNodeCount)
        {
            CreateNodes();
            CreateRoads();
            RemoveIsolation();

            count++;
            if (count > 1000)
            {
                Debug.Log("[ノードを作成の全工程]を1000回試行したため中断");
                break;
            }
        }

        // MapNodeの要素のPlayerEnumをNoneで初期化
        MapNode.ForEach(n => n.PlayerEnum = PlayerEnum.None);

        // ノードを割り当て
        SetPlayerNode();
    }

    void Update()
    {

    }

    // ノードを作成
    void CreateNodes()
    {
        // ノードを作成
        while (MapNode.Count < CreateNodeCount)
        {
            // 範囲内でランダムに生成
            Node instance = Instantiate(Node);
            instance.transform.position = MyMath.RandomGenerateSize(GenerateSize);

            // すでに追加されているノードと比べてどれか近かったら追加しない
            bool flag = false;
            foreach (var node in MapNode)
            {
                if (Vector3.SqrMagnitude(MyMath.ConversionVector2(instance.transform.position) - MyMath.ConversionVector2(node.transform.position)) <= (3 * 3))
                {
                    Destroy(instance.gameObject);
                    flag = true;
                    break;
                }
            }
            if (flag) { continue; }

            // 他ノードと近くなければ追加
            MapNode.Add(instance);
            instance.transform.SetParent(Nodes.transform);
        }
    }

    // ノードをつなげる道を作成
    void CreateRoads()
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
                if (Vector3.SqrMagnitude(distance) >= (13 * 13)) { continue; }
                // どちらかの道の数が一定以上ならこれ以上は繋げない
                if (MapNode[j].ConnectNode.Count >= 4 || MapNode[i].ConnectNode.Count >= 4) { continue; }

                // 橋を生成
                Road road = Instantiate(Road).GetComponent<Road>();
                road.transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
                road.transform.position = (MapNode[i].transform.position) + (road.transform.forward * distance.magnitude / 2f);
                road.transform.localScale = new Vector3(0.5f, 0.1f, distance.magnitude);
                road.transform.SetParent(Roads.transform);
                road.StartNode = MapNode[i];
                road.EndNode = MapNode[j];
                road.PosS = MyMath.ConversionVector2(MapNode[i].transform.position);
                road.PosE = MyMath.ConversionVector2(MapNode[j].transform.position);

                // すでにある道と交差していたら、スキップ
                bool flag = false;
                foreach (var r in MapRoad)
                {
                    if (MyMath.JudgeIentersected(road.PosS, road.PosE, r.PosS, r.PosE))
                    {
                        Destroy(road.gameObject);
                        flag = true;
                        continue;
                    }
                }
                if (flag) continue;

                // 道が自分かつなげる先以外のノードと当たっていたらスキップ
                //foreach (var n in MapNode)
                //{
                //    if (road.OnCollisionStay(n.GetComponent<Collision>()))
                //    {
                //        road.GetComponent<Renderer>().material.color = Color.red;
                //    }
                //}

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
    }

    // 孤立を削除
    void RemoveIsolation()
    {
        List<Node> OpenList = new List<Node>();

        // 最初に追加されたノードを親とする
        OpenList.Add(MapNode[0]);
        // 親から繋がっているノードを次々にリストに追加していく
        RemoveIsolation_Method(ref OpenList, MapNode[0]);

        // リストに入っていないのであれば孤立しているとみなす
        MapNode.ForEach(m =>
        {
            if (!OpenList.Contains(m))
            {
                // 消すノードの繋がっている道の削除フラグを立てる
                m.ConnectRoad.ForEach(r => { r.RemoveFlag = true; });
                Destroy(m.gameObject);
            }
        });
        MapNode.RemoveAll(m => !OpenList.Contains(m));

        // 孤立した道を削除
        MapRoad.ForEach(m => { if (m.RemoveFlag) { Destroy(m.gameObject); } });
        MapRoad.RemoveAll(m => m.RemoveFlag);
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
    void SetPlayerNode()
    {
        // 各プレイヤーランダムに拠点を決める
        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                Node baseNode = MapNode[Random.Range(0, MapNode.Count)];

                // すでにそこ誰かの本拠地じゃないなら
                if (!baseNode.IsBaseNode)
                {
                    // プレイヤーの本拠地に設定する
                    baseNode.PlayerEnum = (PlayerEnum)i;
                    baseNode.IsBaseNode = true;
                    PlayerBaseNode[i] = baseNode;

                    break;
                }
            }
        }

        List<Node>[] openNode = new List<Node>[3];

        for (int i = 0; i < 3; i++)
        {
            openNode[i] = new List<Node>();
            openNode[i].Add(PlayerBaseNode[i]);
        }

        int count = 0;
        int num = 0;
        while (MapNode.Any(n => n.PlayerEnum == PlayerEnum.None))
        {
            // 無限ループ回避用
            count++;
            if (count > 10000)
            {
                Debug.Log("無限ループに陥ったため処理を停止");
                break;
            }

            // もしプレイヤーがどこにも開けるノードがないなら、次のプレイヤーに見送ってスキップ
            if (!openNode[num].Any(l => l.ConnectNode.Any(n => n.PlayerEnum == PlayerEnum.None)))
            {
                num++;
                if (num >= 3) { num = 0; }
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
            if (num >= 3) { num = 0; }

            count = 0;
        }

        // もしどれかのプレイヤーのノード数が条件以下の数だったら
        // もう一度本拠地を決めるところから処理をやり直す
        for (int i = 0; i < 3; i++)
        {
            // もしプレイヤーのノードの数が一定以下なら
            Debug.Log(((PlayerEnum)i).ToString() + ":" + MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count());
            if (MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count() <= 10)
            {
                // 所属を初期化
                MapNode.ForEach(n => n.PlayerEnum = PlayerEnum.None);
                for (int j = 0; j < 3; j++)
                {
                    PlayerBaseNode[j].IsBaseNode = false;
                    PlayerBaseNode[j] = null;
                    Debug.Log("生成をやり直す");
                }

                // もう一度関数を呼び出す
                SetPlayerNode();

                break;
            }
        }

        MapNode.ForEach(n => n.UpdateNodeColor());
    }
}
