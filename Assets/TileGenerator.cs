using UnityEngine;
using UnityEngine.PlayerLoop;

public class TileGenerator : MonoBehaviour
{
    GameObject prefab;  // 要生成的素材
    public GameObject[,] cloneTiles = new GameObject[9, 7]; // 生成後的素材
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    float spacing = 0.75f;  // 物體間的間距

    public TileGenerator(GameObject prefab) {
        this.prefab = prefab;
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
                cloneTiles[row, col] = Instantiate(prefab, position, Quaternion.identity);
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
}
