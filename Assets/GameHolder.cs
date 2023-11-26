using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Chess
{
    public char id;
    public float angle;
    public Chess(char id, float angle) {
        // A = mirrorB
        // B = mirrorG
        // C = laserB
        // D = laserG
        // E = Empty
        this.id = id;
        this.angle = angle;
    }
}

public class GameHolder : MonoBehaviour
{
    public GameObject tile;
    Material originalMaterial;
    public GameObject laserB;
    public GameObject laserG;
    public GameObject mirrorB;
    public GameObject mirrorG;
    TileGenerator tileGenerator;
    BoardHolder boardHolder;
    
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    Tuple<int, int> lastSelect;
    bool select = false;

    // Start is called before the first frame update
    void Start()
    {
        // init tile
        tileGenerator = new TileGenerator(tile);
        tileGenerator.generateObjects();
        originalMaterial = tile.GetComponent<Renderer>().material;
        // init game object
        boardHolder = new BoardHolder(
            laserB, laserG, 
            mirrorB, mirrorG
        );
        boardHolder.refresh();
    }

    // Update is called once per frame
    void Update()
    {
        // 在每一幀檢測是否有點擊
        if (Input.GetMouseButtonDown(0)) // 左鍵點擊
        {
            HandleClick();
        }
    }
    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 使用 Raycast 檢測點擊的物體
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider.gameObject 是被點擊到的物體
            GameObject clickedObject = hit.collider.gameObject;
            Tuple<int, int> tilePosition = getTilePosition(clickedObject);
            // 如果棋格是空的話跳過
            if (boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'E' && !select)
                return;
            // 這邊是判斷是否移動棋子
            if (select && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'E') {
                // 判斷是否在周圍
                int distance = Math.Abs(tilePosition.Item1 + tilePosition.Item2 - lastSelect.Item1 - lastSelect.Item2);
                if (distance == 1) {
                    boardHolder.moveChess(lastSelect, tilePosition);
                    tileGenerator.initTaliesMaterial(originalMaterial);
                    boardHolder.refresh();
                }
                return;
            }
            // 重製所有棋格的材質
            tileGenerator.initTaliesMaterial(originalMaterial);
            // 選重的棋格變成紅色
            Material selectMaterial = new Material(Shader.Find("Standard"));
            selectMaterial.color = Color.red;
            Renderer renderer = clickedObject.GetComponent<Renderer>();
            renderer.material = selectMaterial;
            // 如過是發射端的話跳過
            if (boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'C' || 
                boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'D') {
                    return;
                }
            // 周圍能走的棋格變成黃色
            Material enableMaterial = new Material(Shader.Find("Standard"));
            enableMaterial.color = Color.yellow;
            GameObject enableObject;
            Renderer enableRenderer;
            // 上方
            if (tilePosition.Item1 + 1 != rowCount && boardHolder.board[tilePosition.Item1 + 1, tilePosition.Item2].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1 + 1, tilePosition.Item2];
                enableRenderer = enableObject.GetComponent<Renderer>();
                enableRenderer.material = enableMaterial;
            }
            // 下方
            if (tilePosition.Item1 - 1 != -1 && boardHolder.board[tilePosition.Item1 - 1, tilePosition.Item2].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1 - 1, tilePosition.Item2];
                enableRenderer = enableObject.GetComponent<Renderer>();
                enableRenderer.material = enableMaterial;
            }
            // 右方
            if (tilePosition.Item2 + 1 != columnCount && boardHolder.board[tilePosition.Item1, tilePosition.Item2 + 1].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1, tilePosition.Item2 + 1];
                enableRenderer = enableObject.GetComponent<Renderer>();
                enableRenderer.material = enableMaterial;
            }
            // 左方
            if (tilePosition.Item2 - 1 != -1 && boardHolder.board[tilePosition.Item1, tilePosition.Item2 - 1].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1, tilePosition.Item2 - 1];
                enableRenderer = enableObject.GetComponent<Renderer>();
                enableRenderer.material = enableMaterial;
            }
            select = true;
            lastSelect = tilePosition;
        }
    }

    Tuple<int, int> getTilePosition(GameObject tile) {
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (tile.Equals(tileGenerator.cloneTiles[row, col]))
                    return new Tuple<int, int>(row, col);
            }
        }
        return new Tuple<int, int>(-1, -1);
    }
}
