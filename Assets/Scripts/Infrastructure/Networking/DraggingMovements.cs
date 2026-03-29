using Mirror;
using UnityEngine;

public class Movements : NetworkBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;

    [Client]
    private void OnMouseDown()
    {
        if (!isOwned || !isLocalPlayer)
            return;
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        mOffset = transform.position - GetMouseWorldPos();
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    [Client]
    private void OnMouseDrag()
    {
        if (!isOwned || !isLocalPlayer)
            return;

        Vector3 targetPosition = GetMouseWorldPos() + mOffset;
        CmdMoveObject(targetPosition);
    }

    [Command]
    void CmdMoveObject(Vector3 newPosition)
    {
        if (!connectionToClient.isReady || connectionToClient.identity != netIdentity)
            return;

        RpcUpdatePosition(newPosition);
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}