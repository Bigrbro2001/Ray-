using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TicTacToe : MonoBehaviour
{
    public Button[] cells;
    public Text resultText;
    public Button restartButton;
    public Button difficultyLevel1Button;
    public Button difficultyLevel2Button;
    public Button quitButton;
    public Text scoreText;
    public AudioSource scoreReachedAudioSource; // 新增：用于播放积分达到 5 时的音效
    public AudioClip scoreReachedAudioClip; // 新增：积分达到 5 时的音效文件
    public AudioSource scoreReachedTenAudioSource; // 新增：用于播放积分达到 10 时的音效
    public AudioClip scoreReachedTenAudioClip; // 新增：积分达到 10 时的音效文件
    public Image newImage; // 新增：用于引用新的图片

    private string[] board;
    private bool isPlayerTurn;
    private string playerSymbol = "X";
    private string aiSymbol = "O";
    private float randomMoveProbabilityLevel1 = 0.8f;//随机落子的概率，当AI难度被选为1的时候，会有80%概率随机落子剩下20%概率使用的是minimax
    private float randomMoveProbabilityLevel2 = 0.1f;//当AI难度被选为2的时候，会有10%概率随机落子，剩下90%概率使用的是minimax
    private float currentRandomMoveProbability;
    private int score = 0;

    void Start()
    {
        InitializeBoard();
        isPlayerTurn = true;
        resultText.text = "";
        UpdateScoreText();//更新分数text

        restartButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        difficultyLevel1Button.gameObject.SetActive(true);
        difficultyLevel2Button.gameObject.SetActive(true);
        HideCells();
        newImage.gameObject.SetActive(false);   // 初始化新图片为隐藏状态

        difficultyLevel1Button.onClick.AddListener(() => SelectDifficulty(randomMoveProbabilityLevel1));
        difficultyLevel2Button.onClick.AddListener(() => SelectDifficulty(randomMoveProbabilityLevel2));
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        scoreReachedAudioSource.clip = scoreReachedAudioClip;
        scoreReachedTenAudioSource.clip = scoreReachedTenAudioClip; // 初始化新音效
    }

    void InitializeBoard()
    {
        board = new string[9];
        for (int i = 0; i < 9; i++)
        {
            board[i] = "";
            int index = i;
            cells[i].onClick.AddListener(() => OnCellClick(index));
            cells[i].GetComponentInChildren<Text>().text = "";
        }
    }

    void OnCellClick(int index)
    {
        if (isPlayerTurn && board[index] == "")
        {
            board[index] = playerSymbol;
            cells[index].GetComponentInChildren<Text>().text = playerSymbol;
            if (CheckWin(playerSymbol))
            {
                resultText.text = "欧尼酱赢了！";
                UpdateScore(1);
                DisableCells();
                ShowRestartAndQuitButtons();
            }
            else if (IsBoardFull())
            {
                resultText.text = "平局！";
                DisableCells();
                ShowRestartAndQuitButtons();
            }
            else
            {
                isPlayerTurn = false;
                AITurn();
            }
        }
    }

    void AITurn()//AI走动
    {
        int move;
        if (Random.value < currentRandomMoveProbability)
        {
            move = GetRandomMove();//随机落子
        }
        else
        {
            move = FindBestMove();//最优落子
        }

        board[move] = aiSymbol;
        cells[move].GetComponentInChildren<Text>().text = aiSymbol;
        if (CheckWin(aiSymbol))
        {
            resultText.text = "妹妹我赢了哦！";
            UpdateScore(-1);
            DisableCells();
            ShowRestartAndQuitButtons();
        }
        else if (IsBoardFull())
        {
            resultText.text = "平局！";
            DisableCells();
            ShowRestartAndQuitButtons();
        }
        else
        {
            isPlayerTurn = true;
        }
    }

    int GetRandomMove()//找随机落子点
    {
        List<int> availableMoves = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == "")
            {
                availableMoves.Add(i);
            }
        }
        return availableMoves[Random.Range(0, availableMoves.Count)];
    }

    int FindBestMove()//找最优落子点
    {
        int bestScore = int.MinValue;
        int bestMove = -1;
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == "")
            {
                board[i] = aiSymbol;
                int score = Minimax(board, 0, false);
                board[i] = "";
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }
        return bestMove;
    }

    int Minimax(string[] board, int depth, bool isMaximizing)//Minimax最大化收益和最小化别人收益算法，深度优先遍历，从叶子节点回溯计算最优值来确定最优行动。
    {
        if (CheckWin(aiSymbol))
        {
            return 1;
        }
        else if (CheckWin(playerSymbol))
        {
            return -1;
        }
        else if (IsBoardFull())
        {
            return 0;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == "")
                {
                    board[i] = aiSymbol;
                    int score = Minimax(board, depth + 1, false);
                    board[i] = "";
                    bestScore = Mathf.Max(score, bestScore);
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == "")
                {
                    board[i] = playerSymbol;
                    int score = Minimax(board, depth + 1, true);
                    board[i] = "";
                    bestScore = Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }
    }

    bool CheckWin(string symbol)//核心胜利规则，检查横竖着，斜都是相同内容视为赢。
    {
        // 检查行
        for (int i = 0; i < 9; i += 3)
        {
            if (board[i] == symbol && board[i + 1] == symbol && board[i + 2] == symbol)
            {
                return true;
            }
        }
        // 检查列
        for (int i = 0; i < 3; i++)
        {
            if (board[i] == symbol && board[i + 3] == symbol && board[i + 6] == symbol)
            {
                return true;
            }
        }
        // 检查对角线
        if (board[0] == symbol && board[4] == symbol && board[8] == symbol)
        {
            return true;
        }
        if (board[2] == symbol && board[4] == symbol && board[6] == symbol)
        {
            return true;
        }
        return false;
    }

    bool IsBoardFull()//棋子满格结束判定。
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == "")
            {
                return false;
            }
        }
        return true;
    }

    void DisableCells()
    {
        foreach (Button cell in cells)
        {
            cell.interactable = false;
        }
    }

    void RestartGame()
    {
        InitializeBoard();
        isPlayerTurn = true;
        resultText.text = "";
        restartButton.gameObject.SetActive(false);//重新开始游戏按钮隐藏
        quitButton.gameObject.SetActive(false);//退出开始游戏按钮隐藏
        EnableCells();
        UpdateScoreText();
        newImage.gameObject.SetActive(false); // 重新开始游戏时隐藏新图片
    }

    void ShowRestartAndQuitButtons()
    {
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    void HideCells()//隐藏格子
    {
        foreach (Button cell in cells)
        {
            cell.gameObject.SetActive(false);
        }
    }

    void ShowCells()//显示格子
    {
        foreach (Button cell in cells)
        {
            cell.gameObject.SetActive(true);
            cell.interactable = true;
        }
    }

    void SelectDifficulty(float probability)//难度选择按钮
    {
        currentRandomMoveProbability = probability;
        difficultyLevel1Button.gameObject.SetActive(false);
        difficultyLevel2Button.gameObject.SetActive(false);
        ShowCells();
    }

    void EnableCells()
    {
        foreach (Button cell in cells)
        {
            cell.interactable = true;
            cell.GetComponentInChildren<Text>().text = "";
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void UpdateScore(int change)
    {
        int oldScore = score;
        score = Mathf.Clamp(score + change, 0, 10);
        UpdateScoreText();

        if (oldScore < 5 && score == 5)
        {
            scoreReachedAudioSource.Play();
        }
        if (oldScore < 10 && score == 10) // 检查积分是否从小于 10 达到 10
        {
            scoreReachedTenAudioSource.Play(); // 播放积分达到 10 时的音效
            newImage.gameObject.SetActive(true); // 显示新图片
        }
        else if (score < 10)
        {
            newImage.gameObject.SetActive(false); // 积分小于 10 时隐藏新图片
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "积分: " + score;
    }
}