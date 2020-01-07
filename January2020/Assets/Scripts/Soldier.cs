using UnityEngine;

// 兵士
public class Soldier : MonoBehaviour
{
    public Node ParentNode { get; set; }

    public void UpdateNode(Node node)
    {
        // nullが与えられたらnullを代入してreturn
        if (node == null) { node = null; return; }

        // 更新するノードが同じなら早期リターン
        if (node == ParentNode) return;

        // 親を更新
        ParentNode = node;

        // 子オブジェクトに移動
        transform.SetParent(ParentNode.transform);

        // 座標を更新
        float scale = ParentNode.transform.localScale.x / 2f;
        transform.position
            = ParentNode.transform.position
            + (Vector3.up * 0.25f)
            + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
    }
}
