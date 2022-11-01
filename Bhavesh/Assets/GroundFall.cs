using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFall : MonoBehaviour
{

    bool shouldFall = false;
    public float fallSpeed = 1;

    public Player player;
    public List<Obstacle> obstacles = new List<Obstacle>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
        if (shouldFall)
        {
            Vector2 pos = transform.position;
            float fallAmount = fallSpeed * Time.fixedDeltaTime;
            pos.y -= fallAmount;

            if (player != null)
            {
                player.groundHeight -= fallAmount;
                Vector2 playerPos = player.transform.position;
                playerPos.y -= fallAmount;
                player.transform.position = playerPos;
            }

            foreach (Obstacle o in obstacles)
            {
                if (o != null)
                {
                    Vector2 oPos = o.transform.position;
                    oPos.y -= fallAmount;
                    o.transform.position = oPos;
                }
            }
            transform.position = pos;
        }
        else
        {
            if (player != null)
            {
                shouldFall = true;
            }
        }

    }
}
