using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618 // 型またはメンバーが古い形式です

public class BattleResultUI : MonoBehaviour
{
    public void Initialize(PlayerEnum node1)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        switch (node1)
        {
            case PlayerEnum.Player01:
                sr.color = Color.red;
                break;
            case PlayerEnum.Player02:
                sr.color = Color.blue;
                break;
            case PlayerEnum.Player03:
                sr.color = Color.green;
                break;
            case PlayerEnum.Player04:
                sr.color = Color.yellow;
                break;
        }
    }
}