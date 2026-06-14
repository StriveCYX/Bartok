using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// 玩家可以是真人或AI
public enum PlayerType
{
    human,
    ai
}

[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand;   // 玩家手牌

    // 增加一张牌
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand is null)
            hand = new List<CardBartok>();

        // 将纸牌添加到手中
        hand.Add(eCB);

        // 如果这是一个真人玩家，将其手牌进行排序
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();

            // LINQ调用
            cards = cards.OrderBy(cd => cd.rank).ToArray();

            hand = new List<CardBartok>(cards);
        }

        eCB.SetSortingLayerName("10");      // 此处排序将纸牌移动到顶部
        eCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return (eCB);
    }

    public CardBartok RemoveCard(CardBartok cb)
    {
        // 如果hand为null或没有包含cb，返回null
        if (hand is null || !hand.Contains(cb))
            return null;
        hand.Remove(cb);
        FanHand();
        return (cb);
    }

    public void FanHand()
    {
        // startRot是第一张牌的z旋转
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // 将所有纸牌移动到新位置、
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; ++i)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
            pos = rotQ * pos;

            // 添加玩家手牌的基本位置（在扇形排列的纸牌底部中心）
            pos += handSlotDef.pos;
            // 纸牌在z方向错开，这是不可见的，可避免重叠
            pos.z = -0.5f * i;

            // 如果它不是游戏最开始发的牌，下面一行代码确保纸牌立即开始移动
            if (Bartok.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            // 设置第i张牌的当前位置及旋转
            hand[i].MoveTo(pos, rotQ);      // 告诉CardBartok插入
            hand[i].state = CBState.toHand; // 移动之后，CardBartok讲状态设置为CBState.hand

            // 设置localPosition以及第i张牌的旋转
            //hand[i].transform.localPosition = pos;
            //hand[i].transform.rotation = rotQ;
            //hand[i].state = CBState.hand;

            hand[i].faceUp = (type == PlayerType.human);

            // 设置纸牌的SortOrder，以便它们能正确重叠
            //hand[i].SetSortOrder(i * 4);
            hand[i].eventualSortOrder = i * 4;
        }
    }

    // TakeTurn()函数启用计算机玩家的AI
    public void TakeTurn()
    {
        Utils.tr("player.TakeTurn");

        // 如果这是真人玩家，什么都不做
        if (type == PlayerType.human)
            return;

        Bartok.S.phase = TurnPhase.waiting;

        CardBartok cb;

        // 如果这是AI玩家，需要选择出什么牌
        // 找到有效的牌
        List<CardBartok> validCards = new List<CardBartok>();
        foreach(CardBartok tCB in hand)
        {
            if (Bartok.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }

        // 如果没有有效的牌，必须抽牌
        if (validCards.Count == 0)
        {
            // 抽一张牌
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }

        // 否则，如果有一张或多张有效的牌，选择一张
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallback(CardBartok tCB)
    {
        Utils.tr("player.CBCallback(): " ,tCB.name,"Player"+ playerNum);
        // 此牌完成移动，传递轮转次序
        Bartok.S.PassTurn();
    }

}
