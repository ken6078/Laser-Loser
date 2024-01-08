using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameTutorialScript : MonoBehaviour
{
    GameObject[] tiles = new GameObject[9];
    GameObject mirrorG;
    public Slider angleSlider;
    // Start is called before the first frame update
    void Start()
    {
        // init angleSlider
        angleSlider = GameObject.Find("angleSlider").GetComponent<Slider>();
        angleSlider.onValueChanged.AddListener(onSliderValueChanged);
        
        mirrorG = GameObject.Find("mirrorG");

        for (int i = 0; i < 9; i++)
        {
            tiles[i] = GameObject.Find("tile" + (i+1));
        }
        SetTileColor(tiles[6], Color.red);
        SetTileColor(tiles[3], Color.yellow);
        SetTileColor(tiles[7], Color.yellow);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetTileColor(GameObject tile, Color color)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        Material selectMaterial = renderer.material;
        selectMaterial.color = color;
        renderer.material = selectMaterial;
    }
    void onSliderValueChanged(float value)
    {
        Vector3 localEulerAngles = tiles[6].transform.localEulerAngles;
        localEulerAngles.z = value;
        mirrorG.transform.localEulerAngles = localEulerAngles;
    }
}
