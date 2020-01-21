using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphImage : MonoBehaviour
{
    public float NodeCount { get; set; }
    public Image Image { get; private set; }
    public Text Text { get; private set; }
    public PlayerEnum PlayerEnum { get; set; }

    void Awake()
    {
        Image = GetComponent<Image>();
        Text = transform.GetChild(0).GetComponent<Text>();
    }

    void Start()
    {
        // バーの色
        switch (PlayerEnum)
        {
            case PlayerEnum.Player01: Image.color = new Color(225, 125, 125, 255) / 255f; break;
            case PlayerEnum.Player02: Image.color = new Color(125, 225, 225, 255) / 255f; break;
            case PlayerEnum.Player03: Image.color = new Color(125, 225, 125, 255) / 255f; break;
            case PlayerEnum.Player04: Image.color = new Color(225, 225, 125, 255) / 255f; break;
        }
    }

    void Update()
    {
        Text.text = NodeCount.ToString();
    }
}
