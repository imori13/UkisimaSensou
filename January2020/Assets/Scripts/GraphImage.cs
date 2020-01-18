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
    [SerializeField] Image PlayerColor;

    void Awake()
    {
        Image = GetComponent<Image>();
        Text = transform.GetChild(1).GetComponent<Text>();
    }

    void Start()
    {
        // バーの色
        //switch (PlayerEnum)
        //{
        //    case PlayerEnum.Player01: Image.color = Color.green; break;
        //    case PlayerEnum.Player02: Image.color = Color.red*Color.gray*1.2f; break;
        //    case PlayerEnum.Player03: Image.color = Color.blue * Color.gray * 1.2f; break;
        //    case PlayerEnum.Player04: Image.color = Color.yellow * Color.gray * 1.2f; break;
        //}
        Image.color = new Color(50, 50, 50, 255) / 255f;

        // アイコンの色
        switch (PlayerEnum)
        {
            case PlayerEnum.Player01: PlayerColor.color = Color.green; break;
            case PlayerEnum.Player02: PlayerColor.color = Color.red; break;
            case PlayerEnum.Player03: PlayerColor.color = Color.blue; break;
            case PlayerEnum.Player04: PlayerColor.color = Color.yellow; break;
        }
    }

    void Update()
    {
        Text.text = NodeCount.ToString();
    }
}
