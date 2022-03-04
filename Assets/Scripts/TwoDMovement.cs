using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//some easy ported over movement code from a previous game jam to test room navigation and collision
[RequireComponent(typeof(Rigidbody2D))]
public class TwoDMovement : MonoBehaviour
{

    [SerializeField] float speed = 10;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    //Checks input and handles movement
    void HandleMovement(){
        //moving in all 4 directions
        if(Input.GetKey(KeyCode.W)){
            rb.velocity = Vector2.up * speed;

        }
        else if(Input.GetKey(KeyCode.S)){
            rb.velocity = Vector2.down * speed;

        }
        else if(Input.GetKey(KeyCode.A)){
            rb.velocity = Vector2.left * speed;

        }
        else if(Input.GetKey(KeyCode.D)){
            rb.velocity = Vector2.right * speed;

        }
        //stationary
        else {
            rb.velocity = Vector2.zero;
        }
    }
}
