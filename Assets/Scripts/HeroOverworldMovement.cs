using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroOverworldMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask collisionDetector;
    public Animator anim;
    //Start is called before the first frame update 
    void Start()
    {
        anim = GetComponent<Animator>();
        movePoint.parent = null;
    }

    //Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, movePoint.position) <= 0.05f){
            if(Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f){
                anim.SetFloat("turnHoriz", Input.GetAxisRaw("Horizontal"));
                anim.SetFloat("turnVert", 0);
                if(!Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), 0f, collisionDetector))
                movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                }
            else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f){
                anim.SetFloat("turnHoriz", 0);
                anim.SetFloat("turnVert", Input.GetAxisRaw("Vertical"));
                if(!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), 0f, collisionDetector)){
                movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                }
            }
            anim.SetBool("moving", false);
        }   else { 
            anim.SetBool("moving", true);
        }

    }
}
