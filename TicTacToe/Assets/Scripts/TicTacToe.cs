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
    public AudioSource scoreReachedAudioSource; // ���������ڲ��Ż��ִﵽ 5 ʱ����Ч
    public AudioClip scoreReachedAudioClip; // ���������ִﵽ 5 ʱ����Ч�ļ�
    public AudioSource scoreReachedTenAudioSource; // ���������ڲ��Ż��ִﵽ 10 ʱ����Ч
    public AudioClip scoreReachedTenAudioClip; // ���������ִﵽ 10 ʱ����Ч�ļ�
    public Image newImage; // ���������������µ�ͼƬ

    private string[] board;
    private bool isPlayerTurn;
    private string playerSymbol = "X";
    private string aiSymbol = "O";
    private float randomMoveProbabilityLevel1 = 0.8f;//������ӵĸ��ʣ���AI�Ѷȱ�ѡΪ1��ʱ�򣬻���80%�����������ʣ��20%����ʹ�õ���minimax
    private float randomMoveProbabilityLevel2 = 0.1f;//��AI�Ѷȱ�ѡΪ2��ʱ�򣬻���10%����������ӣ�ʣ��90%����ʹ�õ���minimax
    private float currentRandomMoveProbability;
    private int score = 0;

    void Start()
    {
        InitializeBoard();
        isPlayerTurn = true;
        resultText.text = "";
        UpdateScoreText();//���·���text

        restartButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        difficultyLevel1Button.gameObject.SetActive(true);
        difficultyLevel2Button.gameObject.SetActive(true);
        HideCells();
        newImage.gameObject.SetActive(false);   // ��ʼ����ͼƬΪ����״̬

        difficultyLevel1Button.onClick.AddListener(() => SelectDifficulty(randomMoveProbabilityLevel1));
        difficultyLevel2Button.onClick.AddListener(() => SelectDifficulty(randomMoveProbabilityLevel2));
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        scoreReachedAudioSource.clip = scoreReachedAudioClip;
        scoreReachedTenAudioSource.clip = scoreReachedTenAudioClip; // ��ʼ������Ч
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
                resultText.text = "ŷ�ὴӮ�ˣ�";
                UpdateScore(1);
                DisableCells();
                ShowRestartAndQuitButtons();
            }
            else if (IsBoardFull())
            {
                resultText.text = "ƽ�֣�";
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

    void AITurn()//AI�߶�
    {
        int move;
        if (Random.value < currentRandomMoveProbability)
        {
            move = GetRandomMove();//�������
        }
        else
        {
            move = FindBestMove();//��������
        }

        board[move] = aiSymbol;
        cells[move].GetComponentInChildren<Text>().text = aiSymbol;
        if (CheckWin(aiSymbol))
        {
            resultText.text = "������Ӯ��Ŷ��";
            UpdateScore(-1);
            DisableCells();
            ShowRestartAndQuitButtons();
        }
        else if (IsBoardFull())
        {
            resultText.text = "ƽ�֣�";
            DisableCells();
            ShowRestartAndQuitButtons();
        }
        else
        {
            isPlayerTurn = true;
        }
    }

    int GetRandomMove()//��������ӵ�
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

    int FindBestMove()//���������ӵ�
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

    int Minimax(string[] board, int depth, bool isMaximizing)//Minimax����������С�����������㷨��������ȱ�������Ҷ�ӽڵ���ݼ�������ֵ��ȷ�������ж���
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

    bool CheckWin(string symbol)//����ʤ�����򣬼������ţ�б������ͬ������ΪӮ��
    {
        // �����
        for (int i = 0; i < 9; i += 3)
        {
            if (board[i] == symbol && board[i + 1] == symbol && board[i + 2] == symbol)
            {
                return true;
            }
        }
        // �����
        for (int i = 0; i < 3; i++)
        {
            if (board[i] == symbol && board[i + 3] == symbol && board[i + 6] == symbol)
            {
                return true;
            }
        }
        // ���Խ���
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

    bool IsBoardFull()//������������ж���
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
        restartButton.gameObject.SetActive(false);//���¿�ʼ��Ϸ��ť����
        quitButton.gameObject.SetActive(false);//�˳���ʼ��Ϸ��ť����
        EnableCells();
        UpdateScoreText();
        newImage.gameObject.SetActive(false); // ���¿�ʼ��Ϸʱ������ͼƬ
    }

    void ShowRestartAndQuitButtons()
    {
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    void HideCells()//���ظ���
    {
        foreach (Button cell in cells)
        {
            cell.gameObject.SetActive(false);
        }
    }

    void ShowCells()//��ʾ����
    {
        foreach (Button cell in cells)
        {
            cell.gameObject.SetActive(true);
            cell.interactable = true;
        }
    }

    void SelectDifficulty(float probability)//�Ѷ�ѡ��ť
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
        if (oldScore < 10 && score == 10) // �������Ƿ��С�� 10 �ﵽ 10
        {
            scoreReachedTenAudioSource.Play(); // ���Ż��ִﵽ 10 ʱ����Ч
            newImage.gameObject.SetActive(true); // ��ʾ��ͼƬ
        }
        else if (score < 10)
        {
            newImage.gameObject.SetActive(false); // ����С�� 10 ʱ������ͼƬ
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "����: " + score;
    }
}