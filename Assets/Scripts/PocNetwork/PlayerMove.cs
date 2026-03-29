using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
    private void Update()
    {
        if (isLocalPlayer)
        {
            //float h = Input.GetAxis("Horizontal");
            //float v = Input.GetAxis("Vertical");
            bool left = Input.GetKey(KeyCode.LeftArrow);
            bool right = Input.GetKey(KeyCode.RightArrow);
            bool forward = Input.GetKey(KeyCode.UpArrow);
            bool backward = Input.GetKey(KeyCode.DownArrow);

            float h = (left ? -1 : 0) + (right ? 1 : 0);
            float v = (forward ? 1 : 0) + (backward ? -1 : 0);

            Vector3 playerMovement = new Vector3(h * 0.1f, 0, v * 0.1f);
            transform.position = transform.position + playerMovement;
        }
    }
}
