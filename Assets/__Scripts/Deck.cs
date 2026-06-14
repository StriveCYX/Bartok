using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Xml.Linq;
using System.Linq;

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool     startFaceUp = false;
    // 花色
    public Sprite   suitClub;       // 梅花的Sprite
    public Sprite   suitDiamond;    // 方片的Sprite
    public Sprite   suitHeart;      // 红桃的Sprite
    public Sprite   suitSpade;      // 黑桃的Sprite
    public Sprite[] faceSprites;    // 花牌的Sprite
    public Sprite[] rankSprites;    // 点数的Sprite
    public Sprite   cardBack;       // 普通纸牌背面的Sprite
    public Sprite   cardBackGold;   // 金色纸牌背面的Sprite
    public Sprite   cardFront;      // 普通纸牌正面的背景Sprite
    public Sprite   cardFrontGold;  // 金色纸牌正面的背景Sprite
    // 预设
    public GameObject prefabSprite;
    public GameObject prefabCard;

    [Header("Set Dynamically")]
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    public XDocument xmlr;

    // 当Prospector脚本运行时，将调用这里的InitDeck函数
    public void InitDeck(string deckXMLText)
    {
        // 以下语句为层级面板中的所有Card游戏对象创建一个锚点
        if(null==GameObject.Find("_Deck"))
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        // 使用所有必需的Sprite初始化SuitSpites字典
        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C",suitClub},
            { "D",suitDiamond},
            { "H",suitHeart},
            { "S",suitSpade},
        };

        ReadDeck(deckXMLText);
        MakeCards();
    }

    // 当ReadDeck函数将传入的XML文件解析为CardDefinition类的实例
    public void ReadDeck(string deckXMLText)
    {
        xmlr = XDocument.Parse(deckXMLText);
        XElement root = xmlr.Element("xml");

        // 这里将输出一条测试语句，演示xmlr如何使用
        //string s = "xml[0] decorator[0] ";
        //s += "type="+root.Element("decorator").Attribute("type").Value;
        //s += " x=" + root.Element("decorator").Attribute("x").Value;
        //s += " y=" + root.Element("decorator").Attribute("y").Value;
        //s += " scale=" + root.Element("decorator").Attribute("scale").Value;
        //print(s);

        // 读取所有纸牌的角码(Decorator)
        decorators = new List<Decorator>();     // 初始化一个Decorator对象列表
        // 从XML文件中获取所有<decorator>标签，构成一个PT_XMLHashList列表    ？
        // PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"]

        Decorator deco;
        var findDecorator = from e in root.Elements("decorator")
                            select e;
        foreach (var decorator in findDecorator) 
        {
            // 对于XML中每一个<decorator>
            deco = new Decorator();     // 创建一个新的Decorator对象
            // 将<decorator>标签中的所有属性复制给该Decorator对象
            deco.type = decorator.Attribute("type").Value;
            // 当flip属性文本为1时，deco.flip变量值为true
            deco.flip = ("1" == decorator.Attribute("flip").Value);
            // 浮点数需要从属性字符串中解析出来
            deco.scale = float.Parse(decorator.Attribute("scale").Value);
            // 3D向量loc已初始化为[0,0,0]，我们只需要修改其值
            deco.loc.x = float.Parse(decorator.Attribute("x").Value);
            deco.loc.y = float.Parse(decorator.Attribute("y").Value);
            deco.loc.z = float.Parse(decorator.Attribute("z").Value);
            // 将临时变量deco添加到由角码构成的List
            decorators.Add(deco);
        }

        // 读取每种点数对应的花色符号位置
        cardDefs = new List<CardDefinition>();
        // 初始化由CardDefinition构成的List
        // 从XML文件中获取所有<card>标签，构成一个PT_XMLHashList列表
        // PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"]

        var findCard = from e in root.Elements("card")
                       select e;
        foreach (var card in findCard)
        {
            // 对于每个<card>标签
            // 创建一个新的CardDefinition变量cDef
            CardDefinition cDef = new CardDefinition();
            // 解析其属性值并添加到cDef中
            cDef.rank = int.Parse(card.Attribute("rank").Value);
            // 获取当前<card>标签中所有的<pip>标签，构成一个PT_XMLHashList列表 ？
            // PT_XMLHashList xPips = xCardDefs[i]["pip"];

            var findPip = from e in card.Elements("pip")
                          select e;
            foreach (var pip in findPip)
            {
                // 遍历所有的<pip>标签
                deco = new Decorator();
                // 通过Decorator类处理<card>中的<pip>标签
                deco.type = "pip";
                deco.flip = ("1" == pip.Attribute("flip").Value);
                deco.loc.x = float.Parse(pip.Attribute("x").Value);
                deco.loc.y = float.Parse(pip.Attribute("y").Value);
                deco.loc.z = float.Parse(pip.Attribute("z").Value);
                if (null != pip.Attribute("scale"))
                {
                    deco.scale = float.Parse(pip.Attribute("scale").Value);
                }

                cDef.pips.Add(deco);
            }
            // 花牌（J、Q、K）包含一个face属性
            if (null != card.Attribute("face"))
            {
                cDef.face = card.Attribute("face").Value;
            }
            cardDefs.Add(cDef);
        }


    }

    // 根据点数(1~13分别代表纸牌的A~K)获取对应的CardDefinition（牌面布局定义）
    public CardDefinition GetCardDefinitionByRank(int rnk)
    { 
        // 搜索所有的CardDefinition
        foreach(CardDefinition cd in cardDefs)
        {
            //如果点数正确，则返回相应的定义
            if (cd.rank == rnk)
                return (cd);
        }

        return null;
    }

    // 创建Card游戏对象
    public void MakeCards()
    {
        // List型变量cardNames中是要创建的纸牌名称
        // 每种花色均包含1到13的点数（例如黑桃为C1到C13）
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach(string s in letters)
        {
            for(int i = 0; i < 13; ++i)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        // 创建一个List，用于存储所有的纸牌
        cards = new List<Card>();

        // 遍历前面得到的所有纸牌名称
        for(int i = 0; i < cardNames.Count; ++i)
        {
            // 生成纸牌并添加到纸牌Deck
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        // 创建一个新的Card游戏对象
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        // 将transform.parent设置为锚点
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();   // 获取Card组件

        // 以下语句用于排列纸牌，使其整齐摆放
        cgo.transform.localPosition = new Vector3((cNum%13)*3, cNum / 13*4,0);

        // 设置纸牌的基本属性值
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if("D"== card.suit ||"H"==card.suit)
        {
            card.colS = "Red";
            card.color = Color.red;
        }

        // 提取本张纸牌的定义
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    // 这些私有变量会在helper方法中重用
    private Sprite          _tSp = null;
    private GameObject      _tGO = null;
    private SpriteRenderer  _tSR = null;

    private void AddDecorators(Card card)
    {
        //添加角码
        foreach(Decorator deco in decorators)
        {
            if(deco.type=="suit")
            {
                // 初始化一个Sprite游戏对象
                _tGO = Instantiate(prefabSprite) as GameObject;
                // 获取SpriteRenderer组件
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                // 将Sprite设置为正确的花色
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponentInParent<SpriteRenderer>();
                // 获取正确的Sprite显示该点数
                _tSp = rankSprites[card.rank];
                // 将表示点数的Sprite赋给SpriteRender
                _tSR.sprite = _tSp;
                // 使点数符号的颜色与纸牌的花色相符
                _tSR.color = card.color;
            }
            // 使表示角码的Sprite显示在纸牌之上
            _tSR.sortingOrder = 1;
            // 使表示角码的Sprite成为纸牌的子对象
            _tGO.transform.parent = card.transform;
            // 根据DeckXML中的位置设置localPosition
            _tGO.transform.localPosition = deco.loc;
            // 如有必要，则反转角码
            if (deco.flip)
            {
                // 让角码沿z轴进行180°的欧拉旋转，即会使它翻转
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            // 设置角码的缩放比例，以免其尺寸过大
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            // 为游戏对象指定名称，使其易于查找
            _tGO.name = deco.type;
            // 将这个deco游戏对象添加到card.decoGos列表List中
            card.decoGOs.Add(_tGO);
        }
    }

    private void AddPips(Card card)
    {
        // 对于定义内容中的每个花色符号
        foreach( Decorator pip in card.def.pips)
        {
            // 初始化一个Sprite游戏对象
            _tGO = Instantiate(prefabSprite) as GameObject;
            // 将Card设置为它的父对象
            _tGO.transform.SetParent(card.transform);
            // 按照XML内容设置其位置
            _tGO.transform.localPosition = pip.loc;

            // 必要时进行缩放（只适用于点数为A的情况）
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            // 为游戏对象指定名称
            _tGO.name = "pip";
            // 获取它的SpriteRenderer组件
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            // 将Sprite设置为正确的花色符号
            _tSR.sprite = dictSuits[card.suit];
            // 设置sortingOrder，使花色符号显示在纸牌背景Card_Front之上
            _tSR.sortingOrder = 1;
            // 将Add this to the Card's list of pips
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)
    {
        if (card.def.face != "")
        {
            // 如果card.def的face字段不为空（表示纸牌有牌面的图案）
            _tGO = Instantiate( prefabSprite) as GameObject;
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            // 生成正确的名称并传递给GetFace()
            _tSp = GetFace(card.def.face + card.suit);
            _tSR.sprite = _tSp;     // 将这个Sprite赋为_tSp变量
            _tSR.sortingOrder = 1;  // 设置sortingOrder
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = Vector3.zero;
            _tGO.name = "face";
        }
    }

    // 查找正确的花牌
    public Sprite GetFace(string faceS)
    {
        foreach(Sprite _tSP in faceSprites)
        {
            // 如果Sprite名称正确......
            if(_tSP.name == faceS)
            {
                // 则返回这个Sprite
                return (_tSP);
            }
        }
        // 如果查找不到，则返回null
        return (null);
    }

    private void AddBack(Card card)
    {
        // 添加纸牌背景
        // Card_Back将覆盖纸牌上的所有其他元素
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        // 它的sortingOrder值高于纸牌上的所有其他元素
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;

        // face-up的默认值
        card.faceUp = startFaceUp;  // 使用Card的faceUp属性
    }

    // 为Deck.cards中的纸牌洗牌
    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();    // 创建一个临时List，用于存储洗牌后纸牌的新顺序

        int ndx;    //这个变量将存储要移动的纸牌的索引
        // tCards = new List<Card>();      // 初始化临时List

        // 只要原始List中还有纸牌，就一直循环
        while (oCards.Count>0)
        {
            // 随机抽取一张纸牌，并得到它的索引
            ndx = UnityEngine.Random.Range(0, oCards.Count);
            // 把这张纸牌加到临时List中
            tCards.Add(oCards[ndx]);
            // 同时把它从原始List中删除
            oCards.RemoveAt(ndx);
        }
        // 用新的临时List取代原始List
        oCards = tCards;
        // 因为oCards是一个引用型参数，所以传入的原始List也会被修改

    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Card card in cards)
        {
            card.faceUp = startFaceUp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
