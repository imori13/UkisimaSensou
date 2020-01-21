using UnityEngine;

// 指揮官キャラクター
public class Commander : MonoBehaviour
{
    public Node ParentNode { get; set; }
    public Vector3 DestPosition { get; set; }

    Vector3 prevPos;

    [SerializeField] Material[] MaterialArray;
    [SerializeField] SkinnedMeshRenderer SkinnedMeshRenderer;

    public Animator Animator { get; private set; }

    void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, DestPosition, 0.1f * MyTime.time);

        Vector3 dir = transform.position - prevPos;
        Vector3 vec3 = new Vector3(dir.x, 0, dir.z);
        if (vec3.magnitude >= 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(vec3);
        }

        prevPos = transform.position;

        Animator.speed = MyTime.timeScale;
    }

    public void UpdateNode(Node node)
    {
        // nullが与えられたらnullを代入してreturn
        if (node == null) { node = null; return; }

        // 更新するノードが同じなら早期リターン
        if (ParentNode == node) return;

        // 親を更新
        ParentNode = node;

        // 子オブジェクトに移動
        transform.SetParent(ParentNode.transform);

        // マテリアルを更新
        Material material = SkinnedMeshRenderer.material;
        SkinnedMeshRenderer.material = MaterialArray[(int)ParentNode.PlayerEnum];
        Destroy(material);
    }
}
