using Mirror;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PingData
{
    public Vector3 coords;
    public int senderId;

    public PingData(Vector3 coords)
    {
        if (coords == null)
        {
            coords = Vector3.zero;
        }
        this.coords = coords;
        this.senderId = 0;
    }

    public override string ToString()
    {
        return $"Ping at x: {coords.x} y: {coords.y} z: {coords.z} by {senderId}";
    }
}

public class PingManager : NetworkBehaviour
{
    [SerializeField] private GameObject pingPrefab = null;
    private static event Action<PingData> OnPing;

    [Client]
    private void Update()
    {
        if (Keyboard.current.leftAltKey.isPressed && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ping();
        }
    }

    private void Start()
    {
        OnPing += HandlePing;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        OnPing -= HandlePing;
    }

    private void HandlePing(PingData ping)
    {
        Instantiate(pingPrefab, ping.coords, Quaternion.identity);
    }

    [Client]
    public void Ping()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        
        if (Physics.Raycast(ray, out var hit))
        {
            PingData ping = new(hit.point);
            CmdPing(ping);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdPing(PingData ping, NetworkConnectionToClient sender = null)
    {
        if (sender != null)
        {
            ping.senderId = sender.connectionId;
        }
        RpcHandlePing(ping);
    }

    [ClientRpc]
    private void RpcHandlePing(PingData ping)
    {
        OnPing?.Invoke(ping);
    }
}
