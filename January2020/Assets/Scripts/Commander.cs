using UnityEngine;

// 指揮官キャラクター
public class Commander : MonoBehaviour
{
    public Node ParentNode { get; set; }

    public void UpdateParentNode(Node node)
    {
        // 更新するノードが同じなら早期リターン
        if (node == ParentNode) return;

        // 親を更新
        ParentNode = node;

        // 座標を更新
        float scale = ParentNode.transform.localScale.x / 2f;
        transform.position
            = ParentNode.transform.position
            + (Vector3.up * 0.5f)
            + (new Vector3(Random.Range(-scale, scale), 0, Random.Range(-scale, scale)));
    }
}
