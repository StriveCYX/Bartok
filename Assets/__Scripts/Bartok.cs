using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 此枚举包含一个游戏轮转的不同阶段
public enum TurnPhase
{
    //idle,       // 等待玩家输入
    //pre,        // 玩家已选择一张牌，等待动画完成
    //waiting,    // 等待其他玩家出牌
    //post,       // 其他玩家已出牌，等待动画完成
    //gameOver    // 游戏结束
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour
{
    static public Bartok S;
    static public Player CURRENT_PLAYER;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;

    [Header("Set in Dynamically")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;
    public List<Player> players;
    public CardBartok targetCard;

    public TurnPhase phase = TurnPhase.idle;

    public BartokLayout layout;
    public Transform layoutAnchor;

    private void Awake()
    {
        S = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        deck = GetComponent<Deck>();    // 获取Deck值
        deck.InitDeck(deckXML.text);    // 传递DeckXML值给它
        Deck.Shuffle(ref deck.cards);   // 重置deck

        layout = GetComponent<BartokLayout>();  // 获取Layout
        if (layout is null)
        {
            print("layout is null");
        }
        layout.ReadLayout(layoutXML.text);      // 传递LayoutXML给它

        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();
    }

    // Update is called once per frame
    // 此Update方法用于测试给玩家添加纸牌
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        players[0].AddCard(Draw());
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        players[1].AddCard(Draw());
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        players[2].AddCard(Draw());
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        players[3].AddCard(Draw());
    //    }
    //}

    List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD)
        {
            lCB.Add(tCD as CardBartok);
        }

        return (lCB);
    }

    // 在drawPile里正确定位所有的牌
    public void ArrangeDrawPile()
    {
        CardBartok tCB;

        for (int i = 0; i < drawPile.Count; ++i)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;

            // 旋转应该从0开始
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB.state = CBState.drawpile;
        }
    }
    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0];    // 取出0号CardProspector

        if (drawPile.Count == 0)     // 如果目标牌堆为空
        {
            // 将弃牌堆洗牌并放入目标牌堆
            int ndx;
            while(discardPile.Count > 0)
            {
                // 从弃牌堆中随机选择一张牌，并将它添加到目标牌堆中
                ndx = Random.Range(0, discardPile.Count);
                drawPile.Add(discardPile[ndx]);
                discardPile.RemoveAt(ndx);
            }
            ArrangeDrawPile();
            // 显示移动到目标牌堆的纸牌
            float t = Time.time;
            foreach( CardBartok tCB in drawPile)
            {
                tCB.transform.localPosition = layout.discardPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.02f;
                tCB.state = CBState.toDrawpile;
                tCB.eventualSortLayer = "0";
            }
        }
        
        drawPile.RemoveAt(0);       // 然后从List<> drawPile删除它
        return (cd);    //然后返回它
    }

    // 执行初始游戏布局
    void LayoutGame()
    {
        // 创建空的GameObject作为画面的锚点
        if (layoutAnchor is null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        // 定位drawPile的牌
        ArrangeDrawPile();

        // 设置玩家
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = tSD;
            players.Add(pl);
            pl.playerNum = players.Count;
        }
        players[0].type = PlayerType.human;    // 构建第0个真人玩家

        CardBartok tCB;
        // 给每位玩家发 numStartingCards 张牌
        for (int i = 0; i < numStartingCards; ++i)
        {
            for (int j = 0; j < players.Count; ++j)
            {
                tCB = Draw();   //抽一张牌
                // 稍微错开抽牌的时间
                tCB.timeStart = Time.time + drawTimeStagger * (i * players.Count + j);

                players[(j + 1) % players.Count].AddCard(tCB);
            }
        }

        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * players.Count + 4));
    }

    public void DrawFirstTarget()
    {
        // 从牌堆中抽出一张牌作为目标牌
        CardBartok tCB = MoveToTarget(Draw());
        // 完成时，在此Bartok上设置CardBartok，用来调用CBCallback
        tCB.reportFinishTo = this.gameObject;
    }

    // 最后一张牌开始处理时将使用此回调
    public void CBCallback(CardBartok cb)
    {
        // 你有时希望就像这样条用报告方法
        Utils.tr("Bartok.CBCallback()", cb.name);

        StartGame();    // 开始游戏
    }

    public void StartGame()
    {
        // 真人玩家左边的玩家先出牌
        // player[0]是玩家
        PassTurn(1);
    }

    public void PassTurn(int num = -1)
    {
        // 如果没有号码传入，就选择下一位玩家
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % players.Count;
        }
        
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;

            // 检查Game Over，弃牌需要重新洗牌
            if(CheckGameOver())
            {
                return;
            }
        }

        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

         CURRENT_PLAYER.TakeTurn();
        // 报告轮转传递
        Utils.tr("Bartok.PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
    }

    public bool CheckGameOver()
    {
        // 判断是否需要将弃牌重新洗入牌堆中
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile)
            {
                cards.Add(cb);
            }

            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardsList(cards);

            ArrangeDrawPile();
        }
        
        // 检查当前玩家是否取胜
        if(CURRENT_PLAYER.hand.Count == 0)
        {
            // 当前玩家获胜！
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);

            return true;
        }

        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("__Bartok_Scene_0");
    }

    // ValidPlay验证所选的纸牌可以放入弃牌堆
    public bool ValidPlay(CardBartok cb)
    {
        // 如果rank是相同的，它就是一个有效操作
        if (cb.rank == targetCard.rank)
        {
            Utils.tr("Bartok.ValidPlay()", "cb: " + cb.name, "targetCard: " + targetCard.name,"cb.rank: "+cb.rank, "targetCard.rank: "+ targetCard.rank);
            return true;
        }

        // 如果花色(suit)是相同的，它就是一个有效操作
        if (cb.suit == targetCard.suit)
        {
            return true;
        }

        // 否则，返回false
        return false;
    }


    // 使另一张牌成为目标牌
    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
        {
            MoveToDiscard(targetCard);
        }

        targetCard = tCB;

        return tCB;
    }

    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return tCB;
    }

    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human)
            return;
        if (phase == TurnPhase.waiting)
            return;
        
        switch(tCB.state)
        {
            case CBState.drawpile:
                // 抓取顶部的牌，不一定是单击的那张牌
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr("Bartok.CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                // 检查纸牌是否有效
                if(ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("Bartok: CardClicked()", "Play", tCB.name, targetCard.name+" is target");
                    phase = TurnPhase.waiting;
                }
                else
                {
                    // 忽略，但忘记玩家操作
                    Utils.tr("Bartok:CardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
                }
                break;
        }
    }
}
