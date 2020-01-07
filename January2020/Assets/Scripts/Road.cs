using UnityEngine;

public class Road : MonoBehaviour
{
    public Node StartNode { get; set; }
    public Node EndNode { get; set; }
    public Vector2 PosS { get; set; }
    public Vector2 PosE { get; set; }
    public bool RemoveFlag { get; set; }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Node")
        {
            Node node = collision.gameObject.GetComponent<Node>();
            
            // 橋をつなげているノード以外と触れていたら、Trueを返す
            if (node != StartNode && node != EndNode)
            {
                GetComponent<Renderer>().material.color = Color.red;
                //return true;
            }
        }

        //return false;
    }
}
