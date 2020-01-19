using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    float deg;

    void Start()
    {

    }

    void Update()
    {
        deg +=0.1f * Time.deltaTime * 60;
        if (deg >= 360) deg -= 360;

        Vector2 vec2 = MyMath.RadToVec2(deg * Mathf.Deg2Rad) * 8;
        Vector3 vec3 = new Vector3(vec2.x, 4.5f, vec2.y);
        transform.position = Vector3.Lerp(transform.position, vec3, 0.1f * Time.deltaTime * 60);

        transform.rotation = Quaternion.LookRotation(-transform.position, Vector3.up);
    }
}
