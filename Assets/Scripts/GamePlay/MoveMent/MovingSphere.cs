using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    Rigidbody rb;
    [Header("Movement")]
    public float speed = 10f;
    //public float speedY;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update () {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
        //playerInput.Normalize();
		//Vector3 displacement = new Vector3(playerInput.x , 0f, playerInput.y) * speed * 0.01f;
		//rb.MovePosition(transform.position + displacement);
	}
}
