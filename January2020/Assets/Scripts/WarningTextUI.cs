using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningTextUI : MonoBehaviour
{
    static Image BackImage;
    static Text WarningText;
    [SerializeField] Image Image;
    [SerializeField] Text Text;

    static float time;
    static bool view;

    void Awake()
    {
        BackImage = Image;
        WarningText = Text;
        time = 0;
        view = false;
        WarningText.text = "";
        BackImage.color = BackImage.color * new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (view)
        {
            time += Time.deltaTime;
            float limit = 2.0f;
            if (time >= limit)
            {
                time = 0;
                view = false;
                WarningText.text = "";
                BackImage.color = BackImage.color * new Color(1, 1, 1, 0);
            }
        }
    }

    public static void UpdateWarningText(string text)
    {
        view = true;
        time = 0;
        WarningText.text = text;
        BackImage.color = BackImage.color + new Color(0, 0, 0, 1);
    }
}
