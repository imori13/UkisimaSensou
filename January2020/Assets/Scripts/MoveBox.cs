using System.Collections.Generic;
using UnityEngine;

// ノード間を移動するときに生成する、移動ボックス
class MoveBox : MonoBehaviour
{
    public PlayerEnum PlayerEnum { get; set; }
    public Node MoveNode { get; set; }

    public List<Commander> Commander { get; private set; } = new List<Commander>();
    public List<Soldier> Soldier { get; private set; } = new List<Soldier>();

    static readonly float speed = 5f;

    void Start()
    {

    }

    void Update()
    {
        // 現在地から目標地への方向を求める
        Vector3 direction = MoveNode.transform.position - transform.position;
        // 移動する(方向を正規化してスピードをかけて)
        transform.position += direction.normalized * speed * Time.deltaTime;

        // ノードに近くなったら終了
        if (direction.sqrMagnitude <= (0.5f*0.5f))
        {
            Arrival();
            Destroy(gameObject);
        }
    }

    // 到着した用メソッド
    void Arrival()
    {
        // 移動先が同じプレイヤーなら移動
        if (MoveNode.PlayerEnum == PlayerEnum)
            Move();
        // 移動先が違うプレイヤーなら攻撃
        else
            Attack();
    }

    // 移動
    void Move()
    {
        // 指揮官がいた場合にも、移動させる
        MoveNode.Commander.AddRange(Commander);
        Commander.Clear();
        // 兵士を移動させる
        MoveNode.Soldier.AddRange(Soldier);
        Soldier.Clear();

        // 親を更新
        MoveNode.Commander.ForEach(c => c.UpdateNode(MoveNode));
        MoveNode.Soldier.ForEach(c => c.UpdateNode(MoveNode));
    }

    // 攻撃
    void Attack()
    {
        if (Battle())
            AttackWin();
        else
            AttackLose();
    }

    bool Battle()
    {
        float character_count01 = Commander.Count + Soldier.Count;
        float combatpower01 = character_count01 * (Commander.Count + 1);

        float character_count02 = MoveNode.Commander.Count + MoveNode.Soldier.Count;
        float combatpower02 = character_count02 * (MoveNode.Commander.Count + 1);


        Debug.Log(PlayerEnum.ToString() + "戦闘力:" + combatpower01 + " VS " + MoveNode.PlayerEnum.ToString() + "戦闘力:" + combatpower02);
        Debug.Log((combatpower01 > combatpower02) ? ("侵略成功！") : ("侵略失敗"));

        return (combatpower01 > combatpower02);
    }

    // 勝利時
    void AttackWin()
    {
        // 相手の指揮官を消して自分の兵士を指揮官させる
        MoveNode.Commander.ForEach(c => Destroy(c.gameObject));
        MoveNode.Commander.Clear();
        MoveNode.Commander.AddRange(Commander);
        Commander.Clear();

        // 自分の兵士を１人消す
        Destroy(Soldier[0].gameObject);
        Soldier.RemoveAt(0);
        // 相手の兵士を消して自分の兵士を移動させる
        MoveNode.Soldier.ForEach(s => Destroy(s.gameObject));
        MoveNode.Soldier.Clear();
        MoveNode.Soldier.AddRange(Soldier);
        Soldier.Clear();

        // 相手のノードの占有者を変える
        MoveNode.PlayerEnum = PlayerEnum;

        // ノードの色を更新
        MoveNode.UpdateNodeColor();
        MoveNode.Renderer.material.color = MoveNode.Normal_Color;
        MoveNode.Renderer.material.color += MoveNode.Normal_Color / 4f;

        // 親を更新
        MoveNode.Commander.ForEach(c => c.UpdateNode(MoveNode));
        MoveNode.Soldier.ForEach(c => c.UpdateNode(MoveNode));
    }

    // 敗北時
    void AttackLose()
    {
        // 全部消滅
        Commander.ForEach(c => Destroy(c.gameObject));
        Commander.Clear();
        Soldier.ForEach(s => Destroy(s.gameObject));
        Soldier.Clear();
    }
}