using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    private bool is_dead = false;
    private Rigidbody2D rb2d;
    public float up_force = 200f;
    private Animator anim;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!is_dead)
        {
            if (Input.GetMouseButtonDown(0) ||Input.GetKeyDown(KeyCode.UpArrow))
            {
                rb2d.velocity = Vector2.zero;
                rb2d.AddForce(new Vector2(0,up_force));
                anim.SetTrigger("Flap");
            }
        }
	}

    void RigidUpdate()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag != "Bird")
        {
            rb2d.velocity = Vector2.zero;
            is_dead = true;
            anim.SetTrigger("Die");
            GameController.instance.BirdDied();
        }
        
    }

}
