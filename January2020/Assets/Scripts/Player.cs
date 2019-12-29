using UnityEngine;

// クリック操作をする操作する側のクラス
public class Player : MonoBehaviour
{
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
                        a.Renderer.material.color += a.Normal_Color/4f;
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
                            // !!!仮!!!
                            // 指揮官を選択したノードから右クリックしたノードに移す
                            rightClickNode.ConnectCommander.AddRange(selectNode.ConnectCommander);
                            // 親を更新
                            selectNode.ConnectCommander.ForEach(c => c.UpdateParentNode(rightClickNode));
                            // 現在選択しているノードの指揮官をクリア
                            selectNode.ConnectCommander.Clear();
                        }
                    }
                }
            }
        }
    }
}