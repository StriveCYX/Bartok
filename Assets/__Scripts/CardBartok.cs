using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CBState包含游戏状态和动作状态
public enum CBState
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    // 这些静态字段用于设置在CardBartok所有实例中都相同的值
    static public float     MOVE_DURATION = 0.5f;
    static public string    MOVE_EASING = Easing.InOut;
    static public float     CARD_HEIGHT = 3.5f;
    static public float     CARD_WIDTH = 2f;

    [Header("Set Dynamically: CardBartok")]
    public CBState  state = CBState.drawpile;

    // 存储纸牌移动和旋转信息的字段
    public List<Vector3>        bezierPts;
    public List<Quaternion>     bezierRots;
    public float                timeStart, timeDuration;
    public int                  eventualSortOrder;
    public string               eventualSortLayer;
    public GameObject           reportFinishTo = null;

    [System.NonSerialized]
    public Player               callbackPlayer = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);
                if (u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                }
                else if (u>=1)
                {
                    uC = 1;
                    if (state == CBState.toHand)        state = CBState.hand;
                    if (state == CBState.toTarget)      state = CBState.toTarget;
                    if (state == CBState.toDrawpile)    state = CBState.drawpile;
                    if (state==CBState.to)              state = CBState.idle;

                    // 移动到最终位置
                    transform.localPosition = bezierPts[bezierPts.Count-1];
                    transform.rotation = bezierRots[bezierPts.Count-1];

                    // TimeStart重置0，这样下次就会被重写
                    timeStart = 0;

                    if (reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallback", this);
                        reportFinishTo = null;
                    }
                    else if(callbackPlayer!=null)
                    {
                        // 如果此处有一个Player回调
                        // 就在Player上调用CBCallback，并把这个CardBartok作为参数传递
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                    else
                    {
                        // 如果无回调，就什么都不做
                    }
                }
                else
                {
                    // 0 <= u < 1, 意味着这是现在的插值
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;

                    if (u>0.5)
                    {
                        SpriteRenderer sRend = spriteRenderers[0];
                        if(sRend.sortingOrder!=eventualSortOrder)
                        {
                            // 跳转到正确位置
                            SetSortOrder(eventualSortOrder);
                        }
                        if(sRend.sortingLayerName!=eventualSortLayer)
                        {
                            // 跳转到正确的排序层
                            SetSortingLayerName(eventualSortLayer);
                        }
                    }
                }

                break;
        }
    }

    // MoveTo告知纸牌插入到一个新的位置并旋转
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        // 为纸牌做新的插值表
        // 位置和旋转将各有两个值
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition); // 当前位置
        bezierPts.Add(ePos);                    // 新的位置

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);     // 当前旋转
        bezierRots.Add(eRot);                   // 新的旋转

        if (timeStart ==0)
        {
            timeStart = Time.time;
        }
        // timeDuration开始总是一样，但可以稍后改变
        timeDuration = MOVE_DURATION;

        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }

    // 让纸牌被单击时做出反应
    override public void OnMouseUpAsButton()
    {
        // 在Bartok单人模式中调用CardClicked方法
        Bartok.S.CardClicked(this);
        // 调用此方法的基本类（Card.cs）版本
        base.OnMouseUpAsButton();
    }
}
