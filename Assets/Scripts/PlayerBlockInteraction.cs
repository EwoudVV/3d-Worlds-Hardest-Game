using UnityEngine;
public class PlayerBlockInteraction : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("bablockyes")) {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.green;
        } else if (collision.gameObject.CompareTag("bablockno")) {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.red;
            Debug.Log("Collided with bablockno, calling TriggerRespawn");
            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null)
                pm.TriggerRespawn();
        }
    }
}
