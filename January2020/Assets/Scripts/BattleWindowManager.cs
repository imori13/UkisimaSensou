using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleWindowManager : MonoBehaviour
{
    // prefab
    [SerializeField] GameObject CommanderBattlePrefab;
    [SerializeField] GameObject SoldierBattlePrefab;

    [SerializeField] GameObject BattleCamera;
    [SerializeField] Text AttackCombatPower;
    [SerializeField] Text DefenceCombatPower;
    List<GameObject> attackBattleSoldier = new List<GameObject>();
    List<GameObject> attackBattleCommander = new List<GameObject>();
    List<GameObject> defenceBattleSoldier = new List<GameObject>();
    List<GameObject> defenceBattleCommander = new List<GameObject>();

    static Vector3 BATTLE_POS = new Vector3(0, 0, -500f);

    // バトルするMoveBox
    public MoveBox BattleMoveBox { get; set; } = null;

    void Start()
    {
        BattleCamera.SetActive(false);
        AttackCombatPower.text = "";
        DefenceCombatPower.text = "";
    }

    public void Initialize(MoveBox moveBox)
    {
        AttackCombatPower.text = "";
        DefenceCombatPower.text = "";
        attackBattleSoldier.Clear();
        attackBattleCommander.Clear();
        defenceBattleSoldier.Clear();
        defenceBattleCommander.Clear();

        BattleMoveBox = moveBox;

        // 攻撃側兵士
        for (int i = 0; i < BattleMoveBox.Soldier.Count; i++)
        {
            int count = BattleMoveBox.Soldier.Count;
            GameObject instance = Instantiate(SoldierBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(-4, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, 90, 0);
            instance.transform.localScale = Vector3.zero;
            attackBattleSoldier.Add(instance);
        }
        // 攻撃側指揮官
        for (int i = 0; i < BattleMoveBox.Commander.Count; i++)
        {
            int count = BattleMoveBox.Commander.Count;
            GameObject instance = Instantiate(CommanderBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(-6, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, 90, 0);
            instance.transform.localScale = Vector3.zero;
            attackBattleCommander.Add(instance);
        }

        // 防御側兵士
        for (int i = 0; i < BattleMoveBox.Node2.Soldier.Count; i++)
        {
            int count = BattleMoveBox.Node2.Soldier.Count;
            GameObject instance = Instantiate(SoldierBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(4, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, -90, 0);
            instance.transform.localScale = Vector3.zero;
            defenceBattleSoldier.Add(instance);
        }
        // 防御側指揮官
        for (int i = 0; i < BattleMoveBox.Node2.Commander.Count; i++)
        {
            int count = BattleMoveBox.Node2.Commander.Count;
            GameObject instance = Instantiate(CommanderBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(6, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, -90, 0);
            instance.transform.localScale = Vector3.zero;
            defenceBattleCommander.Add(instance);
        }

        StartCoroutine("State01");
    }

    // 時間を止めて、戦闘が起きているノードにカメラが移動する
    IEnumerator State01()
    {
        MyTime.IsTimeStop = true;
        float time = 0;
        while (true)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, BattleMoveBox.Node2.transform.position + new Vector3(0, 20, -15f), 0.2f * Time.deltaTime * 60);
            time += Time.deltaTime;

            if (time >= 1) { break; }

            yield return null;
        }

        StartCoroutine("State02");
        yield break;
    }

    // State01から一定時間後に、戦闘画面を表示する
    IEnumerator State02()
    {
        BattleCamera.SetActive(true);
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;

            if (time >= 0.25f) { break; }

            yield return null;
        }

        yield return null;
        StartCoroutine("State03");
        yield break;
    }

    // 攻撃側キャラクターが出現する、戦闘力UIが出現する
    IEnumerator State03()
    {
        Vector3 scale = Vector3.zero;
        float time = 0;
        float power = 0;
        while (true)
        {
            scale = Vector3.Lerp(scale, Vector3.one * 0.33f, 0.2f * Time.deltaTime * 60);

            attackBattleSoldier.ForEach(s => s.transform.localScale = scale);
            attackBattleCommander.ForEach(c => c.transform.localScale = scale);

            power = Mathf.Lerp(power, BattleMoveBox.ButtleStruct.AttackBasicCombatPower, 0.2f * Time.deltaTime * 60);
            AttackCombatPower.text = Mathf.RoundToInt(power).ToString();

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleStruct.AttackBasicCombatPower)
            {
                time += Time.deltaTime;

                if (time >= 0.1f) { break; }
            }
            yield return null;
        }

        yield return null;
        StartCoroutine("State04");
        yield break;
    }

    // 防御側キャラクターが出現して、戦闘力UIが出現する
    IEnumerator State04()
    {
        Vector3 scale = Vector3.zero;
        float time = 0;
        float power = 0;
        while (true)
        {
            scale = Vector3.Lerp(scale, Vector3.one * 0.33f, 0.2f * Time.deltaTime * 60);

            defenceBattleSoldier.ForEach(s => s.transform.localScale = scale);
            defenceBattleCommander.ForEach(c => c.transform.localScale = scale);

            power = Mathf.Lerp(power, BattleMoveBox.ButtleStruct.DefenceBasicCombatPower, 0.2f * Time.deltaTime * 60);
            DefenceCombatPower.text = Mathf.RoundToInt(power).ToString();

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleStruct.DefenceBasicCombatPower)
            {
                time += Time.deltaTime;

                if (time >= 0.1f) { break; }
            }
            yield return null;
        }


        yield return null;
        StartCoroutine("State05");
        yield break;
    }

    // 攻撃側がサイコロを振って、戦闘力に計上する
    IEnumerator State05()
    {
        float time = 0;
        float power = BattleMoveBox.ButtleStruct.AttackBasicCombatPower;
        while (true)
        {
            power = Mathf.Lerp(power, BattleMoveBox.ButtleStruct.AttackTotalCombatPower, 0.2f * Time.deltaTime * 60);
            AttackCombatPower.text = Mathf.RoundToInt(power).ToString();

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleStruct.AttackTotalCombatPower)
            {
                time += Time.deltaTime;

                if (time >= 0.2f) { break; }
            }
            yield return null;
        }

        yield return null;
        StartCoroutine("State06");
        yield break;
    }

    // 防御側がサイコロを振って、戦闘力に計上する
    IEnumerator State06()
    {
        float time = 0;
        float power = BattleMoveBox.ButtleStruct.DefenceBasicCombatPower;
        while (true)
        {
            power = Mathf.Lerp(power, BattleMoveBox.ButtleStruct.DefenceTotalCombatPower, 0.2f * Time.deltaTime * 60);
            DefenceCombatPower.text = Mathf.RoundToInt(power).ToString();

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleStruct.DefenceTotalCombatPower)
            {
                time += Time.deltaTime;

                if (time >= 0.2f) { break; }
            }
            yield return null;
        }

        yield return null;
        StartCoroutine("State07");
        yield break;
    }

    // 勝ったほうのキャラ群が、相手に突っ込んで相手側が消滅する
    IEnumerator State07()
    {
        bool attackWin = BattleMoveBox.ButtleStruct.AttackTotalCombatPower >= BattleMoveBox.ButtleStruct.DefenceTotalCombatPower;

        if (attackWin)
        {
            defenceBattleSoldier.ForEach(s => Destroy(s));
            defenceBattleCommander.ForEach(c => Destroy(c));
        }
        else
        {
            attackBattleSoldier.ForEach(s => Destroy(s));
            attackBattleCommander.ForEach(c => Destroy(c));
        }

        float time = 0;

        while (true)
        {
            time += Time.deltaTime;

            if (attackWin)
            {
                attackBattleSoldier.ForEach(s => s.transform.position = Vector3.Lerp(s.transform.position, BATTLE_POS + Vector3.right * 3, 0.1f * Time.deltaTime * 60));
                attackBattleCommander.ForEach(c => c.transform.position = Vector3.Lerp(c.transform.position, BATTLE_POS + Vector3.right * 3, 0.1f * Time.deltaTime * 60));
            }
            else
            {
                defenceBattleSoldier.ForEach(s => s.transform.position = Vector3.Lerp(s.transform.position, BATTLE_POS + Vector3.left * 3, 0.1f * Time.deltaTime * 60));
                defenceBattleCommander.ForEach(c => c.transform.position = Vector3.Lerp(c.transform.position, BATTLE_POS + Vector3.left * 3, 0.1f * Time.deltaTime * 60));
            }

            yield return null;
            if (time >= 1) { break; }
        }

        if (attackWin)
        {
            attackBattleSoldier.ForEach(s => Destroy(s));
            attackBattleCommander.ForEach(c => Destroy(c));
        }
        else
        {
            defenceBattleSoldier.ForEach(s => Destroy(s));
            defenceBattleCommander.ForEach(c => Destroy(c));
        }

        yield return null;
        StartCoroutine("State08");
        yield break;
    }

    // 戦闘ウィンドウを閉じる
    IEnumerator State08()
    {
        BattleCamera.SetActive(false);

        float time = 0;
        while (true)
        {
            time += Time.deltaTime;

            yield return null;
            if (time >= 0.1f) { break; }
        }

        yield return null;
        StartCoroutine("State09");
        yield break;
    }

    // 戦闘ウィンドウが閉じたら、時間を再生して元に戻す
    IEnumerator State09()
    {
        MyTime.IsTimeStop = false;
        BattleMoveBox.AttackHoge();

        BattleMoveBox = null;

        yield return null;
        yield break;
    }
}
