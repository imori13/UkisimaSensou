using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618 // 型またはメンバーが古い形式です

public class BattleResultUI : MonoBehaviour
{
    [SerializeField] AnimationClip Red2Blue;
    [SerializeField] AnimationClip Red2Green;
    [SerializeField] AnimationClip Red2Yellow;

    [SerializeField] AnimationClip Blue2Red;
    [SerializeField] AnimationClip Blue2Green;
    [SerializeField] AnimationClip Blue2Yellow;

    [SerializeField] AnimationClip Green2Red;
    [SerializeField] AnimationClip Green2Blue;
    [SerializeField] AnimationClip Green2Yellow;

    [SerializeField] AnimationClip Yellow2Red;
    [SerializeField] AnimationClip Yellow2Blue;
    [SerializeField] AnimationClip Yellow2Green;

    string overrideClipName = "AnimationClip"; // 上書きするAnimationClip対象

    private AnimatorOverrideController overrideController;
    private Animator anim;

    public void Initialize(PlayerEnum node1, PlayerEnum node2)
    {
        anim = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
        anim.runtimeAnimatorController = overrideController;

        switch (node1)
        {
            case PlayerEnum.Player01:
                switch (node2)
                {
                    case PlayerEnum.Player02:
                        ChangeClip(Red2Blue);
                        break;
                    case PlayerEnum.Player03:
                        ChangeClip(Red2Green);
                        break;
                    case PlayerEnum.Player04:
                        ChangeClip(Red2Yellow);
                        break;
                }
                break;
            case PlayerEnum.Player02:
                switch (node2)
                {
                    case PlayerEnum.Player01:
                        ChangeClip(Blue2Red);
                        break;
                    case PlayerEnum.Player03:
                        ChangeClip(Blue2Green);
                        break;
                    case PlayerEnum.Player04:
                        ChangeClip(Blue2Yellow);
                        break;
                }
                break;
            case PlayerEnum.Player03:
                switch (node2)
                {
                    case PlayerEnum.Player01:
                        ChangeClip(Green2Red);
                        break;
                    case PlayerEnum.Player02:
                        ChangeClip(Green2Blue);
                        break;
                    case PlayerEnum.Player04:
                        ChangeClip(Green2Yellow);
                        break;
                }
                break;
            case PlayerEnum.Player04:
                switch (node2)
                {
                    case PlayerEnum.Player01:
                        ChangeClip(Yellow2Red);
                        break;
                    case PlayerEnum.Player02:
                        ChangeClip(Yellow2Blue);
                        break;
                    case PlayerEnum.Player03:
                        ChangeClip(Yellow2Green);
                        break;
                }
                break;
        }
    }

    public void ChangeClip(AnimationClip clip)
    {
        // ステートをキャッシュ
        AnimatorStateInfo[] layerInfo = new AnimatorStateInfo[anim.layerCount];
        for (int i = 0; i < anim.layerCount; i++)
        {
            layerInfo[i] = anim.GetCurrentAnimatorStateInfo(i);
        }

        // AnimationClipを差し替えて、強制的にアップデート
        // ステートがリセットされる
        overrideController[overrideClipName] = clip;
        anim.Update(0.0f);

        // ステートを戻す
        for (int i = 0; i < anim.layerCount; i++)
        {
            anim.Play(layerInfo[i].nameHash, i, layerInfo[i].normalizedTime);
        }
    }
}