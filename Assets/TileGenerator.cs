using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TileGenerator : MonoBehaviour
{
    GameObject prefab;  // 要生成的素材
    public GameObject[,] cloneTiles = new GameObject[9, 7]; // 生成後的素材
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    float spacing = 100f;  // 物體間的間距

    Canvas canvas;

    public TileGenerator(GameObject prefab, Canvas canvas) {
        this.prefab = prefab;
        this.canvas = canvas;
    }

    public void generateObjects()
    {
        float beginXSet = - (columnCount - 1) / 2 * spacing;
        float beginYSet = (rowCount - 1) / 2 * spacing;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                float xPosition = beginXSet + col * spacing;
                float yPosition = beginYSet - row * spacing;
                Vector3 position = new Vector3(xPosition, yPosition, 0f);
                //cloneTiles[row, col] = Instantiate(prefab, position, Quaternion.identity, canvas.transform);
                cloneTiles[row, col] = Instantiate(prefab, canvas.transform);
                cloneTiles[row, col].transform.localPosition = position;
            }
        }
    }

    public void initTaliesMaterial(Material originalMaterial) {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                cloneTiles[row, col].GetComponent<Renderer>().material = originalMaterial;
            }
        }
    }

    public Tuple<int, int> getTilePosition(GameObject tile) {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (tile.Equals(cloneTiles[row, col]))
                    return new Tuple<int, int>(row, col);
            }
        }
        return new Tuple<int, int>(-1, -1);
    }

    public Tuple<int, int> getChessPosition(GameObject chess) {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (chess.transform.localPosition.x == cloneTiles[row, col].transform.localPosition.x && 
                    chess.transform.localPosition.y == cloneTiles[row, col].transform.localPosition.y)
                return new Tuple<int, int>(row, col);
            }
        }
        return new Tuple<int, int>(-1, -1);
    }
}
