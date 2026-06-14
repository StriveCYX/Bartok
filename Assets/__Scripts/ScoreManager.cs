using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// 用于处理所有可能的得分事件的枚举
public enum eScoreEvent
{
    draw,
    mine,
    gameWin,
    gameLoss,
}

// ScoreManager处理所有得分
public class ScoreManager : MonoBehaviour
{
    static public ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int High_SCORE = 0;

    [Header("Set Dynamically")]
    // 记录得分信息的变量
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }

    private void Awake()
    {
        if (S == null)
            S = this;   //设置私有单例
        else
            Debug.LogError("ERROR: ScoreManager.Awake(): S is already set!");

        // 确认PlayerPrefs中的高分值
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            High_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        // 将分数添加到上一轮，如果赢的话分数>0
        score += SCORE_FROM_PREV_ROUND;
        // 并重置SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static public void EVENT(eScoreEvent evt)
    {
        try    // try-catch语句防止停止运行程序的错误
        {
            S.Event(evt);
        }
        catch (System.NullReferenceException nre) 
        {
            Debug.LogError("Scor eManager:Event() called while S=null.\n" + nre);
        }
    }

    // ScoreManager处理所有得分
    void Event (eScoreEvent evt)
    {
        switch (evt)
        {
            //无论是抽牌、赢或输，需要有对应动作
            case eScoreEvent.draw:      // 抽一张牌
            case eScoreEvent.gameWin:   // 赢得本轮
            case eScoreEvent.gameLoss:  // 本轮输了
                chain = 0;  // 重置分数变量
                score += scoreRun;  // 将scoreRun加入总得分
                scoreRun = 0;
                break;
            case eScoreEvent.mine:  // 删除一张矿井纸牌
                ++chain;    //分数变量chain自加
                scoreRun += chain;  // 添加当前纸牌的分数到这回合
                break;
        }

        // 第二个switch语句处理本轮的输赢
        switch (evt)
        {
            case eScoreEvent.gameWin:
                // 赢的话，将分数添加到下一轮
                // 基于SceneManager.LoadLevel(),无需重置静态变量
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;

            case eScoreEvent.gameLoss:
                // 输的话，与最高分进行比较
                if(High_SCORE <= score)
                {
                    print("you got high score! High score: " + score);
                    High_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }
                break;
        }
    }
}
