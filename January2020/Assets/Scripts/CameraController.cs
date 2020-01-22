using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameManager GameManager;
    Vector3 destVelocity;
    public Vector3 DestPosition { get; set; }
    Vector3 offsetDirection = new Vector3(0, 25, -15).normalized;
    float distance = 0;

    static readonly Vector3 clampMinPos = new Vector3(-100, 10, -100);
    static readonly Vector3 clampMaxPos = new Vector3(100, 35, 100);

    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    void Update()
    {
        float moveSpeed = 1.25f;
        float updownSpeed = 15f;

        destVelocity = Vector3.zero;

        // 移動処理
        if (Input.GetKey(KeyCode.W)) { destVelocity += Vector3.forward; }
        if (Input.GetKey(KeyCode.S)) { destVelocity += Vector3.back; }
        if (Input.GetKey(KeyCode.A)) { destVelocity += Vector3.left; }
        if (Input.GetKey(KeyCode.D)) { destVelocity += Vector3.right; }

        // 移動量を正規化
        destVelocity.Normalize();
        destVelocity *= moveSpeed * Time.deltaTime * 60;
        DestPosition += destVelocity;

        // 上下移動
        if (Input.GetAxis("Mouse ScrollWheel") > 0) { distance -= updownSpeed * Time.deltaTime * 60; }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) { distance += updownSpeed * Time.deltaTime * 60; }
        distance = Mathf.Clamp(distance, clampMinPos.y, clampMaxPos.y);

        // 移動量を座標に足す
        if (!GameManager.IsEnd)
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, DestPosition.x + (offsetDirection.x * distance), 0.05f * Time.deltaTime * 60),
                Mathf.Lerp(transform.position.y, DestPosition.y + (offsetDirection.y * distance), 0.1f * Time.deltaTime * 60),
                Mathf.Lerp(transform.position.z, DestPosition.z + (offsetDirection.z * distance), 0.05f * Time.deltaTime * 60));

        // 座標を指定の範囲に制限する
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, clampMinPos.x, clampMaxPos.x),
            Mathf.Clamp(transform.position.y, clampMinPos.y, clampMaxPos.y),
            Mathf.Clamp(transform.position.z, clampMinPos.z, clampMaxPos.z));
    }
}
