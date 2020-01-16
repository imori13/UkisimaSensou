using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class MyTime : MonoBehaviour
{
    public static float deltaTime { get { return Time.deltaTime * timeScale; } }
    public static float time { get { return deltaTime * 60f; } }
    public static float timeScale { get; private set; } = 1f;

    public static bool IsTimeStop { get; set; } = false;

    [SerializeField] Image SlowFilter;

    float destTimeScale = 1f;

    public void Update()
    {
        destTimeScale = 1f;
        if (Input.GetKey(KeyCode.Space) || IsTimeStop)
        {
            destTimeScale = 0;
        }
        timeScale = Mathf.Lerp(timeScale, destTimeScale, 0.2f * Time.deltaTime * 60);
        SlowFilter.color = new Color(SlowFilter.color.r, SlowFilter.color.g, SlowFilter.color.b, 1 - timeScale);
    }
}