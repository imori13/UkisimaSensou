using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ノード間を移動するときに生成する、移動ボックス
public class MoveBox : MonoBehaviour
{
    public MapManager Map { get; set; }
    public PlayerEnum PlayerEnum { get; set; }
    public Node ParentNode { get; set; }
    public Node MoveNode { get; set; }

    public Node DestinationNode { get; set; } = null;  // 最終的な目的地

    public List<Commander> Commander { get; private set; } = new List<Commander>();
    public List<Soldier> Soldier { get; private set; } = new List<Soldier>();

    static readonly float speed = 5;

    void Start()
    {

    }

    void FixedUpdate()
    {
        // 線形補完を応用したイージング処理で移動させる
        transform.position = Vector3.Lerp(transform.position, MoveNode.transform.position, 0.05f);

        // ノードに近くなったら終了
        if (Vector3.SqrMagnitude(transform.position - MoveNode.transform.position) <= (0.5f * 0.5f))
        {
            Arrival();
            Destroy(gameObject);
        }
    }

    // 到着した用メソッド
    void Arrival()
    {
        // お互いのノードの移動許可フラグを再びONにする
        ParentNode.MovePermission = true;
        MoveNode.MovePermission = true;

        // 生成時にMoveBoxの数も足す目的のリストから外す
        if (ParentNode.HeadingMovebox.Contains(this))
            ParentNode.HeadingMovebox.Remove(this);
        if (MoveNode.HeadingMovebox.Contains(this))
            MoveNode.HeadingMovebox.Remove(this);

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

        return (combatpower01 >= combatpower02);
    }

    // 勝利時
    void AttackWin()
    {
        // もし相手のノードが本拠地なら
        if (MoveNode.IsBaseNode)
        {
            MoveNode.IsBaseNode = false;
            Map.PlayerBaseNode[(int)MoveNode.PlayerEnum] = null;
            Map.MapNode.Where(n => n.PlayerEnum == MoveNode.PlayerEnum).ToList().ForEach(n =>
            {
                n.Commander.ForEach(c => Destroy(c.gameObject));
                n.Commander.Clear();
                n.Soldier.ForEach(s => Destroy(s.gameObject));
                n.Soldier.Clear();
                MoveBox heading = null;
                n.HeadingMovebox.Where(h => h.PlayerEnum == n.PlayerEnum).ToList().ForEach(h =>
                {
                    heading = h;
                    Destroy(h.gameObject);
                    n.HeadingMovebox.Remove(h);
                    h.ParentNode.MovePermission = true;
                    h.MoveNode.MovePermission = true;
                });
                n.ConnectNode.ForEach(c => c.HeadingMovebox.RemoveAll(h => c.HeadingMovebox.Contains(heading)));
                n.PlayerEnum = PlayerEnum.None;
            });
        }

        // 相手の指揮官を消して自分の兵士を指揮官を相手に移動させる
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

        // 親を更新
        MoveNode.Commander.ForEach(c => c.UpdateNode(MoveNode));
        MoveNode.Soldier.ForEach(c => c.UpdateNode(MoveNode));

        // ノードの色をすべて更新
        Map.AllUpdateNodeColor();
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