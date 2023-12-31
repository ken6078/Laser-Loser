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
    public Text timerText;
    public Slider angleSlider;
    public Button okButton;
    public LayerMask mirrorLayer;
    public Canvas gameCanvas;
    public LineRenderer laser;
    TileGenerator tileGenerator;
    BoardHolder boardHolder;
    
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    Tuple<int, int> lastSelect;
    bool select = false;
    GameObject selectChess;
    int round = 1;
    int laserMaxReflex = 10000;
    bool gameFinish = false;
    int roundTimeSecond = 30;
    float roundTime;

    // Start is called before the first frame update
    void Start()
    {
        // init angleSlider
        angleSlider.onValueChanged.AddListener(onSliderValueChanged);
        angleSlider.interactable = false;
        // init laser
        // laser = gameObject.AddComponent<LineRenderer>();
        laser.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        laser.positionCount = laserMaxReflex;
        laser.startWidth = 0.01f;
        laser.endWidth = 0.01f;
        laser.startColor = Color.red;
        laser.endColor = Color.red;
        // init button
        okButton.onClick.AddListener(onOkButtonClick);
        okButton.interactable = false;
        // init tile
        tileGenerator = new TileGenerator(tile, gameCanvas);
        tileGenerator.generateObjects();
        originalMaterial = tile.GetComponent<Renderer>().material;
        // init game object
        boardHolder = new BoardHolder(
            gameCanvas,
            laserB, laserG, 
            mirrorB, mirrorG
        );
        boardHolder.refresh();
        titleText.text = $"Player {round} Round";
        roundTime = 0;
        timerText.text = $"{roundTimeSecond} Second";
    }

    // Update is called once per frame
    void Update()
    {
        if (gameFinish)
            return;
        // 計時器
        roundTime += Time.deltaTime;
        int lastTime = roundTimeSecond - (int) roundTime;
        timerText.text = $"{lastTime} Second";
        if (lastTime <= 0) {
            StartCoroutine(nextRound());
            roundTime = 0;
        }
        // 在每一幀檢測是否有點擊
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0) // 左鍵點擊
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
        Vector3 clickPos;
        if (Input.GetMouseButtonDown(0))
            clickPos = Input.mousePosition;
        else
            clickPos = Input.GetTouch(0).position;

        Ray ray = Camera.main.ScreenPointToRay(clickPos);
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
                    StartCoroutine(nextRound());
                }
                return;
            }
            // 重製所有棋格的材質
            tileGenerator.initTaliesMaterial(originalMaterial);
            boardHolder.refresh();
            // 選重的棋格變成紅色
            Renderer renderer = clickedObject.GetComponent<Renderer>();
            Material selectMaterial = renderer.material;
            selectMaterial.color = Color.red;
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
            GameObject enableObject;
            Renderer enableRenderer;
            // 上方
            if (tilePosition.Item1 + 1 != rowCount && boardHolder.board[tilePosition.Item1 + 1, tilePosition.Item2].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1 + 1, tilePosition.Item2];
                enableRenderer = enableObject.GetComponent<Renderer>();
                Material enableMaterial = enableRenderer.material;
                enableMaterial.color = Color.yellow;
                enableRenderer.material = enableMaterial;
            }
            // 下方
            if (tilePosition.Item1 - 1 != -1 && boardHolder.board[tilePosition.Item1 - 1, tilePosition.Item2].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1 - 1, tilePosition.Item2];
                enableRenderer = enableObject.GetComponent<Renderer>();
                Material enableMaterial = enableRenderer.material;
                enableMaterial.color = Color.yellow;
                enableRenderer.material = enableMaterial;
            }
            // 右方
            if (tilePosition.Item2 + 1 != columnCount && boardHolder.board[tilePosition.Item1, tilePosition.Item2 + 1].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1, tilePosition.Item2 + 1];
                enableRenderer = enableObject.GetComponent<Renderer>();
                Material enableMaterial = enableRenderer.material;
                enableMaterial.color = Color.yellow;
                enableRenderer.material = enableMaterial;
            }
            // 左方
            if (tilePosition.Item2 - 1 != -1 && boardHolder.board[tilePosition.Item1, tilePosition.Item2 - 1].id == 'E') {
                enableObject = tileGenerator.cloneTiles[tilePosition.Item1, tilePosition.Item2 - 1];
                enableRenderer = enableObject.GetComponent<Renderer>();
                Material enableMaterial = enableRenderer.material;
                enableMaterial.color = Color.yellow;
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
        StartCoroutine(nextRound());
    }
    
    IEnumerator nextRound() {
        roundTime = 0;
        // 更新頁面
        boardHolder.refresh();
        // 在下一幀執行剩下內容 (等物件更新)
        yield return new WaitForSeconds(0.05f);
        // 射雷射
        float initialAngle;
        Vector3 origin;
        Vector3 direction;
        if (round == 1) {
            initialAngle = boardHolder.board[0, 0].angle;
            direction = Quaternion.Euler(0, 0, initialAngle) * Vector3.right;
            origin = boardHolder.cloneLaserB.transform.localPosition + direction * 55f;
        }
        else{
            initialAngle = boardHolder.board[8, 6].angle;
            direction = Quaternion.Euler(0, 0, initialAngle) * Vector3.right;
            origin = boardHolder.cloneLaserG.transform.localPosition + direction * 55f;
        }

        // 从特定点以特定角度发射光线
        Ray2D ray = new Ray2D(origin, direction);

        // 发射光线并进行反射
        ReflectRay(ray);
        if (gameFinish)
            yield break;

        select = false;
        angleSlider.interactable = false;
        okButton.interactable = false;
        // 重製所有棋格的材質
        tileGenerator.initTaliesMaterial(originalMaterial);
        if (round == 1)
            round = 2;
        else
            round = 1;
        titleText.text = $"Player {round} Round";
    }

    void ReflectRay(Ray2D ray, int times = 0)
    {
        if (times >= laserMaxReflex) return;
        Vector3 worldPosition = gameCanvas.transform.TransformPoint(ray.origin);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, ray.direction, Mathf.Infinity, mirrorLayer);

        if (hit.collider != null)
        {
            Debug.Log($"{times} Hit");
            print($"ray.origin {ray.origin}");
            print($"worldPosition {worldPosition}");
            // 在这里处理光线碰到镜子的情况
            Vector2 reflectionDirection = Vector2.Reflect(ray.direction, hit.normal);
            // 畫線
            Debug.DrawRay(ray.origin, ray.direction*hit.distance, Color.red, 10f);
            laser.SetPosition(times, ray.origin);
            Vector2 canvasPosition = WorldToUI(gameCanvas.GetComponent<RectTransform>(), hit.point);
            print($"hit.point {hit.point}");
            print($"canvasPosition {canvasPosition}");
            for (int i = times+1; i < laserMaxReflex; i++)
                laser.SetPosition(i, canvasPosition);
            // 處理碰到雷射光源&勝利
            if (round == 1 && hit.collider.gameObject.Equals(boardHolder.cloneLaserG)) {
                titleText.text = "Player 1 WIN!";
                gameFinish = true;
                return;
            }
            if (round == 2 && hit.collider.gameObject.Equals(boardHolder.cloneLaserB)) {
                titleText.text = "Player 2 WIN!";
                gameFinish = true;
                return;
            }
            if (hit.collider.gameObject.Equals(boardHolder.cloneLaserG) ||
                hit.collider.gameObject.Equals(boardHolder.cloneLaserB)) {
                    return;
                }
            // 继续反射, 偷跑一小段以免困在物體內部
            Ray2D reflectedRay = new Ray2D(canvasPosition+reflectionDirection*1f, reflectionDirection);
            

            ReflectRay(reflectedRay, times+1);
        }
        else
        {
            Debug.Log($"{times} Not Hit");
            // 在这里处理光线没有碰到镜子的情况
            laser.SetPosition(times, ray.origin);
            for (int i = times+1; i < laserMaxReflex; i++)
                laser.SetPosition(i, ray.origin+ray.direction*5000f);
            Debug.DrawRay(ray.origin, ray.direction*50f, Color.green, 10f);
        }
    }
    static public Vector2 WorldToUI(RectTransform r, Vector3 pos)
    {
        Vector2 screenPos = Camera.main.WorldToViewportPoint(pos); //世界物件在螢幕上的座標，螢幕左下角為(0,0)，右上角為(1,1)
        Vector2 viewPos = (screenPos - r.pivot) * 2; //世界物件在螢幕上轉換為UI的座標，UI的Pivot point預設是(0.5, 0.5)，這邊把座標原點置中，並讓一個單位從0.5改為1
        float width = r.rect.width / 2; //UI一半的寬，因為原點在中心
        float height = r.rect.height / 2; //UI一半的高
        return new Vector2(viewPos.x * width, viewPos.y * height); //回傳UI座標
    }
}
