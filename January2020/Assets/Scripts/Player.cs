using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// クリック操作をする操作する側のクラス
public class Player : MonoBehaviour
{
    [SerializeField] GameObject MoveBoxes;
    Node selectNode;    // 現在選択しているノード
    [SerializeField] Map map;

    void Start()
    {
        // 選択しているノードをnull
        selectNode = null;
    }

    void Update()
    {
        if (selectNode != null)
            Debug.Log(selectNode.Soldier.Count);

        LeftClick();
        RightClick();
        AttackAI();
        MoveAI();
        Quit();
    }

    void Move(Node node1, Node node2)
    {
        if (!node2.MovingNode.Contains(node1))
        {
            MoveBox movebox = CreateMovebox(node1, node2);

            // 兵士がいないなら早期リターン、または開いてノードの兵士が一定以上なら移動できない
            if (node1.Soldier.Count <= 0)
            {
                Destroy(movebox.gameObject);
                return;
            }

            node1.MovingNode.Add(node2);

            // [移動BOXに兵士を移動]
            //movebox.Soldier.AddRange(node1.Soldier.Take(node2.Soldier.Count - 5));
            movebox.Soldier.AddRange(node1.Soldier);
            node1.Soldier.Clear();

            // 兵士を自分の子オブジェクトに格納する
            movebox.Soldier.ForEach(s => s.transform.SetParent(movebox.transform));

            // 親をNULLに更新
            movebox.Soldier.ForEach(s => s.UpdateNode(null));
        }
    }

    void Attack(Node node1, Node node2)
    {
        if (!node2.MovingNode.Contains(node1))
        {
            MoveBox movebox = CreateMovebox(node1, node2);

            // 指揮官が二人未満なら、または兵士がいないなら早期リターン
            if (node1.Commander.Count < 2 || node1.Soldier.Count <= 0)
            {
                Destroy(movebox.gameObject);
                return;
            }

            node1.MovingNode.Add(node2);

            // [移動BOXに指揮官を移動]
            // インデックス最初のやつを保持
            Commander firstCommander = node1.Commander[0];
            // インデックスの最初のやつをリストから一時的に外す
            node1.Commander.RemoveAt(0);
            // 相手の指揮官を消して自分の兵士を指揮官させる
            movebox.Commander.AddRange(node1.Commander);
            node1.Commander.Clear();
            // 一旦削除しておいた最初の指揮官を追加しなおす
            node1.Commander.Add(firstCommander);

            // [移動BOXに兵士を移動]
            movebox.Soldier.AddRange(node1.Soldier);
            node1.Soldier.Clear();

            // 指揮官と兵士を自分の子オブジェクトに格納する
            movebox.Commander.ForEach(c => c.transform.SetParent(movebox.transform));
            movebox.Soldier.ForEach(s => s.transform.SetParent(movebox.transform));

            // 親をNULLに更新
            movebox.Commander.ForEach(s => s.UpdateNode(null));
            movebox.Soldier.ForEach(s => s.UpdateNode(null));
        }
    }

    void Quit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
        }
    }

    void LeftClick()
    {
        // 左クリック
        if (Input.GetMouseButtonDown(0))
        {
            // 前にセレクトしたノードの色を戻す
            if (selectNode != null)
            {
                selectNode.Renderer.material.color = selectNode.Normal_Color;
                foreach (var a in selectNode.ConnectNode)
                {
                    a.Renderer.material.color = a.Normal_Color;
                }
            }

            // 一旦選択を解除
            selectNode = null;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500.0f))
            {
                if (hit.collider.tag == "Node")
                {
                    // 新しく選択したノードの色を変える
                    selectNode = hit.collider.gameObject.GetComponent<Node>();
                    selectNode.Renderer.material.color += selectNode.Normal_Color;
                    foreach (var a in selectNode.ConnectNode)
                    {
                        a.Renderer.material.color += a.Normal_Color / 4f;
                    }
                }
            }
        }
    }

    void RightClick()
    {
        // 右クリック
        if (Input.GetMouseButtonDown(1))
        {
            // ノードが選択された状態じゃないならスキップ
            if (selectNode != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500.0f))
                {
                    if (hit.collider.tag == "Node")
                    {
                        Node rightClickNode = hit.collider.GetComponent<Node>();

                        // 右クリックで選択したノードが、現在選択しているノードの隣のノードなら
                        if (selectNode.ConnectNode.Contains(rightClickNode))
                        {
                            // 移動
                            if (selectNode.PlayerEnum == rightClickNode.PlayerEnum)
                            {
                                Move(selectNode, rightClickNode);
                            }
                            // 攻撃
                            else
                            {
                                Attack(selectNode, rightClickNode);
                            }
                        }
                    }
                }
            }
        }


    }

    MoveBox CreateMovebox(Node node1, Node node2)
    {
        // MoveBoxスクリプトの入ったGameObjectを生成する
        MoveBox movebox = new GameObject("MoveBox").AddComponent<MoveBox>();
        // MoveBoxesオブジェクトの子オブジェクトとして格納する
        movebox.transform.SetParent(MoveBoxes.transform);
        // 現在選択しているノードに座標を合わせる
        movebox.transform.position = node1.transform.position;
        // MoveBoxに移動した地点のノードを与える
        movebox.ParentNode = node1;
        // MoveBoxに移動先のノードの参照を与えて初期化する
        movebox.MoveNode = node2;
        // MoveBoxにSelectNodeのPlayerEnum情報を与える
        movebox.PlayerEnum = node1.PlayerEnum;

        return movebox;
    }

    void AttackAI()
    {
        foreach (var node in map.MapNode)
        {
            foreach (var connect in node.ConnectNode)
            {
                if (node.PlayerEnum == connect.PlayerEnum) continue;

                if (Random.Range(0, 100) != 0) continue;

                float character_count01 = node.Commander.Count - 1 + node.Soldier.Count;
                float combatpower01 = character_count01 * (node.Commander.Count + 1 - 1);

                float character_count02 = connect.Commander.Count + connect.Soldier.Count;
                float combatpower02 = character_count02 * (connect.Commander.Count + 1);

                if (character_count01 > combatpower02)
                {
                    Attack(node, connect);
                }
            }
        }
    }

    void MoveAI()
    {
        for (int i = 0; i < Map.PlayerCount; i++)
        {
            // 前線を追加
            List<Node> frontNode = new List<Node>();
            foreach (var node in map.MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i))
            {
                if (node.ConnectNode.Any(c => node.PlayerEnum != c.PlayerEnum))
                {
                    if (!frontNode.Contains(node))
                    {
                        frontNode.Add(node);
                    }
                }
            }

            List<Node> rootNode = new List<Node>();

            // 前線以外のノードが、すべて本拠地に向かう
            foreach (var node in map.MapNode.Where(n => n.PlayerEnum == (PlayerEnum)i))
            {
                if (Random.Range(0, 100) != 0) continue;

                // 違う領土ならスキップ
                if (node.PlayerEnum != (PlayerEnum)i) continue;

                // 前線のノードならスキップ
                if (frontNode.Contains(node)) continue;

                // 初期化
                map.MapNode.ForEach(n => n.PrevNode = null);
                map.MapNode.ForEach(n => n.Cost = float.MaxValue);
                map.MapNode.ForEach(n => n.Done = false);
                rootNode.Clear();

                // 接続先の距離を調べる
                List<Node> openNode = new List<Node>();

                node.Cost = 0;  // 最初のノードのコストは0
                node.Done = true;
                Node minCostNode = node;

                int count = 0;
                while (true)
                {
                    count++;
                    if (count >= 1000)
                    {
                        Debug.Log("無限ループっぽいので終了");
                        break;
                    }

                    foreach (var connectNode in minCostNode.ConnectNode)
                    {
                        // 接続先が違うプレイヤーの領土ならスキップ
                        if (connectNode.PlayerEnum != minCostNode.PlayerEnum) continue;
                        // 接続先がMINCOSTNODEの親ならスキップ
                        if (connectNode == minCostNode.PrevNode) continue;

                        // コストを計算して代入
                        float cost = minCostNode.Cost + Vector3.Distance(connectNode.transform.position, minCostNode.transform.position);

                        // コストを更新
                        if (connectNode.Cost == float.MaxValue || cost < connectNode.Cost)
                        {
                            // 小さい方に更新
                            connectNode.Cost = cost;
                            connectNode.PrevNode = minCostNode;

                            // 接続先が前線ならスキップ
                            if (connectNode.Done) continue;

                            // まだOpenNodeListに追加されていないなら追加
                            if (!openNode.Contains(connectNode)) openNode.Add(connectNode);
                        }
                    }

                    // コストが最小のノードを検索
                    minCostNode = openNode.Find(n => n.Cost == (openNode.Min(m => m.Cost)));

                    // 見つからないならbreak
                    if (minCostNode == null) break;

                    // リストから削除する
                    openNode.Remove(minCostNode);

                    // 確定ノードにする
                    minCostNode.Done = true;

                    // ゴールにたどり着いたら
                    if (minCostNode == frontNode[Random.Range(0, frontNode.Count)])
                    {
                        // ゴールまでの道をPrevNodeを遡って取り出していく
                        rootNode.Add(minCostNode);
                        while (minCostNode.PrevNode != null)
                        {
                            // リストに追加
                            rootNode.Add(minCostNode.PrevNode);
                            // minCostNodeをminCostNode.PrevNodeに更新
                            minCostNode = minCostNode.PrevNode;
                        }

                        // リストを反転
                        rootNode.Reverse();

                        if (rootNode.Count > 1)
                        {
                            Move(rootNode[0], rootNode[1]);
                        }

                        break;
                    }
                }
            }
        }
    }
}