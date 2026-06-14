using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string   suit;       // 牌的花色（红桃、黑桃、方片、梅花
    public int      rank;       // 牌的点数（1~13）
    public Color    color = Color.black;    // 花色符号的颜色
    public string   colS = "Black";      // 颜色的名称，值为"Black"或"Red"

    // 以下List存储所有的Decorator游戏对象
    public List<GameObject> decoGOs = new List<GameObject>();
    // 以下List存储所有的Pip游戏对象
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject       back;  // 纸牌背面图像的游戏对象

    public CardDefinition   def;    // 该变量的值解析自DeckXML.xml

    // 当前游戏对象的SpriteRenderer组件列表及其子类
    public SpriteRenderer[] spriteRenderers;

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }

    // 通过在子类函数中使用相同名字可以重写函数
    virtual public void OnMouseUpAsButton()
    {
        print(name);    // 单击时输出纸牌名
    }

    // 如果未定义spriteRenderers，使用该函数定义
    public void PopulateSpriteRenderers()
    {
        // 如果spriteRenderers为null或empty
        if(spriteRenderers == null || spriteRenderers.Length == 0)
        {
            // 获取当前游戏对象的SpriteRenderer组件及其子类
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // 设置所有SpriteRenderer组件的sortingLayerName
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    // 设置所有的SpriteRenderer组件的sortingOrder
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        // 遍历所有为tSR的spriteRenderers
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if(tSR.gameObject == this.gameObject)
            {
                // 如果gameObjct为this.gameObject，则为背景
                tSR.sortingOrder = sOrd;    // 设置顺序为sOrd
                continue;   // 继续遍历下一个循环
            }
            // GameObject的每一个子对象都根据names变换名称
            switch(tSR.gameObject.name)
            {
                case "back":    //如果名称为“back”
                    // ^设置为最高层，覆盖所有
                    tSR.sortingOrder = sOrd + 2;
                    break;

                case "face":    // 名字为“face”
                default:        // 或其他
                    // ^设置为中层，置于背景之上
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSortOrder(0);       // 保证纸牌开始于正确的深度排序
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]       // 序列化类可以在检视面板中编辑
public class Decorator
{
    // 此类用于存储来自DeckXML的角码符号（包括纸牌角部的点数和花色符号）的信息
    public  string   type;   // 对于花色符号，type = "pip"
    public  Vector3  loc;     //  Spite在纸牌上的位置信息
    public  bool    flip = false;      // 是否垂直反转Spite
    public  float   scale = 1f;     // Sprite的缩放比例
}

[System.Serializable]
public class CardDefinition
{
    // 此类用于存储各点数的牌面信息
    public string   face;   // 各张花牌（J、Q、K）所用的Sprite
    public int  rank;   // 此牌的点数(1~13)
    public List<Decorator>  pips = new List<Decorator>();   // 所用花色
}