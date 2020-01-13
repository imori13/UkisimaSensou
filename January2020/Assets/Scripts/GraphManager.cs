using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    // prefab
    [SerializeField] Image GraphImage;

    public MapManager MapManager { get; private set; }

    List<Image> graphs = new List<Image>();

    void Start()
    {
        MapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
    }

    void Update()
    {

    }
}
