using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] GameObject Nodes;
    [SerializeField] GameObject Roads;
    [SerializeField] Node Node;
    [SerializeField] GameObject Road;

    List<Node> MapNode = new List<Node>();  // 全ノードを格納するリスト
    Vector3 GenerateSize = new Vector3(25, 0.5f, 15);   // 生成範囲
    Node selectNode;    // 現在選択しているノード

    void Start()
    {
        // 選択しているノードをnull
        selectNode = null;

        // ノードを作成
        int count = 0;
        while (count < 70)
        {
            // 範囲内でランダムに生成
            Node instance = Instantiate(Node);
            instance.transform.position = MyMath.RandomGenerateSize(GenerateSize);

            // すでに追加されているノードと比べてどれか近かったら追加しない
            bool flag = false;
            foreach (var node in MapNode)
            {
                if (Vector3.SqrMagnitude(MyMath.DiscardTheValueOfY(instance.transform.position) - MyMath.DiscardTheValueOfY(node.transform.position)) <= (3 * 3))
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

            count++;
        }

        // 橋を作成
        for (int i = 0; i < MapNode.Count; i++)
        {
            for (int j = 0; j < MapNode.Count; j++)
            {
                // 重複はスキップ
                if (i >= j) continue;

                // 距離が離れすぎているものは橋を繋げない
                Vector3 distance = MapNode[j].transform.position - MapNode[i].transform.position;
                if (Vector3.SqrMagnitude(distance) >= (6 * 6)) { continue; }
                //if (!MapNode[i].NodeAddProbability()) { continue; }

                // 橋を生成
                GameObject road = Instantiate(Road);
                road.transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
                road.transform.position = (MapNode[i].transform.position) + (road.transform.forward * distance.magnitude / 2f);
                road.transform.localScale = new Vector3(0.1f, 0.1f, distance.magnitude);
                road.transform.SetParent(Roads.transform);

                // つながっているノードの参照をお互いに渡す
                MapNode[i].ConnectNode.Add(MapNode[j]);
                MapNode[j].ConnectNode.Add(MapNode[i]);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 前にセレクトしたノードの色を戻す
            if (selectNode != null)
            {
                selectNode.Renderer.material.color = Color.green;
                foreach (var a in selectNode.ConnectNode)
                {
                    a.Renderer.material.color = Color.green;
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
                    selectNode.Renderer.material.color = Color.red;
                    foreach (var a in selectNode.ConnectNode)
                    {
                        a.Renderer.material.color = Color.yellow;
                    }
                }
            }
        }
    }
}
