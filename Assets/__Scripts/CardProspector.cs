using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card      // 确保CardProspector从Card继承
{
    [Header("Set Dynamically: CardProspector")]
    // 枚举CardState的使用方式
    public eCardState               state = eCardState.drawpile;
    // hiddenBy列表保存了使用当前纸牌朝下的其他纸牌
    public List<CardProspector>     hiddenBy = new List<CardProspector>();
    // LayoutID对当前纸牌和Layout XML id进行匹配，判断是否为场景纸牌
    public int                      layoutID;
    // The SlotDef存储从LayoutXML <slot>导入的信息
    public SlotDef                  slotDef;

    // 使得纸牌可以响应单击动作
    public override void OnMouseUpAsButton()
    {
        // 调用Prospector单例的CardClicked方法
        Prospector.S.CardClicked(this);
        // 同时调用基础类（Card.cs）的当前方法
        base.OnMouseUpAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}