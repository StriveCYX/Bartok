using UnityEngine;

public class TurnLight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.back * 3;
        if(Bartok.CURRENT_PLAYER == null)
        {
            return;
        }

        transform.position += Bartok.CURRENT_PLAYER.handSlotDef.pos;
    }
}
