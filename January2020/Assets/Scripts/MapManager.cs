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

    [SerializeField] Image GameStartImage;
    [SerializeField] Text GameStartText;
    [SerializeField] GraphManager GraphManager;
    [SerializeField] GameObject Timer;
    [SerializeField] GameObject BattleCanvas;
    [SerializeField] GameObject MinimapCamera;
    [SerializeField] GameObject OperationExplanation;

    // prefab
    [SerializeField] GameObject CommanderPrefab;
    [SerializeField] GameObject SoldierPrefab;
    [SerializeField] Animator GameStartAnimation;

    public List<Node> MapNode { get; private set; } = new List<Node>();  // 全ノードを格納するリスト
    public List<Road> MapRoad { get; private set; } = new List<Road>();  // 全部の道を格納するリスト

    static float GenerateSize = 70;
    public static readonly int PlayerCount = 4;
    static readonly int CreateNodeCount = 75;
    static readonly int MinPlayerNodeCount = 12;
    static readonly int RemoveLineDistance = 5;

    public List<Node> PlayerBaseNode { get; private set; } = new List<Node>();  // 各プレイヤーの本拠地
    float[] cGeneTime = new float[PlayerCount];
    float[] cGeneLimit = new float[PlayerCount];
    float[] sGeneTime = new float[PlayerCount];
    float[] sGeneLimit = new float[PlayerCount];

    public GameManager GameManager { get; private set; }

    // 生成が終わったか

    void Start()
    {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        GameStartImage.gameObject.SetActive(false);
        GameStartText.gameObject.SetActive(false);
        MinimapCamera.SetActive(false);
        OperationExplanation.SetActive(false);

        // マップを生成
        StartCoroutine("Initialize");
    }

    void Update()
    {
        GenerateCharacters();
    }

    IEnumerator Initialize()
    {
        int count = 0;
        foreach (var road in MapRoad)
        {
            count++;
            if (count >= 3) { count = 0; yield return null; }
            Destroy(road.GetComponent<MeshRenderer>().material);
            Destroy(road.gameObject);
        }
        MapRoad.Clear();

        count = 0;
        foreach (var node in MapNode)
        {
            count++;
            if (count >= 3) { count = 0; yield return null; }
            Destroy(node.MeshRenderer.material);
            Destroy(node.gameObject);
        }
        MapNode.Clear();
        PlayerBaseNode.Clear();

        // 本拠地を生成
        for (int i = 0; i < PlayerCount; i++)
        {
            Node instance = Instantiate(Node);

            float f = i / (float)PlayerCount;
            float rad = (Random.Range(-15f, 15f) + (360f * f) + 45f) * Mathf.Deg2Rad;
            Vector3 vec3 = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            vec3 *= Random.Range(50f, 60f);

            instance.transform.position = vec3;

            float scale = Random.Range(14f, 16f);
            instance.transform.localScale = (new Vector3(1, 1, 1) * scale * 0.3f);
            instance.Scale = scale;
            instance.GameManager = GameManager;

            PlayerBaseNode.Add(instance);
            MapNode.Add(instance);
            instance.transform.SetParent(Nodes.transform);
            instance.IsBaseNode = true;
            instance.Map = this;
        }

        PlayerBaseNode = PlayerBaseNode.OrderBy(a => System.Guid.NewGuid()).ToList();

        StartCoroutine("CreateNodes");
        yield break;
    }

    // ノードを作成
    IEnumerator CreateNodes()
    {
        int count = 0;

        // もうすでに一定の数生成されてたら
        if (MapNode.Count >= CreateNodeCount)
        {
            StartCoroutine("Initialize");
            yield break;
        }

        // ノードを作成
        while (MapNode.Count < CreateNodeCount)
        {
            Node instance = Instantiate(Node);
            //instance.transform.position = (MapNode.Count <= 0) ? (Vector3.zero) : (MyMath.RandomGenerateSize(GenerateSize));
            instance.transform.position = (MapNode.Count <= PlayerCount) ? (Vector3.zero) : (MyMath.CircleRandom(GenerateSize));
            int[] array = new int[] { 0, 0, 0, 0, 0, 0, 0, 10, 20, 50, 10, 10 };
            float scale = MyMath.GetRandomIndex(array) + Random.Range(0f, 1f);
            instance.transform.localScale = (new Vector3(1, 1, 1) * scale * 0.3f);
            instance.transform.Rotate(0, Random.Range(0f, 360f), 0);
            instance.Scale = scale;
            instance.GameManager = GameManager;

            count++;
            if (count >= 4) { count = 0; yield return null; }

            // すでに追加されているノードと比べてどれか近かったら追加しない
            bool flag = false;
            foreach (var node in MapNode)
            {
                if (Vector3.Magnitude(MyMath.ConversionVector2(instance.transform.position) - MyMath.ConversionVector2(node.transform.position)) <= ((instance.Scale + node.Scale + 4) / 2f) ||
                    MapRoad.Any(r => MyMath.IsLineIntersectedCircle(r.PosS, r.PosE, MyMath.ConversionVector2(instance.transform.position), RemoveLineDistance)))
                {
                    Destroy(instance.GetComponent<MeshRenderer>().material);
                    Destroy(instance.gameObject);
                    flag = true;
                    break;
                }
            }
            if (flag) { continue; }

            // 他ノードと近くなければ追加
            if (MapNode.All(n => PlayerBaseNode.Contains(n)))
                MapNode.Insert(0, instance);
            else
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
        int count = 0;

        // 橋を作成
        for (int i = 0; i < MapNode.Count; i++)
        {
            for (int j = 0; j < MapNode.Count; j++)
            {
                // 重複はスキップ
                if (i >= j) continue;

                Vector3 distance = MapNode[j].transform.position - MapNode[i].transform.position;
                // 距離が離れすぎているものは橋を繋げない
                if (Vector3.SqrMagnitude(distance) >= (21 * 21)) { continue; }
                // どちらかの道の数が一定以上ならこれ以上は繋げない
                if (MapNode[j].ConnectNode.Count >= 4 || MapNode[i].ConnectNode.Count >= 4) { continue; }
                // すでにその場所に道をつなげていたらスキップ
                if (MapNode[i].ConnectNode.Any(n => n == MapNode[j]) || MapNode[j].ConnectNode.Any(n => n == MapNode[i])) continue;
                // 本拠地かつ、それがどれかと接続されていたら
                if ((MapNode[i].IsBaseNode && MapNode[i].ConnectRoad.Count() >= 1) || (MapNode[j].IsBaseNode && MapNode[j].ConnectRoad.Count() >= 1)) continue;
                // 本拠地と本拠地同士は繋げないので、スキップ
                if (MapNode[i].IsBaseNode && MapNode[j].IsBaseNode) continue;

                // 橋を生成
                Road road = Instantiate(Road).GetComponent<Road>();
                road.transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
                road.transform.position = (MapNode[i].transform.position + distance.normalized * (MapNode[i].Scale + 2) * 0.33f);
                Vector3 aite = (MapNode[j].transform.position - distance.normalized * (MapNode[j].Scale + 2) * 0.33f);
                road.transform.localScale = new Vector3(road.transform.localScale.x, 1f, (road.transform.position - aite).magnitude * 0.33f);
                road.transform.SetParent(Roads.transform);
                road.StartNode = MapNode[i];
                road.EndNode = MapNode[j];
                road.PosS = MyMath.ConversionVector2(MapNode[i].transform.position);
                road.PosE = MyMath.ConversionVector2(MapNode[j].transform.position);

                count++;
                if (count >= 1) { count = 0; yield return null; }

                // すでにある道と交差していたら、スキップ
                bool flag1 = false;
                foreach (var r in MapRoad)
                {
                    if (MyMath.JudgeIentersected(road.PosS, road.PosE, r.PosS, r.PosE))
                    {
                        Destroy(road.GetComponent<MeshRenderer>().material);
                        Destroy(road.gameObject);
                        flag1 = true;
                        continue;
                    }
                }
                if (flag1) continue;

                // 始点ノードと終点ノードとは違う他ノードと線が衝突していたら、削除
                bool flag2 = false;
                foreach (var node in MapNode)
                {
                    if (node == road.StartNode || node == road.EndNode) continue;

                    bool flag = MyMath.IsLineIntersectedCircle(road.PosS, road.PosE, MyMath.ConversionVector2(node.transform.position), RemoveLineDistance);
                    if (!flag) continue;

                    flag2 = true;
                    Destroy(road.GetComponent<MeshRenderer>().material);
                    Destroy(road.gameObject);
                    break;
                }
                if (flag2) continue;

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

                if (node.IsBaseNode)
                {
                    node.ConnectNode.Clear();
                    node.ConnectRoad.Clear();
                }
                // 消すノードの繋がっている道の削除フラグを立てる
                else
                {
                    node.ConnectRoad.ForEach(r => { r.RemoveFlag = true; });
                    Destroy(node.GetComponent<MeshRenderer>().material);
                    Destroy(node.gameObject);
                }

                yield return null;
            }
        }

        MapNode.RemoveAll(n => (!OpenList.Contains(n) && !n.IsBaseNode));

        // 孤立した道を削除
        MapRoad.ForEach(r => { if (r.RemoveFlag) { Destroy(r.GetComponent<MeshRenderer>().material); Destroy(r.gameObject); } });
        MapRoad.RemoveAll(m => m.RemoveFlag);

        // もう一度処理を行う
        if (PlayerBaseNode.Any(n => n.ConnectRoad.Count() <= 0) || MapNode.Count < CreateNodeCount)
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

        for (int i = 0; i < PlayerBaseNode.Count; i++)
        {
            PlayerBaseNode[i].PlayerEnum = (PlayerEnum)i;
            PlayerBaseNode[i].UpdateNodeColor();
        }

        //int count = 0;
        //int breakCount = 0;
        //while (true)
        //{
        //    List<Node>[] openNode = new List<Node>[PlayerCount];

        //    for (int i = 0; i < PlayerCount; i++)
        //    {
        //        openNode[i] = new List<Node>();
        //        openNode[i].Add(PlayerBaseNode[i]);
        //    }

        //    int num = 0;
        //    while (MapNode.Any(n => n.PlayerEnum == PlayerEnum.None))
        //    {
        //        // もしプレイヤーがどこにも開けるノードがないなら、次のプレイヤーに見送ってスキップ
        //        if (!openNode[num].Any(l => l.ConnectNode.Any(n => n.PlayerEnum == PlayerEnum.None)))
        //        {
        //            num++;
        //            if (num >= PlayerCount) { num = 0; }
        //            continue;
        //        }

        //        // プレイヤーの開けるリストから、ランダムに一つ選ぶ
        //        Node parentNode = openNode[num][Random.Range(0, openNode[num].Count)];
        //        // もし接続先がないならスキップ
        //        if (parentNode.ConnectNode.Count <= 0) continue;

        //        // もし接続先に開けるノードがないなら、現在のノードをリストから外してスキップする
        //        if (!parentNode.ConnectNode.Any(n => n.PlayerEnum == PlayerEnum.None))
        //        {
        //            openNode[num].Remove(parentNode);
        //            continue;
        //        }

        //        // 接続先の中からランダムで一つ選ぶ
        //        Node newNode = parentNode.ConnectNode[Random.Range(0, parentNode.ConnectNode.Count)];
        //        if (newNode.PlayerEnum != PlayerEnum.None) continue;    // もしすでに誰かの領土ならスキップ

        //        // 領土を占有
        //        newNode.PlayerEnum = (PlayerEnum)num;
        //        newNode.UpdateNodeColor();

        //        count++;
        //        if (count >= 1) { count = 0; yield return null; }

        //        // リストに新しく開く場所を追加
        //        openNode[num].Add(newNode);

        //        // 接続先が全部開けなくなったらリストから削除
        //        openNode[num].RemoveAll(n => !n.ConnectNode.Any(c => c.PlayerEnum == PlayerEnum.None));

        //        // 次のプレイヤーに
        //        num++;
        //        if (num >= PlayerCount) { num = 0; }
        //    }

        //    breakCount++;
        //    if (breakCount >= 4) { StartCoroutine("Initialize"); yield break; }

        //    yield return null;

        //    // もしどれかのプレイヤーのノード数が条件以下の数だったら
        //    // または、だれかの本拠地の隣がすぐ敵陣地だったら、やり直す
        //    // もう一度本拠地を決めるところから処理をやり直す
        //    bool flag = false;
        //    for (int i = 0; i < PlayerCount; i++)
        //    {
        //        // もしプレイヤーのノードの数が一定以下なら
        //        if (MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count() <= MinPlayerNodeCount ||
        //            PlayerBaseNode[i].ConnectNode.Any(c => PlayerBaseNode[i].PlayerEnum != c.PlayerEnum))
        //        {
        //            // 所属を初期化
        //            flag = true;
        //            MapNode.Where(n => !PlayerBaseNode.Contains(n)).ToList().ForEach(n => n.PlayerEnum = PlayerEnum.None);
        //            break;
        //        }
        //    }
        //    if (!flag) { break; }
        //}

        MapNode.ForEach(n => n.UpdateNodeColor());

        StartCoroutine("GameStart");

        yield break;
    }


    // ノードを初期化
    IEnumerator GameStart()
    {
        foreach (var node in MapNode)
        {
            node.Initialize();
            yield return null;
        }

        CameraController CameraController = Camera.main.GetComponent<CameraController>();

        float time = 0;
        float rotation = 0;
        Vector2 distance01 = new Vector2(35, 8);
        Vector2 distance02 = new Vector2(80, 20);
        Vector3 pos = Vector3.zero;

        GameStartImage.gameObject.SetActive(true);
        GameStartText.gameObject.SetActive(true);

        float height = GameStartImage.rectTransform.sizeDelta.y;
        GameStartImage.rectTransform.sizeDelta = new Vector2(GameStartImage.rectTransform.sizeDelta.x, 0);
        GameStartText.color *= new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 0);

        while (true)
        {
            time += Time.deltaTime;

            if (time >= 15f && time <= 35)
            {
                if (time <= 30)
                    rotation += 0.5f * Time.deltaTime * 60;
                else
                    GameStartImage.rectTransform.sizeDelta =
                    Vector2.Lerp(GameStartImage.rectTransform.sizeDelta, new Vector2(GameStartImage.rectTransform.sizeDelta.x, 0), 0.05f * Time.deltaTime * 60);

                if (time <= 25)
                {
                    if (time <= 23)
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 1), 0.025f * Time.deltaTime * 60);
                    else
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 0), 0.05f * Time.deltaTime * 60);

                    GameStartText.text = "敵とバトルして、敵よりも多くの陣地を獲得しよう";
                }
                else
                {
                    if (time <= 29.5f)
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 1), 0.025f * Time.deltaTime * 60);
                    else
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 0), 0.05f * Time.deltaTime * 60);

                    GameStartText.text = "ゲーム終了時に一番多くの拠点を持つプレイヤーが勝利します";
                }

                pos = Vector3.zero;
                CameraController.transform.position =
                Vector3.Lerp(
                    CameraController.transform.position,
                    (pos + (MyMath.RadToVec3(rotation * Mathf.Deg2Rad) * distance02.x)) + (Vector3.up * distance02.y),
                    0.005f * Time.deltaTime * 60);
                CameraController.transform.rotation = Quaternion.Lerp(CameraController.transform.rotation, Quaternion.LookRotation(pos - CameraController.transform.position, Vector3.up), 0.025f * Time.deltaTime * 60);
            }
            else if (time <= 15f)
            {
                GameStartImage.rectTransform.sizeDelta =
                    Vector2.Lerp(GameStartImage.rectTransform.sizeDelta, new Vector2(GameStartImage.rectTransform.sizeDelta.x, height), 0.05f * Time.deltaTime * 60);
                if (time <= 10)
                    rotation += 0.5f * Time.deltaTime * 60;


                if (time <= 7)
                {
                    if (time <= 6)
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 1), 0.025f * Time.deltaTime * 60);
                    else
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 0), 0.025f * Time.deltaTime * 60);

                    GameStartText.text = "陣取りゲーム";
                }
                else
                {
                    if (time <= 14)
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 1), 0.025f * Time.deltaTime * 60);
                    else
                        GameStartText.color = Color.Lerp(GameStartText.color, new Color(GameStartText.color.r, GameStartText.color.g, GameStartText.color.b, 0), 0.025f * Time.deltaTime * 60);

                    GameStartText.text = "拠点から生成される兵士を移動させて陣地を広げられる";
                }

                pos = PlayerBaseNode[(int)PlayerEnum.Player01].transform.position;
                CameraController.transform.position =
                Vector3.Lerp(
                    CameraController.transform.position,
                    (pos + (MyMath.RadToVec3(rotation * Mathf.Deg2Rad) * distance01.x)) + (Vector3.up * distance01.y),
                    0.005f * Time.deltaTime * 60);
                CameraController.transform.rotation = Quaternion.Lerp(CameraController.transform.rotation, Quaternion.LookRotation(pos - CameraController.transform.position, Vector3.up), 0.025f * Time.deltaTime * 60);
            }
            else
            {
                CameraController.IsControll = true;
                CameraController.DestPosition = PlayerBaseNode[(int)PlayerEnum.Player01].transform.position;
                GraphManager.gameObject.SetActive(true);
                Timer.SetActive(true);
                MinimapCamera.SetActive(true);
                OperationExplanation.SetActive(true);
                break;
            }

            yield return null;
        }

        Animator animator = Instantiate(GameStartAnimation).GetComponent<Animator>();
        animator.transform.SetParent(BattleCanvas.transform);
        animator.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

        while (true)
        {
            if (animator == null)
            {
                GameManager.IsStart = true;
                break;
            }

            yield return null;
        }
    }

    void GenerateCharacters()
    {
        // まだゲームが開始状態じゃなかったらリターン
        if (!GameManager.IsStart) return;
        // ゲームが終了していたらリターン
        if (GameManager.IsEnd) { return; }


        for (int i = 0; i < PlayerCount; i++)
        {
            cGeneTime[i] += MyTime.deltaTime;
            sGeneTime[i] += MyTime.deltaTime;

            float countMax = 50;
            float countMin = 5;
            float count = MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i).Count();
            count = Mathf.Clamp(count, countMin, countMax);

            // 指揮官生成
            float cGeneLimitMax = 5f;
            float cGeneLimitMin = 3f;
            //float cGeneLimitMax = 1f;
            //float cGeneLimitMin = 0.5f;
            // (count-5) / (50-5) => [ 5/50 => 0/45 ] [ 50/50 => 45/45 ]
            float cGeneRate = (count - countMin) / (countMax - countMin);
            cGeneLimit[i] = Mathf.Lerp(cGeneLimitMax, cGeneLimitMin, cGeneRate);

            if (cGeneTime[i] >= cGeneLimit[i])
            {
                cGeneTime[i] = 0;
                CreateCommander((PlayerEnum)i);
            }

            float sGeneLimitMax = 2.5f;
            float sGeneLimitMin = 0.75f;
            //float sGeneLimitMax = 1f;
            //float sGeneLimitMin = 0.5f;
            // (count-5) / (50-5) => [ 5/50 => 0/45 ] [ 50/50 => 45/45 ]
            float sGeneRate = (count - countMin) / (countMax - countMin);
            sGeneLimit[i] = Mathf.Lerp(sGeneLimitMax, sGeneLimitMin, sGeneRate);
            if (sGeneTime[i] >= sGeneLimit[i])
            {
                sGeneTime[i] = 0;
                CreateSoldier((PlayerEnum)i);
            }
        }
    }

    void CreateCommander(PlayerEnum playerEnum)
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
            Node node = list1[Random.Range(0, list1.Count)];
            node.Commander.Add(commander);
            commander.UpdateNode(node);
            node.ResetPosCharacter();
            commander.transform.position = commander.DestPosition;
        }
    }

    void CreateSoldier(PlayerEnum playerEnum)
    {
        if (PlayerBaseNode[(int)playerEnum] == null) return;

        // 兵士を生成
        // 本拠地に
        for (int i = 0; i < 1; i++)
        {
            if (PlayerBaseNode[(int)playerEnum].SoldierCount < 5)
            {
                Soldier soldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
                PlayerBaseNode[(int)playerEnum].Soldier.Add(soldier);
                soldier.UpdateNode(PlayerBaseNode[(int)playerEnum]);
                PlayerBaseNode[(int)playerEnum].ResetPosCharacter();
                soldier.transform.position = soldier.DestPosition;
            }
        }
    }

    public void AllUpdateNodeColor()
    {
        MapNode.ForEach(n => n.UpdateNodeColor());
    }
}
