using UnityEngine;

// 兵士
public class Soldier : MonoBehaviour
{
    public Node ParentNode { get; set; }

    public Vector3 DestPosition { get; set; }

    public Animator Animator { get; private set; }

    Vector3 prevPos;

    [SerializeField] Material[] MaterialArray;
    [SerializeField] SkinnedMeshRenderer SkinnedMeshRenderer;

    void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, DestPosition, 0.1f * MyTime.time);
        Vector3 dir = transform.position - prevPos;
        Vector3 vec3 = new Vector3(dir.x, 0, dir.z);
        if (vec3.magnitude >= 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(vec3);
        }

        Animator.speed = MyTime.timeScale;

        prevPos = transform.position;
    }

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
        DestPosition = (ParentNode.IsBaseNode) ? (MyMath.CircleRandom(0.8f, 1.3f)) : (MyMath.CircleRandom(0.5f, 1.0f));
        transform.position = ParentNode.transform.position + DestPosition;

        // マテリアルを更新
        Material material = SkinnedMeshRenderer.material;
        SkinnedMeshRenderer.material = MaterialArray[(int)ParentNode.PlayerEnum];
        Destroy(material);
    }
}
