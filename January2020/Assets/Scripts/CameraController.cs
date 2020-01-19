using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameManager GameManager;
    Vector3 velocity;
    Vector3 destVelocity;

    static readonly Vector3 clampMinPos = new Vector3(-100, 3, -100);
    static readonly Vector3 clampMaxPos = new Vector3(100, 90, 100);

    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }

    void Update()
    {
        float moveSpeed = 1.25f;
        float updownSpeed = 0.75f;

        destVelocity = Vector3.zero;

        // 移動処理
        if (Input.GetKey(KeyCode.W)) { destVelocity += Vector3.forward; }
        if (Input.GetKey(KeyCode.S)) { destVelocity += Vector3.back; }
        if (Input.GetKey(KeyCode.A)) { destVelocity += Vector3.left; }
        if (Input.GetKey(KeyCode.D)) { destVelocity += Vector3.right; }

        // 移動量を正規化
        destVelocity.Normalize();
        destVelocity *= moveSpeed * Time.deltaTime * 60;

        // 目標移動量を現在の移動量に線形補完を利用してイージング処理を行う
        velocity = Vector3.Lerp(velocity, destVelocity, 0.1f);

        // 上下移動
        if (Input.GetAxis("Mouse ScrollWheel") > 0) { velocity += Vector3.down * updownSpeed * Time.deltaTime * 60; }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) { velocity += Vector3.up * updownSpeed * Time.deltaTime * 60; }

        // 移動量を座標に足す
        if (!GameManager.IsEnd)
            transform.position += velocity;

        // 座標を指定の範囲に制限する
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, clampMinPos.x, clampMaxPos.x),
            Mathf.Clamp(transform.position.y, clampMinPos.y, clampMaxPos.y),
            Mathf.Clamp(transform.position.z, clampMinPos.z, clampMaxPos.z));
    }
}
