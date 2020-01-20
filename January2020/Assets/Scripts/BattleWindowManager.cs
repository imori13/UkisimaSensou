using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleWindowManager : MonoBehaviour
{
    // prefab
    [SerializeField] BattleCommander CommanderBattlePrefab;
    [SerializeField] BattleSoldier SoldierBattlePrefab;
    [SerializeField] GameObject ExplosionParticlePrefab;

    [SerializeField] GameObject BattleCameraBackImage;
    [SerializeField] GameObject BattleCamera;
    [SerializeField] Text AttackCombatPowerText;
    [SerializeField] Text DefenceCombatPowerText;
    [SerializeField] Text AttackMultiplicationText;
    [SerializeField] Text DefenceMultiplicationText;
    [SerializeField] Sprite[] DiceImage;
    [SerializeField] GameObject DiceUI;
    [SerializeField] Image DiceImagePrefab;
    [SerializeField] Image AttackBackImage;
    [SerializeField] Image DefenceBackImage;
    [SerializeField] Material[] SoldierMaterials;
    [SerializeField] Material[] CommanderMaterials;
    List<BattleSoldier> attackBattleSoldier = new List<BattleSoldier>();
    List<BattleCommander> attackBattleCommander = new List<BattleCommander>();
    List<BattleSoldier> defenceBattleSoldier = new List<BattleSoldier>();
    List<BattleCommander> defenceBattleCommander = new List<BattleCommander>();
    List<Image> AttackDiceImageList = new List<Image>();
    List<Image> DefenceDiceImageList = new List<Image>();

    static Vector3 BATTLE_POS = new Vector3(0, 0, -500f);

    // バトルするMoveBox
    public MoveBox BattleMoveBox { get; set; } = null;

    void Start()
    {
        BattleCamera.SetActive(false);
        BattleCameraBackImage.SetActive(false);
        AttackBackImage.gameObject.SetActive(false);
        DefenceBackImage.gameObject.SetActive(false);
        AttackBackImage.color = Color.clear;
        DefenceBackImage.color = Color.clear;
        AttackCombatPowerText.text = "";
        DefenceCombatPowerText.text = "";
        AttackMultiplicationText.text = "";
        DefenceMultiplicationText.text = "";
    }

    public void Initialize(MoveBox moveBox)
    {
        AttackCombatPowerText.text = "";
        DefenceCombatPowerText.text = "";
        AttackMultiplicationText.text = "";
        DefenceMultiplicationText.text = "";
        attackBattleSoldier.Clear();
        attackBattleCommander.Clear();
        defenceBattleSoldier.Clear();
        defenceBattleCommander.Clear();

        BattleMoveBox = moveBox;

        // 攻撃側兵士
        for (int i = 0; i < BattleMoveBox.Soldier.Count; i++)
        {
            int count = BattleMoveBox.Soldier.Count;
            BattleSoldier instance = Instantiate(SoldierBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(-5, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, 90, 0);
            instance.transform.localScale = Vector3.zero;
            Material material = instance.SkinnedMeshRenderer.material;
            instance.SkinnedMeshRenderer.material = SoldierMaterials[(int)BattleMoveBox.PlayerEnum];
            Destroy(material);
            attackBattleSoldier.Add(instance);
        }
        // 攻撃側指揮官
        for (int i = 0; i < BattleMoveBox.Commander.Count; i++)
        {
            int count = BattleMoveBox.Commander.Count;
            BattleCommander instance = Instantiate(CommanderBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(-8, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, 90, 0);
            instance.transform.localScale = Vector3.zero;
            Material material = instance.SkinnedMeshRenderer.material;
            instance.SkinnedMeshRenderer.material = CommanderMaterials[(int)BattleMoveBox.PlayerEnum];
            Destroy(material);
            attackBattleCommander.Add(instance);
        }

        // 防御側兵士
        for (int i = 0; i < BattleMoveBox.Node2.Soldier.Count; i++)
        {
            int count = BattleMoveBox.Node2.Soldier.Count;
            BattleSoldier instance = Instantiate(SoldierBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(2, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, -90, 0);
            instance.transform.localScale = Vector3.zero;
            Material material = instance.SkinnedMeshRenderer.material;
            instance.SkinnedMeshRenderer.material = SoldierMaterials[(int)BattleMoveBox.Node2.PlayerEnum];
            Destroy(material);
            defenceBattleSoldier.Add(instance);
        }
        // 防御側指揮官
        for (int i = 0; i < BattleMoveBox.Node2.Commander.Count; i++)
        {
            int count = BattleMoveBox.Node2.Commander.Count;
            BattleCommander instance = Instantiate(CommanderBattlePrefab);
            instance.transform.position = BATTLE_POS + new Vector3(5, 0, (i - (count * 0.5f) + 0.5f) * 1.5f);
            instance.transform.Rotate(0, -90, 0);
            instance.transform.localScale = Vector3.zero;
            Material material = instance.SkinnedMeshRenderer.material;
            instance.SkinnedMeshRenderer.material = CommanderMaterials[(int)BattleMoveBox.Node2.PlayerEnum];
            Destroy(material);
            defenceBattleCommander.Add(instance);
        }
        
        attackBattleSoldier.ForEach(s => s.Animator.SetBool("SoldierRun", true));
        attackBattleSoldier.ForEach(s => s.Animator.CrossFade("SoldierRun", 0, 0, Random.Range(0f, 1f)));
        attackBattleCommander.ForEach(c => c.Animator.SetBool("CommanderRun", true));
        attackBattleCommander.ForEach(c => c.Animator.CrossFade("CommanderRun", 0, 0, Random.Range(0f, 1f)));

        StartCoroutine("State01");
    }

    // 時間を止めて、戦闘が起きているノードにカメラが移動する
    IEnumerator State01()
    {
        MyTime.IsTimeStop = true;
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;

            if (BattleMoveBox.Node2.PlayerEnum == PlayerEnum.Player01)
            {
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, (BattleMoveBox.Node2.transform.position + Vector3.up * 10), 0.1f * Time.deltaTime * 60);
                if (time >= 1)
                {
                    break;
                }
            }
            else
            {
                break;
            }

            yield return null;
        }

        StartCoroutine("State02");
        yield break;
    }

    // State01から一定時間後に、戦闘画面を表示する
    IEnumerator State02()
    {
        BattleCameraBackImage.SetActive(true);
        BattleCamera.SetActive(true);
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;

            if (time >= 0.0f) { break; }

            yield return null;
        }

        yield return null;
        StartCoroutine("State03");
        yield break;
    }

    // 攻撃側キャラクターと防御側キャラクターが出現して、戦闘力UIが出現する
    IEnumerator State03()
    {
        Vector3 scale = Vector3.zero;
        float time = 0;
        float power1 = 0;
        float power2 = 0;

        AttackMultiplicationText.text = "×1.0";
        DefenceMultiplicationText.text = "1.0×";

        AttackBackImage.gameObject.SetActive(true);
        DefenceBackImage.gameObject.SetActive(true);

        float[] sRate = new float[attackBattleSoldier.Count];
        for (int i = 0; i < sRate.Length; i++)
        {
            sRate[i] = Random.Range(0.075f, 0.2f);
        }
        float[] cRate = new float[attackBattleCommander.Count];
        for (int i = 0; i < cRate.Length; i++)
        {
            cRate[i] = Random.Range(0.075f, 0.2f);
        }

        while (true)
        {
            scale = Vector3.Lerp(scale, Vector3.one * 0.33f, 0.2f * Time.deltaTime * 60);

            AttackBackImage.color = Color.Lerp(AttackBackImage.color, new Color(50, 50, 50, 190) / 255f, 0.1f * Time.deltaTime * 60);
            DefenceBackImage.color = Color.Lerp(DefenceBackImage.color, new Color(50, 50, 50, 190) / 255f, 0.1f * Time.deltaTime * 60);

            // 攻撃側
            attackBattleSoldier.ForEach(s => s.transform.localScale = scale);
            attackBattleCommander.ForEach(c => c.transform.localScale = scale);

            // 攻撃側兵士
            for (int i = 0; i < attackBattleSoldier.Count; i++)
            {
                int count = attackBattleSoldier.Count;
                attackBattleSoldier[i].transform.position = Vector3.Lerp(attackBattleSoldier[i].transform.position, BATTLE_POS + new Vector3(-2, 0, (i - (count * 0.5f) + 0.5f) * 1.5f), sRate[i] * Time.deltaTime * 60);
            }
            // 攻撃側指揮官
            for (int i = 0; i < attackBattleCommander.Count; i++)
            {
                int count = attackBattleCommander.Count;
                attackBattleCommander[i].transform.position = Vector3.Lerp(attackBattleCommander[i].transform.position, BATTLE_POS + new Vector3(-5, 0, (i - (count * 0.5f) + 0.5f) * 1.5f), cRate[i] * Time.deltaTime * 60);
            }

            power1 = Mathf.Lerp(power1, BattleMoveBox.ButtleResult.AttackBasicCombatPower, 0.2f * Time.deltaTime * 60);
            AttackCombatPowerText.text = Mathf.RoundToInt(power1).ToString();

            // 防衛側
            defenceBattleSoldier.ForEach(s => s.transform.localScale = scale);
            defenceBattleCommander.ForEach(c => c.transform.localScale = scale);

            power2 = Mathf.Lerp(power2, BattleMoveBox.ButtleResult.DefenceBasicCombatPower, 0.2f * Time.deltaTime * 60);
            DefenceCombatPowerText.text = Mathf.RoundToInt(power2).ToString();

            if (Mathf.RoundToInt(power1) == BattleMoveBox.ButtleResult.AttackBasicCombatPower &&
                Mathf.RoundToInt(power2) == BattleMoveBox.ButtleResult.DefenceBasicCombatPower)
            {
                time += Time.deltaTime;

                if (time >= 0.1f) { break; }
            }
            yield return null;
        }

        attackBattleSoldier.ForEach(s => s.Animator.SetBool("SoldierRun", true));
        attackBattleSoldier.ForEach(s => s.Animator.CrossFade("SoldierRun", 0, 0, Random.Range(0f, 1f)));
        attackBattleCommander.ForEach(c => c.Animator.SetBool("CommanderRun", true));
        attackBattleCommander.ForEach(c => c.Animator.CrossFade("CommanderRun", 0, 0, Random.Range(0f, 1f)));

        foreach (var s in attackBattleSoldier)
        {
            s.Animator.SetBool("SoldierRun", false);
            yield return new WaitForSeconds(Random.Range(0.075f, 0.15f));
        }
        foreach (var c in attackBattleCommander)
        {
            c.Animator.SetBool("CommanderRun", false);
            yield return new WaitForSeconds(Random.Range(0.075f, 0.15f));
        }

        yield return null;
        StartCoroutine("State04");
        yield break;
    }

    // あとで消す
    IEnumerator State04()
    {
        StartCoroutine("State05");
        yield break;
    }

    // 攻撃側がサイコロを振って、戦闘力に計上する
    IEnumerator State05()
    {
        // サイコロ画像を配置していく
        for (int i = 0; i < BattleMoveBox.ButtleResult.AttackCommanderDice.Count; i++)
        {
            Image instance = Instantiate(DiceImagePrefab).GetComponent<Image>();
            instance.transform.SetParent(DiceUI.transform);
            instance.transform.localPosition = Vector3.left * 100 - new Vector3(i * 65, 0, 0);
            AttackDiceImageList.Add(instance);
        }

        // サイコロの画像をシャッフルしているように更新する
        float endTime = 0;
        float randomTime = 0;
        while (true)
        {
            endTime += Time.deltaTime;
            randomTime += Time.deltaTime;

            if (endTime >= 3)
            {
                break;
            }

            if (randomTime >= 0.1f)
            {
                randomTime = 0;
                AttackDiceImageList.ForEach(d => d.sprite = DiceImage[Random.Range(0, DiceImage.Length)]);
                yield return null;
            }
        }

        // サイコロの目を更新する
        for (int i = 0; i < BattleMoveBox.ButtleResult.AttackCommanderDice.Count; i++)
        {
            AttackDiceImageList[i].sprite = DiceImage[BattleMoveBox.ButtleResult.AttackCommanderDice[i] - 1];
        }

        attackBattleCommander = attackBattleCommander.OrderBy(a => System.Guid.NewGuid()).ToList();
        foreach (var c in attackBattleCommander)
        {
            c.Animator.SetTrigger("CommanderAttack");
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        // 係数を更新する
        float multiplication = 1.0f;
        float mTime = 0;
        while (true)
        {
            mTime += Time.deltaTime;
            multiplication = Mathf.Lerp(multiplication, 1 + (BattleMoveBox.ButtleResult.AttackCommanderDice.Sum() * 0.1f), 0.1f * Time.deltaTime * 60);
            AttackMultiplicationText.text = "×" + multiplication.ToString("0.0");
            if (mTime >= 1) { break; }
            yield return null;
        }

        // 総合戦闘力を更新する
        float time = 0;
        float power = BattleMoveBox.ButtleResult.AttackBasicCombatPower;
        while (true)
        {
            power = Mathf.Lerp(power, BattleMoveBox.ButtleResult.AttackTotalCombatPower, 0.2f * Time.deltaTime * 60);
            AttackCombatPowerText.text = Mathf.Round(power).ToString();
            AttackCombatPowerText.color = Color.Lerp(AttackCombatPowerText.color, new Color(255, 255, 100, 255) / 255f, 0.2f * Time.deltaTime * 60);

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleResult.AttackTotalCombatPower)
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
        // サイコロ画像を配置していく
        for (int i = 0; i < BattleMoveBox.ButtleResult.DefenceCommanderDice.Count; i++)
        {
            Image instance = Instantiate(DiceImagePrefab).GetComponent<Image>();
            instance.transform.SetParent(DiceUI.transform);
            instance.transform.localPosition = Vector3.right * 100 + new Vector3(i * 65, 0, 0);
            DefenceDiceImageList.Add(instance);
        }

        // サイコロの画像をシャッフルしているように更新する
        float endTime = 0;
        float randomTime = 0;

        while (true)
        {
            endTime += Time.deltaTime;
            randomTime += Time.deltaTime;

            if (endTime >= 3)
            {
                break;
            }

            if (randomTime >= 0.1f)
            {
                randomTime = 0;
                DefenceDiceImageList.ForEach(d => d.sprite = DiceImage[Random.Range(0, DiceImage.Length)]);
                yield return null;
            }
        }

        // サイコロの目を更新する
        for (int i = 0; i < BattleMoveBox.ButtleResult.DefenceCommanderDice.Count; i++)
        {
            DefenceDiceImageList[i].sprite = DiceImage[BattleMoveBox.ButtleResult.DefenceCommanderDice[i] - 1];
        }

        defenceBattleCommander = defenceBattleCommander.OrderBy(a => System.Guid.NewGuid()).ToList();
        foreach (var c in defenceBattleCommander)
        {
            c.Animator.SetTrigger("CommanderAttack");
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        // 係数を更新する
        float multiplication = 1.0f;
        float mTime = 0;
        while (true)
        {
            mTime += Time.deltaTime;
            multiplication = Mathf.Lerp(multiplication, 1 + (BattleMoveBox.ButtleResult.DefenceCommanderDice.Sum() * 0.1f), 0.1f * Time.deltaTime * 60);
            DefenceMultiplicationText.text = multiplication.ToString("0.0") + "×";
            if (mTime >= 1) { break; }
            yield return null;
        }

        // 総合戦闘力を更新する
        float time = 0;
        float power = BattleMoveBox.ButtleResult.DefenceBasicCombatPower;
        while (true)
        {
            power = Mathf.Lerp(power, BattleMoveBox.ButtleResult.DefenceTotalCombatPower, 0.2f * Time.deltaTime * 60);
            DefenceCombatPowerText.text = Mathf.RoundToInt(power).ToString();
            DefenceCombatPowerText.color = Color.Lerp(DefenceCombatPowerText.color, new Color(255, 255, 100, 255) / 255f, 0.2f * Time.deltaTime * 60);

            if (Mathf.RoundToInt(power) == BattleMoveBox.ButtleResult.DefenceTotalCombatPower)
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

    // 勝ったほうのキャラ群が、攻撃アニメーションを再生して負けたほうは爆発して消える
    IEnumerator State07()
    {
        bool attackWin = BattleMoveBox.ButtleResult.AttackTotalCombatPower >= BattleMoveBox.ButtleResult.DefenceTotalCombatPower;

        if (attackWin)
        {
            attackBattleSoldier = attackBattleSoldier.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var s in attackBattleSoldier)
            {
                s.Animator.SetTrigger("SoldierAttack");
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }
        else
        {
            defenceBattleSoldier = defenceBattleSoldier.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var s in defenceBattleSoldier)
            {
                s.Animator.SetTrigger("SoldierAttack");
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }

        float time1 = 0;
        while (true)
        {
            time1 += Time.deltaTime;


            yield return null;
            if (time1 >= 0.5f) { break; }
        }


        if (attackWin)
        {
            defenceBattleSoldier = defenceBattleSoldier.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var s in defenceBattleSoldier)
            {
                Instantiate(ExplosionParticlePrefab, s.transform.position, ExplosionParticlePrefab.transform.rotation);
                Destroy(s.SkinnedMeshRenderer.material);
                Destroy(s.gameObject);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
            }

            defenceBattleCommander = defenceBattleCommander.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var c in defenceBattleCommander)
            {
                Instantiate(ExplosionParticlePrefab, c.transform.position, ExplosionParticlePrefab.transform.rotation);
                Destroy(c.SkinnedMeshRenderer.material);
                Destroy(c.gameObject);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
            }
        }
        else
        {
            attackBattleSoldier = attackBattleSoldier.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var s in attackBattleSoldier)
            {
                Instantiate(ExplosionParticlePrefab, s.transform.position, ExplosionParticlePrefab.transform.rotation);
                Destroy(s.SkinnedMeshRenderer.material);
                Destroy(s.gameObject);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
            }

            attackBattleCommander = attackBattleCommander.OrderBy(a => System.Guid.NewGuid()).ToList();
            foreach (var c in attackBattleCommander)
            {
                Instantiate(ExplosionParticlePrefab, c.transform.position, ExplosionParticlePrefab.transform.rotation);
                Destroy(c.SkinnedMeshRenderer.material);
                Destroy(c.gameObject);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));
            }
        }

        float time2 = 0;
        while (true)
        {
            time2 += Time.deltaTime;

            yield return null;
            if (time2 >= 1) { break; }
        }

        if (attackWin)
        {
            attackBattleSoldier.ForEach(s => Destroy(s.SkinnedMeshRenderer.material));
            attackBattleSoldier.ForEach(s => Destroy(s.gameObject));
            attackBattleCommander.ForEach(c => Destroy(c.SkinnedMeshRenderer.material));
            attackBattleCommander.ForEach(c => Destroy(c.gameObject));
        }
        else
        {
            defenceBattleSoldier.ForEach(s => Destroy(s.SkinnedMeshRenderer.material));
            defenceBattleSoldier.ForEach(s => Destroy(s.gameObject));
            defenceBattleCommander.ForEach(c => Destroy(c.SkinnedMeshRenderer.material));
            defenceBattleCommander.ForEach(c => Destroy(c.gameObject));
        }

        StartCoroutine("State08");
        yield break;
    }

    // 戦闘ウィンドウを閉じる
    IEnumerator State08()
    {
        BattleCamera.SetActive(false);
        BattleCameraBackImage.SetActive(false);
        AttackBackImage.gameObject.SetActive(false);
        DefenceBackImage.gameObject.SetActive(false);
        AttackBackImage.color = Color.clear;
        DefenceBackImage.color = Color.clear;
        AttackCombatPowerText.text = "";
        AttackCombatPowerText.color = Color.white;
        DefenceCombatPowerText.text = "";
        DefenceCombatPowerText.color = Color.white;
        AttackMultiplicationText.text = "";
        DefenceMultiplicationText.text = "";
        AttackDiceImageList.ForEach(d => Destroy(d.gameObject));
        AttackDiceImageList.Clear();
        DefenceDiceImageList.ForEach(d => Destroy(d.gameObject));
        DefenceDiceImageList.Clear();

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
