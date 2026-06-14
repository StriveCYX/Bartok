using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 用于记录FloatingScore所有状态的枚举
public enum eFSState
{
    idle,
    pre,
    active,
    post
}

// FloatingScore可以在屏幕上沿着贝济埃曲线移动
public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    // score属性页可设置_score和scoreString
    public int score
    {
        get
        {
            return _score;
        }
        set // 看不懂
        {
            _score = value;
            scoreString = _score.ToString("N0");    // "N0" 为num添加逗号
            // 为ToString格式查找 "C# Standard Numeric Format Strings"
            GetComponent<TextMeshProUGUI>().text = scoreString;
        }
    }

    public List<Vector2>    bezierPts;  // 用于移动的贝济埃坐标
    public List<float>  fontSizes;  // 用于字体缩放的贝济埃坐标
    public float timeStart = -1f;
    public float timeDuration = -1f;
    public string easingCurve = Easing.InOut;   // 使用Utills.cs的Easing

    // 移动完成时游戏对象将接收SendMessage
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private TextMeshProUGUI txt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 如果没有移动，则返回
        if (state == eFSState.idle)
            return;

        // 从当前时间和持续时间计算u，u的范围为0到1（通常）
        float u = (Time.time - timeStart) / timeDuration;
        // 使用Utils的Easing类描绘u值曲线图
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)      // 如果u<0，那么还不能移动
        {
            state = eFSState.pre;
            txt.enabled = false;    // 隐藏初始得分
        }
        else
        {
            if (u >= 1)     // 如果u>=1，已完成移动
            {
                uC = 1;     // 设置uC=1，避免越界溢出
                state = eFSState.post;
                if(reportFinishTo != null)  // 如果有回调GameObject
                {
                    // ...就使用SendMessage调用FSCallback方法，并带this参数
                    reportFinishTo.SendMessage("FSCallback", this);
                    // 消息发送后，销毁当前游戏对象
                    Destroy(gameObject);
                }
                else   // 如果没有回调
                {
                    // 不销毁当前游戏对象，仅保持
                    state = eFSState.idle;
                }
            }
            else
            {
                // 0<=u<1 代表当前对象有效且正在移动
                state = eFSState.active;
                txt.enabled = true;     // 再次显示得分
            }
            // 使用贝济埃曲线将当前对象移动到正确坐标
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            // RectTransform用于UI对象定位整个屏幕所处位置
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                // 如果fontSizes有值
                // 那么调整GUIText的fontSize
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<TextMeshProUGUI>().fontSize = size;
            }
        }
    }

    // 设置FloatingScore移动
    // 注意默认参数eTimeS & eTimeD的使用
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<TextMeshProUGUI>();

        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1)    // 如果只有一个坐标
        {
            // 只运行至此
            transform.position = ePts[0];
            return;
        }

        // 如果eTimeS为默认值，就从当前时间开始
        if (eTimeS == 0)
            eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre;    // 设置为pre state，准备好开始移动
    }

    public void FSCallback(FloatingScore fs)
    {
        // 当SendMessage调用这个callback时，从参数FloatingScore获得要加的分数
        score += fs.score;
    }
}
