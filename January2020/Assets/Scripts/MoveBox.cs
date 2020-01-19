using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ノード間を移動するときに生成する、移動ボックス
public class MoveBox : MonoBehaviour
{
    static readonly float speed = 5;

    public MapManager Map { get; set; }
    public GameManager GameManager { get; set; }
    public BattleWindowManager BattleWindowManager { get; set; }
    public PlayerEnum PlayerEnum { get; set; }
    public Node Node1 { get; set; }
    public Node Node2 { get; set; }
    public Node DestNode { get; set; }

    public List<Commander> Commander { get; private set; } = new List<Commander>();
    public List<Soldier> Soldier { get; private set; } = new List<Soldier>();

    public BattleResult ButtleResult { get; private set; }

    void Start()
    {
        Soldier.ForEach(s => s.Animator.SetBool("SoldierRun", true));
        Soldier.ForEach(s => s.Animator.CrossFade("SoldierRun", 0, 0, Random.Range(0f, 1f)));
    }

    void Update()
    {
        // 線形補完を応用したイージング処理で移動させる
        transform.position = Vector3.Lerp(transform.position, Node2.transform.position, 0.05f * MyTime.time);

        if (BattleWindowManager.BattleMoveBox != null) return;
        if (Vector3.SqrMagnitude(transform.position - Node2.transform.position) <= (0.5f * 0.5f))
        {
            Arrival();
        }
    }

    // 到着した用メソッド
    void Arrival()
    {
        Node1.UpdateNodeColor();
        Node2.UpdateNodeColor();

        // 生成時にMoveBoxの数も足す目的のリストから外す
        if (Node1.HeadingMovebox.Contains(this))
            Node1.HeadingMovebox.Remove(this);
        if (Node2.HeadingMovebox.Contains(this))
            Node2.HeadingMovebox.Remove(this);

        Soldier.ForEach(s => s.Animator.SetBool("SoldierRun", false));
        Soldier.ForEach(s => s.Animator.CrossFade("SoldierWait", 0, 0, Random.Range(0f, 1f)));

        // 移動先が同じプレイヤーなら移動
        if (Node2.PlayerEnum == PlayerEnum)
            Move();
        // 移動先が違うプレイヤーなら攻撃
        else
            Attack();
    }

    // 移動
    void Move()
    {
        // 指揮官がいた場合にも、移動させる
        Node2.Commander.AddRange(Commander);
        Commander.Clear();
        // 兵士を移動させる
        Node2.Soldier.AddRange(Soldier);
        Soldier.Clear();

        // 親を更新
        Node2.Commander.ForEach(c => c.UpdateNode(Node2));
        Node2.Soldier.ForEach(c => c.UpdateNode(Node2));

        // お互いのノードの移動許可フラグを再びONにする
        Node2.MovePermission = true;

        // GameManagerのMove関数を呼び出す
        List<Node> SearchNode = GameManager.SearchRoute(Node2, DestNode);
        if (DestNode != null && SearchNode != null)
            GameManager.Move(SearchNode[0], SearchNode[1], DestNode);

        // MoveBoxを削除
        Destroy(gameObject);
    }

    // 攻撃
    void Attack()
    {
        ButtleResult = new BattleResult();
        ButtleResult.Battle(this);

        // 相手が無所属でないかつ、もしプレイヤーと関係のあるバトルだった場合
        if (Node2.PlayerEnum != PlayerEnum.None && (Node1.PlayerEnum == PlayerEnum.Player01 || Node2.PlayerEnum == PlayerEnum.Player01))
        {
            BattleWindowManager.Initialize(this);
            return;
        }

        if (ButtleResult.AttackTotalCombatPower >= ButtleResult.DefenceTotalCombatPower)
            AttackWin();
        else
            AttackLose();
    }

    public void AttackHoge()
    {
        if (ButtleResult.AttackTotalCombatPower >= ButtleResult.DefenceTotalCombatPower)
            AttackWin();
        else
            AttackLose();
    }

    // 勝利時
    void AttackWin()
    {
        // もし相手のノードが本拠地なら
        if (Node2.IsBaseNode)
        {
            Node2.IsBaseNode = false;
            Map.PlayerBaseNode[(int)Node2.PlayerEnum] = null;
            Map.MapNode.Where(n => n.PlayerEnum == Node2.PlayerEnum).ToList().ForEach(n =>
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
                    h.Node1.MovePermission = true;
                    h.Node2.MovePermission = true;
                });
                n.ConnectNode.ForEach(c => c.HeadingMovebox.RemoveAll(h => c.HeadingMovebox.Contains(heading)));
                n.PlayerEnum = PlayerEnum.None;
            });
        }

        // 相手の指揮官を消して自分の兵士を指揮官を相手に移動させる
        Node2.Commander.ForEach(c => Destroy(c.gameObject));
        Node2.Commander.Clear();
        Node2.Commander.AddRange(Commander);
        Commander.Clear();

        // 自分の兵士を１人消す
        Destroy(Soldier[0].gameObject);
        Soldier.RemoveAt(0);
        // 相手の兵士を消して自分の兵士を移動させる
        Node2.Soldier.ForEach(s => Destroy(s.gameObject));
        Node2.Soldier.Clear();
        Node2.Soldier.AddRange(Soldier);
        Soldier.Clear();

        // 相手のノードの占有者を変える
        Node2.PlayerEnum = PlayerEnum;

        // 親を更新
        Node2.Commander.ForEach(c => c.UpdateNode(Node2));
        Node2.Soldier.ForEach(c => c.UpdateNode(Node2));

        // ノードの色をすべて更新
        Map.AllUpdateNodeColor();

        // お互いのノードの移動許可フラグを再びONにする
        Node2.MovePermission = true;

        // MoveBoxを削除
        Destroy(gameObject);
    }

    // 敗北時
    void AttackLose()
    {
        // 全部消滅
        Commander.ForEach(c => Destroy(c.gameObject));
        Commander.Clear();
        Soldier.ForEach(s => Destroy(s.gameObject));
        Soldier.Clear();

        // お互いのノードの移動許可フラグを再びONにする
        Node2.MovePermission = true;

        // MoveBoxを削除
        Destroy(gameObject);
    }
}