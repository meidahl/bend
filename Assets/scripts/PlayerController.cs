using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 2;
    public float turnSpeed = 60;
    public float jumpSpeed = 6;
    public float gravity = 2;

    private Vector3 velocity = new Vector3();

    private Vector3 turnVector;
    private CharacterController characterController;

	// Use this for initialization
	void Start () {
        turnVector = new Vector3(0, turnSpeed, 0);
        characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {

        velocity.y -= gravity;

        // If the character is on the ground
        if (characterController.isGrounded) {
            // Stop falling
            velocity.y = 0;

            // Friction for x velocity
            if (Mathf.Abs(velocity.x) < speed) velocity.x = 0;
            if (velocity.x >= speed) velocity.x -= speed;
            if (velocity.x <= -speed) velocity.x += speed;

            // Friction for z velocity
            if (Mathf.Abs(velocity.z) < speed) velocity.z = 0;
            if (velocity.z >= speed) velocity.z -= speed;
            if (velocity.z <= -speed) velocity.z += speed;

            // Move forward
            if (Input.GetKey(KeyCode.W)) {
                velocity += transform.forward * speed;
            }
            
            // Move backwards
            if (Input.GetKey(KeyCode.S)) {
                velocity -= transform.forward * speed;
            }

            // Jump
            if (Input.GetKey(KeyCode.Space)) {
                velocity.y += jumpSpeed;
            }

        }

        if (Input.GetKey(KeyCode.LeftShift)) {
            velocity.y = 0;
            transform.position = new Vector3(transform.position.x, transform.position.y + speed * Time.deltaTime, transform.position.z); 
        }

        // Turn left
        if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(-turnVector * Time.deltaTime);
        }

        // Turn right
        if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(turnVector * Time.deltaTime);
        }

        characterController.Move(velocity * Time.deltaTime);
    }
}
