﻿using System.Collections;
using System.Collections.Generic;
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
    Vector3 GenerateSize = new Vector3(25, 0, 15);   // 生成範囲
    Node selectNode;    // 現在選択しているノード

    void Start()
    {
        // 選択しているノードをnull
        selectNode = null;

        // ノードを作成
        while (MapNode.Count < 40)
        {
            CreateNodes();
            CreateRoads();
            RemoveIsolation();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 前にセレクトしたノードの色を戻す
            if (selectNode != null)
            {
                selectNode.Renderer.material.color = Node.NORMALCOLOR;
                foreach (var a in selectNode.ConnectNode)
                {
                    a.Renderer.material.color = Node.NORMALCOLOR;
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

    // ノードを作成
    void CreateNodes()
    {
        // ノードを作成
        while (MapNode.Count < 40)
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

                // 距離が離れすぎているものは橋を繋げない
                Vector3 distance = MapNode[j].transform.position - MapNode[i].transform.position;
                if (Vector3.SqrMagnitude(distance) >= (6 * 6)) { continue; }
                if (MapNode[j].ConnectNode.Count >= 4 || MapNode[i].ConnectNode.Count >= 4) { continue; }

                // 橋を生成
                Road road = Instantiate(Road).GetComponent<Road>();
                road.transform.rotation = Quaternion.LookRotation(distance, Vector3.up);
                road.transform.position = (MapNode[i].transform.position) + (road.transform.forward * distance.magnitude / 2f);
                road.transform.localScale = new Vector3(0.5f, 0.1f, distance.magnitude);
                road.transform.SetParent(Roads.transform);
                road.PosS = MyMath.ConversionVector2(MapNode[i].transform.position);
                road.PosE = MyMath.ConversionVector2(MapNode[j].transform.position);

                // すでにある道と交差していたら、スキップ
                bool flag = false;
                foreach (var r in MapRoad)
                {
                    if (MyMath.JudgeIentersected(road.PosS, road.PosE, r.PosS, r.PosE))
                    {
                        road.GetComponent<Renderer>().material.color = Color.red;
                        Destroy(road.gameObject);
                        flag = true;
                        continue;
                    }
                }
                if (flag) continue;

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
}
