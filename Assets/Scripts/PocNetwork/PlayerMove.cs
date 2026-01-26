using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    private void Update()
    {
        if (isLocalPlayer)
        {
            float h = Input.GetAxis("Horizontal");
            float w = Input.GetAxis("Vertical");

            Vector3 playerMovement = new Vector3(h * 0.1f, w * 0.1f, 0);
            transform.position = transform.position + playerMovement;
        }
    }
}
