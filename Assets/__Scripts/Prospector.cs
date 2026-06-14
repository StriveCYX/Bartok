using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    //public XDocument    deckXML;
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3  layoutCenter;
    public Vector2  fsPosMid    = new Vector2(0.5f, 0.90f);
    public Vector2  fsPosRun    = new Vector2(0.5f, 0.75f);
    public Vector2  fsPosMid2   = new Vector2(0.4f, 1.0f);
    public Vector2  fsPosEnd    = new Vector2(0.5f, 0.95f);
    public float    reloadDelay = 2f;   //回合有2秒间隔
    public TextMeshProUGUI gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]
    public Deck                 deck;
    public Layout               layout;
    public List<CardProspector> drawPile;
    public Transform            layoutAnchor;
    public CardProspector       target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore        fsRun;

    private void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    // Start is called before the first frame update
    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);   // 本代码执行洗牌任务

        //Card c;
        //for (int cNum = 0; cNum<deck.cards.Count; ++cNum)
        //{
        //    c = deck.cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        layout = GetComponent<Layout>();        // 获取布局
        layout.ReadLayout(layoutXML.text);      // 将LayoutXML传递给脚本
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetUpUITexts()
    {
        // 设置HighScore UI Text
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<TextMeshProUGUI>();

            int highScore = ScoreManager.High_SCORE;
            string hScore = "Hign Score: " + Utils.AddCommasToNumber(highScore);
            //go.GetComponent<TextMeshProUGUI>().text = hScore;
            highScoreText.text = hScore;
        }

        // 设置最后一轮显示的GUITexts

        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<TextMeshProUGUI>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<TextMeshProUGUI>();
        }

        // 使之不可见
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach( Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    // Draw将从drawpile取出一张纸牌并返回
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];    // 取出0号CardProspector
        drawPile.RemoveAt(0);       // 然后从List<> drawPile删除它
        return (cd);    //然后返回它
    }

    // LayoutGame()定位纸牌的初始场景，a.k.a. “矿井”
    void LayoutGame()
    {
        // 创建一个空的游戏对象作为场景
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            // ^在层级面板中创建一个空的名为_LayoutAnchor的游戏对象
            layoutAnchor = tGO.transform;   // 获取Transform
            layoutAnchor.transform.position = layoutCenter;     //定位
        }

        CardProspector cp;
        // 按照布局

        foreach ( SlotDef tSD in layout.slotDefs )
        {
            // ^遍历layout.slotDefs中为tSD的所有SlotDefs
            cp = Draw();        // 从drawPile的顶部（开始）取出一张纸牌
            cp.faceUp = tSD.faceUp;     //设置该纸牌的faceUp为SlotDef中的值
            cp.transform.parent = layoutAnchor;     // 设置它的父元素为layoutAnchor
            // 代替先前的父元素deck.deckAnchor,即场景播放时出现在层级结构中的_Deck
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            // 根据slotDef设置纸牌的localPosition
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            // 画面中的CardProspectors具有CardState.tableau状态
            cp.state = eCardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);      // 设置排序层
            cp.SetSortOrder(tSD.layerID*2);
            tableau.Add(cp);    // Add this CardProspector to the List<> tableau
        }

        // 设置纸牌间如何覆盖隐藏
        foreach (CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // 设置初始目标纸牌
        MoveToTarget(Draw());

        // 设置储备牌
        UpdateDrawPile();
    }

    // 将整型layoutID转换为具有该ID的CardProspector
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            // 遍历 tableau List<> 中所有纸牌
            if (tCP.layoutID == layoutID)
            {
                // 如果纸牌具有相同ID，返回它
                return tCP;
            }
        }

        return null;
    }

    // 纸牌变为朝上或朝下
    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool fup = true;    // 假设纸牌朝上
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // 如果画面中有被盖住的纸牌
                if (cover.state == eCardState.tableau)
                {
                    fup = false;
                }
            }
            cd.faceUp = fup;   // 设置纸牌分数
        }
    }

    // 移动当前目标纸牌到弃牌堆
    void MoveToDiscard(CardProspector cd)
    {
        // 设置纸牌状态为丢弃
        cd.state = eCardState.discard;
        discardPile.Add(cd);    // 添加到discardPile List<>
        cd.transform.parent = layoutAnchor;     // 更新transform父元素

        // 定位到弃牌堆
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        // 放到牌堆顶部用于深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);

    }

    // 使cd成为新的目标牌
    void MoveToTarget(CardProspector cd)
    {
        // 如果当前已有目标牌，则将它移动到弃牌堆
        if (target != null)
            MoveToDiscard(cd);
        target = cd;    // cd成为新的目标牌
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        // 移动到目标位置
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x*layout.discardPile.x,
            layout.multiplier.y*layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true;
        // 设置深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // 排开所有储备牌显示剩余张数
    void UpdateDrawPile()
    {
        CardProspector cd;
        // 遍历所有牌
        for(int i=0;i<drawPile.Count;++i)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // 使用layout.drawPile.stagger精确定位
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x*(layout.drawPile.x+i*dpStagger.x),
                layout.multiplier.y*(layout.drawPile.y+i*dpStagger.y),
                -layout.drawPile.layerID+0.1f*i);
            cd.faceUp = false;  // 使所有牌朝下
            cd.state = eCardState.drawpile;
            // 设置深度排序
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);

        }
    }

    // 在游戏中任何时刻单击纸牌都会调用CardClicked
    public void CardClicked(CardProspector cd)
    {
        // 根据被单击纸牌的状态进行响应
        switch (cd.state)
        {
            case eCardState.target:
                // 单击目标纸牌无响应
                break;

            case eCardState.drawpile:
                // 单击任何储看牌堆将抽出下一张牌
                MoveToDiscard(target);      // 移动目标纸牌到弃牌堆
                MoveToTarget(Draw());       // 将抽出的牌移动为目标纸牌
                UpdateDrawPile();           // 重新储备牌
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;

            case eCardState.tableau:
                // 单击画面中的纸牌将检查是否有效
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    // 如果纸牌向下则无效
                    validMatch = false;
                }
                if (!AdjacentRank(cd,target))
                {
                    // 如果不为相邻点数则无效
                    validMatch = false;
                }
                if (!validMatch)
                    return;    // 无效则返回

                // 执行到这一步，那么：耶！这是一张有效牌
                tableau.Remove(cd);     // 从tableau List移除
                MoveToDiscard(target);  // 我加的，为了不和新的目标牌重叠
                MoveToTarget(cd);   // 使之成为目标牌
                SetTableauFaces();      // 更新朝上的纸牌
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
        }

        // 检查游戏是否结束
        CheckForGameOver();
    }

    // 检查游戏是否结束
    void CheckForGameOver()
    {
        // 如果画面为空，则游戏结束
        if (tableau.Count == 0)
        {
            // 调用 GameOver() 并且结果为赢
            GameOver(true);
            return;
        }

        //如果储备牌堆中仍有牌，则游戏未结束
        if (drawPile.Count > 0)
            return;

        //检查剩余有效可玩纸牌
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                // 如果有可玩纸牌，则游戏未结束
                return;
            }
        }

        // 没有可玩纸牌，则游戏结束
        // 调用 GameOver 并且结束为输
        GameOver(false);
    }

    // 游戏结束时调用。仅用于此处，但可扩展
    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null)
            score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            //print("Game Over. You won! :)");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else 
        {
            gameOverText.text = "Game Over";
            if ( ScoreManager.High_SCORE <= score)
            {
                string str = "You got the high score! \nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            print("Game Over. You lost. :(");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }

        // 重新加载场景，重置游戏
        //SceneManager.LoadScene("__Prospector_Scene_0");

        // 在reloadDelay时间内重新加载场景
        // 定义分数环绕屏幕的时刻
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        // 重新加载场景，重置游戏
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    // 如果2张牌为相邻点数则返回true（包括A&K）
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // 如果有纸牌朝下，则不相邻
        if (!c0.faceUp || !c1.faceUp) return (false);

        // 如果只差1个点数，则相邻
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
            return (true);

        // 如果一个为A，一个为K，则相邻
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank ==13 && c1.rank == 1) return (true);

        // 否则返回false
        return (false);
    }

    // 处理FloatingScore行为
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // 无论抽牌、赢或输都需要响应相同的动作
            case eScoreEvent.draw:      // 抽牌
            case eScoreEvent.gameWin:   // 赢
            case eScoreEvent.gameLoss:  // 输
                
                // 将fsRun添加到Scoreboard分数
                if(fsRun != null)
                {
                    // 创建贝济埃曲线的坐标点
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.Init(fsPts, 0, 1);
                    // 同时调整fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;   // 清除fsRun以再次创建

                }
                break;
            case eScoreEvent.mine:  // 移除矿井纸牌

                // 为当前分数创建FloatingScore
                FloatingScore fs;
                // 从mosePosition移动到fsPosRun;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }
}