using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Scoreboard类管理向玩家展示的分数
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;     // Scoreboard单例

    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore;

    [Header("Set in Inspector")]
    [SerializeField] private int _score = 0;
    [SerializeField] public string _scoreString;
    private Transform   canvasTrans;

    // score属性也可以设置scoreString
    public int score
    {
        get 
        { 
            return _score; 
        }
        set
        {
            _score = value;
            _scoreString = _score.ToString("NO");
        }
    }

    // scoreString属性也可以设置Text.text
    public string scoreString
    {
        get
        {
            return _scoreString;
        }
        set
        {
            _scoreString = value;
            GetComponent<TextMeshProUGUI>().text = _scoreString;
        }
    }

    private void Awake()
    {
        if (S == null)
        {
            S = this;   // 设置私有单例
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake():S is already set");
        }
        canvasTrans = transform.parent;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 当被SendMessage调用时，将fs.score加到this.score上
    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    // 实例化一个新的FloatingScore游戏对象并初始化。它返回一个FloatingScore创建的
    // 指针，这样调用函数可以完成更多的功能（如设置fontSizes等）
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;    // 设置fs为回调的当前对象
        fs.Init(pts);
        return fs;
    }
}
