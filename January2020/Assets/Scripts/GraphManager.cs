using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    // prefab
    [SerializeField] GameObject GraphImage_Prefab;

    public MapManager MapManager { get; private set; }
    public GameManager GameManager { get; private set; }

    public List<GraphImage> Graphs { get; private set; } = new List<GraphImage>();

    void Start()
    {
        MapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        for (int i = 0; i < MapManager.PlayerCount; i++)
        {
            GraphImage instance = Instantiate(GraphImage_Prefab).GetComponent<GraphImage>();
            instance.transform.position += Vector3.up * i * -40;
            instance.transform.SetParent(transform, false);

            instance.PlayerEnum = (PlayerEnum)i;

            Graphs.Add(instance);
        }
    }

    void Update()
    {
        if (GameManager.IsStart)
        {
            // プレイヤーの最大領土を調べる
            int MaxCount = 0;
            int MinCount = 0;
            for (int i = 0; i < Graphs.Count; i++)
            {
                int count = MapManager.MapNode.Where(n => n.PlayerEnum == Graphs[i].PlayerEnum).Count();
                Graphs[i].GetComponent<GraphImage>().NodeCount = count;
                MaxCount = (count > MaxCount) ? (count) : (MaxCount);
                MinCount = (count < MaxCount) ? (count) : (MinCount);
            }

            Graphs = Graphs.OrderBy(s => -s.NodeCount).ToList();

            for (int i = 0; i < Graphs.Count; i++)
            {
                int count = MapManager.MapNode.Where(n => n.PlayerEnum == Graphs[i].PlayerEnum).Count();

                float MAXWIDTH = 400;
                float MINWIDTH = 0;

                float rate = count / (float)MaxCount;

                float destWidth = Mathf.Lerp(MINWIDTH, MAXWIDTH, rate);
                float destYPos = -40 * i;

                Graphs[i].Image.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(Graphs[i].Image.rectTransform.sizeDelta.x, destWidth, 0.025f * Time.deltaTime * 60), Graphs[i].Image.rectTransform.sizeDelta.y);
                Graphs[i].Image.transform.localPosition =
                    Vector3.Lerp(Graphs[i].Image.transform.localPosition,
                    new Vector3(Graphs[i].Image.transform.localPosition.x, destYPos, Graphs[i].Image.transform.localPosition.z),
                    0.025f * Time.deltaTime * 60);
            }
        }
    }
}
