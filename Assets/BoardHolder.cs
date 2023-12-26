using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BoardHolder : MonoBehaviour
{
    GameObject laserB;
    public GameObject cloneLaserB;
    GameObject laserG;
    public GameObject cloneLaserG;
    GameObject mirrorB;
    GameObject[] cloneMirrorB = new GameObject[10];
    GameObject mirrorG;
    GameObject[] cloneMirrorG = new GameObject[10];
    int rowCount = 9;    // 行數
    int columnCount = 7; // 列數
    float spacing = 100f;  // 物體間的間距
    public Chess[,] board = new Chess[9, 7];

    public BoardHolder(
        Canvas canvas,
        GameObject laserB, GameObject laserG, 
        GameObject mirrorB, GameObject mirrorG
    ) {
        this.laserB = laserB;
        this.laserG = laserG;
        this.mirrorB = mirrorB;
        this.mirrorG = mirrorG;
        // init gameObject
        cloneLaserB = Instantiate(laserB, canvas.transform);
        cloneLaserG = Instantiate(laserG, canvas.transform);
        for (int i = 0; i < 10; i++) {
            cloneMirrorB[i] = Instantiate(mirrorB, canvas.transform);
            cloneMirrorG[i] = Instantiate(mirrorG, canvas.transform);
        }

        // init board
        for (int row = 0; row < rowCount; row++) {
            for (int col = 0; col < columnCount; col++) {
                board[row, col] = new Chess('E', 0f);
            }
        }
        // put laser
        board[0, 0].id = 'C';
        board[0, 0].angle = 0f;
        board[8, 6].id = 'D';
        board[8, 6].angle = 180f;
        // random mirror
        int count = 0;
        while (count != 20) {
            int row = UnityEngine.Random.Range(0, rowCount);
            int col = UnityEngine.Random.Range(0, columnCount);
            if (board[row, col].id == 'E') {
                count++;
                board[row, col].id = count > 10 ? 'A' : 'B';
                board[row, col].angle = UnityEngine.Random.Range(0, 180);
            }
        }
    }

    public void refresh() {
        int BCount = 0;
        int GCount = 0;
        for (int row = 0; row < rowCount; row++) {
            for (int col = 0; col < columnCount; col++) {
                if (board[row, col].id == 'A') {
                    cloneMirrorB[BCount].transform.localPosition = position(row, col);
                    cloneMirrorB[BCount++].transform.rotation = Quaternion.Euler(0f, 0f, board[row, col].angle);
                } else if (board[row, col].id == 'B') {
                    cloneMirrorG[GCount].transform.localPosition = position(row, col);
                    cloneMirrorG[GCount++].transform.rotation = Quaternion.Euler(0f, 0f, board[row, col].angle);
                } else if (board[row, col].id == 'C') {
                    cloneLaserB.transform.localPosition = position(row, col);
                    cloneLaserB.transform.rotation = Quaternion.Euler(0f, 0f, board[row, col].angle-45);
                } else if (board[row, col].id == 'D') {
                    cloneLaserG.transform.localPosition = position(row, col);
                    cloneLaserG.transform.rotation = Quaternion.Euler(0f, 0f, board[row, col].angle-45);
                }
            }
        }
    }

    public Vector3 position (int row, int col) {
        float beginXSet = - (columnCount - 1) / 2 * spacing;
        float beginYSet = (rowCount - 1) / 2 * spacing;
        float xPosition = beginXSet + col * spacing;
        float yPosition = beginYSet - row * spacing;
        return new Vector3(xPosition, yPosition, 0f);
    }

    public void moveChess(Tuple<int, int> source, Tuple<int, int> destination) {
        if (board[destination.Item1, destination.Item2].id != 'E') return;
        if (board[source.Item1, source.Item2].id == 'E') return;
        board[destination.Item1, destination.Item2] = board[source.Item1, source.Item2];
        board[source.Item1, source.Item2] = new Chess('E', 0f);
    }

    public GameObject returnChess(GameObject tile) {
        if (cloneLaserB.transform.localPosition.x == tile.transform.localPosition.x && 
            cloneLaserB.transform.localPosition.y == tile.transform.localPosition.y)
            return cloneLaserB;
        if (cloneLaserG.transform.localPosition.x == tile.transform.localPosition.x && 
            cloneLaserG.transform.localPosition.y == tile.transform.localPosition.y)
            return cloneLaserG;
        foreach (GameObject item in cloneMirrorB)
        {
            Debug.Log(item.transform.localPosition.x);
            if (item.transform.localPosition.x == tile.transform.localPosition.x && 
            item.transform.localPosition.y == tile.transform.localPosition.y)
            return item;
        }
        foreach (GameObject item in cloneMirrorG)
        {
            if (item.transform.localPosition.x == tile.transform.localPosition.x && 
            item.transform.localPosition.y == tile.transform.localPosition.y)
            return item;
        }
        
        return new GameObject();
    }

    public void rotateBoard() {
        for (int row = 0; row < rowCount/2; row++) {
            for (int col = 0; col < columnCount; col++) {
                swapChess((row, col), (rowCount-row-1, columnCount-col-1));
            }
        }
        if (rowCount%2 == 1) {
            for (int col = 0; col < columnCount/2; col++)
                swapChess((rowCount/2, col), (rowCount/2, columnCount-col-1));
        }
    }

    public void swapChess((int, int) chess1, (int, int) chess2) {
        (board[chess1.Item1, chess1.Item2], board[chess2.Item1, chess2.Item2]) = (board[chess2.Item1, chess2.Item2], board[chess1.Item1, chess1.Item2]);
    }
}
