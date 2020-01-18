using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleResult
{
    // 攻撃側防御側それぞれのキャラの数
    public int AttackSoldier;
    public int AttackCommander;
    public int DefenceSoldier;
    public int DefenceCommander;

    // サイコロの出た目リスト
    public List<int> AttackCommanderDice;
    public List<int> DefenceCommanderDice;
    // 基礎戦闘力(ダイスの出た数を掛け算しない一段階目の数値)
    public int AttackBasicCombatPower;
    public int DefenceBasicCombatPower;
    // 総合戦闘力(ダイスの出た数を掛け算する二段階目の数値)
    public float AttackTotalCombatPower;
    public float DefenceTotalCombatPower;

    public void Battle(MoveBox movebox)
    {
        // 攻撃側防御側それぞれのキャラの数を代入
        AttackSoldier = movebox.Soldier.Count;
        AttackCommander = movebox.Commander.Count;
        DefenceSoldier = movebox.Node2.Soldier.Count;
        DefenceCommander = movebox.Node2.Commander.Count;

        // リストを生成
        AttackCommanderDice = new List<int>();
        DefenceCommanderDice = new List<int>();

        // 基礎戦闘力を計算
        AttackBasicCombatPower = (AttackSoldier * 1000) + (AttackCommander * 1000);
        DefenceBasicCombatPower = (DefenceSoldier * 1000) + (DefenceCommander * 1000);

        // 攻撃側防御側指揮官の数だけ ( 1 ~ 6 ) の目のダイスを振る
        for (int i = 0; i < AttackCommander; i++)
            AttackCommanderDice.Add(Random.Range(1, 7));
        for (int i = 0; i < DefenceCommander; i++)
            DefenceCommanderDice.Add(Random.Range(1, 7));

        // 総合戦闘力を計算 (10出たなら1+(10*0.1)=2倍 5出たなら1+(5*0.1)=1.5倍)
        AttackTotalCombatPower = AttackBasicCombatPower * (1 + AttackCommanderDice.Sum() * 0.1f);
        DefenceTotalCombatPower = DefenceBasicCombatPower * (1 + DefenceCommanderDice.Sum() * 0.1f);
    }
}
