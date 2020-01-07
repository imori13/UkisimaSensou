using UnityEngine;

// クリック操作をする操作する側のクラス
public class Player : MonoBehaviour
{
    [SerializeField] GameObject MoveBoxes;
    Node selectNode;    // 現在選択しているノード

    void Start()
    {
        // 選択しているノードをnull
        selectNode = null;
    }

    void Update()
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
                            // MoveBoxスクリプトの入ったGameObjectを生成する
                            MoveBox movebox = new GameObject("MoveBox").AddComponent<MoveBox>();
                            // MoveBoxesオブジェクトの子オブジェクトとして格納する
                            movebox.transform.SetParent(MoveBoxes.transform);
                            // 現在選択しているノードに座標を合わせる
                            movebox.transform.position = selectNode.transform.position;
                            // MoveBoxに移動先のノードの参照を与えて初期化する
                            movebox.MoveNode = rightClickNode;
                            // MoveBoxにSelectNodeのPlayerEnum情報を与える
                            movebox.PlayerEnum = selectNode.PlayerEnum;

                            Debug.Log("Select : " + selectNode.PlayerEnum.ToString() + " Right : " + rightClickNode.PlayerEnum.ToString());

                            // 移動
                            if (selectNode.PlayerEnum == rightClickNode.PlayerEnum)
                            {
                                // 兵士がいないなら早期リターン
                                if (selectNode.Soldier.Count <= 0)
                                {
                                    Destroy(movebox.gameObject);
                                    return;
                                }

                                // [移動BOXに兵士を移動]
                                movebox.Soldier.AddRange(selectNode.Soldier);
                                selectNode.Soldier.Clear();

                                // 兵士を自分の子オブジェクトに格納する
                                movebox.Soldier.ForEach(s => s.transform.SetParent(movebox.transform));

                                // 親をNULLに更新
                                movebox.Soldier.ForEach(s => s.UpdateNode(null));
                            }
                            // 攻撃
                            else
                            {
                                // 指揮官が二人未満なら、または兵士がいないなら早期リターン
                                if (selectNode.Commander.Count < 2 || selectNode.Soldier.Count <= 0)
                                {
                                    Destroy(movebox.gameObject);
                                    return;
                                }

                                // [移動BOXに指揮官を移動]
                                // インデックス最初のやつを保持
                                Commander firstCommander = selectNode.Commander[0];
                                // インデックスの最初のやつをリストから一時的に外す
                                selectNode.Commander.RemoveAt(0);
                                // 相手の指揮官を消して自分の兵士を指揮官させる
                                movebox.Commander.AddRange(selectNode.Commander);
                                selectNode.Commander.Clear();
                                // 一旦削除しておいた最初の指揮官を追加しなおす
                                selectNode.Commander.Add(firstCommander);

                                // [移動BOXに兵士を移動]
                                movebox.Soldier.AddRange(selectNode.Soldier);
                                selectNode.Soldier.Clear();

                                // 指揮官と兵士を自分の子オブジェクトに格納する
                                movebox.Commander.ForEach(c => c.transform.SetParent(movebox.transform));
                                movebox.Soldier.ForEach(s => s.transform.SetParent(movebox.transform));

                                // 親をNULLに更新
                                movebox.Commander.ForEach(s => s.UpdateNode(null));
                                movebox.Soldier.ForEach(s => s.UpdateNode(null));
                            }
                        }
                    }
                }
            }
        }
    }
}