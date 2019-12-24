using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 velocity;
    Vector3 destVelocity;

    static readonly Vector3 clampMinPos = new Vector3(-20, 10, -15);
    static readonly Vector3 clampMaxPos = new Vector3(20, 30, 5);

    void Start()
    {

    }

    void Update()
    {
        destVelocity = Vector3.zero;
        // 移動処理
        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - Screen.height / 100f) { destVelocity += Vector3.forward; }
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < Screen.height / 100f) { destVelocity += Vector3.back; }
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < Screen.width / 100f) { destVelocity += Vector3.left; }
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - Screen.width / 100f) { destVelocity += Vector3.right; }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) { velocity += Vector3.down; }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) { velocity += Vector3.up; }

        // 移動量を正規化
        destVelocity.Normalize();

        // 目標移動量を現在の移動量に線形補完を利用してイージング処理を行う
        velocity = Vector3.Lerp(velocity, destVelocity, 0.1f);

        // 移動量を座標に足す
        transform.position += velocity * 0.5f;

        // 座標を指定の範囲に制限する
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, clampMinPos.x, clampMaxPos.x),
            Mathf.Clamp(transform.position.y, clampMinPos.y, clampMaxPos.y),
            Mathf.Clamp(transform.position.z, clampMinPos.z, clampMaxPos.z));
    }
}
