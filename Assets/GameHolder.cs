using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Text titleText;
    public Slider angleSlider;
    public Button okButton;
    TileGenerator tileGenerator;
    BoardHolder boardHolder;
    
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    Tuple<int, int> lastSelect;
    bool select = false;
    GameObject selectChess;
    int round = 1;

    // Start is called before the first frame update
    void Start()
    {
        // init angleSlider
        angleSlider.onValueChanged.AddListener(onSliderValueChanged);
        angleSlider.interactable = false;
        // init button
        okButton.onClick.AddListener(onOkButtonClick);
        okButton.interactable = false;
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
        titleText.text = $"Player {round} Round";
    }

    // Update is called once per frame
    void Update()
    {
        // 在每一幀檢測是否有點擊
        if (Input.GetMouseButtonDown(0)) // 左鍵點擊
        {
            HandleClick();
            if (select) {
                angleSlider.interactable = true;
                okButton.interactable = true;
            }
            else {
                angleSlider.interactable = false;
                okButton.interactable = false;
            }
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
            Tuple<int, int> tilePosition = tileGenerator.getTilePosition(clickedObject);
            // 如果棋格是空的話跳過
            if (boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'E' && !select)
                return;
            // 這邊是判斷是否移動棋子
            if (select && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'E') {
                // 判斷是否在周圍
                int distanceY = Math.Abs(tilePosition.Item1 - lastSelect.Item1);
                int distanceX = Math.Abs(tilePosition.Item2 - lastSelect.Item2);
                if ((distanceX == 0 && distanceY == 1) || (distanceX == 1 && distanceY == 0)) {
                    // 移動棋子
                    boardHolder.moveChess(lastSelect, tilePosition);
                    // 更改玩家回合
                    nextRound();
                    titleText.text = $"Player {round} Round";
                }
                return;
            }
            // 重製所有棋格的材質
            tileGenerator.initTaliesMaterial(originalMaterial);
            boardHolder.refresh();
            // 選重的棋格變成紅色
            Material selectMaterial = new Material(Shader.Find("Standard"));
            selectMaterial.color = Color.red;
            Renderer renderer = clickedObject.GetComponent<Renderer>();
            renderer.material = selectMaterial;
            // 調整拉桿角度
            selectChess = boardHolder.returnChess(clickedObject);
            Tuple<int, int> chessPosition = tileGenerator.getChessPosition(selectChess);
            if (boardHolder.board[chessPosition.Item1, chessPosition.Item2].id == 'C') 
                angleSlider.value = boardHolder.returnChess(clickedObject).transform.localEulerAngles.z - 180;
            else
                angleSlider.value = boardHolder.returnChess(clickedObject).transform.localEulerAngles.z;
            // 如果不是正確的玩家就跳過
            if (round == 1 && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'B') {
                select = false;
                return;
            }
            if (round == 2 && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'A') {
                select = false;
                return;
            }
            if (round == 2 && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'C') {
                select = false;
                return;
            }
            if (round == 1 && boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'D') {
                select = false;
                return;
            }
            // 如過是發射端的話只允許旋轉
            if (boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'C' || 
                boardHolder.board[tilePosition.Item1, tilePosition.Item2].id == 'D') {
                    select = true;
                    lastSelect = new Tuple<int, int>(-1, -1);
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

    void onSliderValueChanged(float value) {
        Vector3 localEulerAngles = selectChess.transform.localEulerAngles;
        Tuple<int, int> chessPosition = tileGenerator.getChessPosition(selectChess);
        // 藍色雷射光源
        if (boardHolder.board[chessPosition.Item1, chessPosition.Item2].id == 'C') 
            localEulerAngles[2] = value+180;
        else
            localEulerAngles[2] = value;
        selectChess.transform.localEulerAngles = localEulerAngles;
    }

    void onOkButtonClick() {
        Tuple<int, int> chessPosition = tileGenerator.getChessPosition(selectChess);
        // 儲存旋轉的棋子
        if (boardHolder.board[chessPosition.Item1, chessPosition.Item2].id == 'C' ||
            boardHolder.board[chessPosition.Item1, chessPosition.Item2].id == 'D') 
            boardHolder.board[chessPosition.Item1, chessPosition.Item2].angle = selectChess.transform.localEulerAngles.z+45;
        else
            boardHolder.board[chessPosition.Item1, chessPosition.Item2].angle = selectChess.transform.localEulerAngles.z;
        // 變更回合
        nextRound();
    }
    
    void nextRound() {
        select = false;
        angleSlider.interactable = false;
        okButton.interactable = false;
        // 重製所有棋格的材質
        tileGenerator.initTaliesMaterial(originalMaterial);
        // 更新頁面
        boardHolder.refresh();
        if (round == 1)
            round = 2;
        else
            round = 1;
    }
}
