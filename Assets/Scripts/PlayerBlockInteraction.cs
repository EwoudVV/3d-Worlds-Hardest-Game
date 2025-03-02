using UnityEngine;

public class PlayerBlockInteraction : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("bablockyes"))
        {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.green; //change color to green
        }
        else if (collision.gameObject.CompareTag("bablockno"))
        {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.red; // change color to red

            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null) // die
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                
                rb.position = playerMovement.respawnPosition;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}