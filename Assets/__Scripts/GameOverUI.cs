using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI txt;

    void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
        txt.text = "";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }

        // 只有游戏结束时才会运行至此
        if (Bartok.CURRENT_PLAYER == null)
            return;
       
        if(Bartok.CURRENT_PLAYER.type == PlayerType.human)
        {
            txt.text = "You Won!";
        }
        else
        {
            txt.text = "Game Over";
        }
        
    }
}
