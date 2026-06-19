using GameLogic;
using UnityEngine;
using Packets;

public class GameManager : MonoBehaviour
{
    [SerializeField] TCPClientConnection m_TCPClientConnection;
    [SerializeField] UIManager m_UIManager;
    [SerializeField] HeartbeatServiceManager m_HeartbeatServiceManager;

    int m_PlayerID;
    string m_PlayerName;
    string m_PlayerReconnectionToken;

    bool m_IsPlayerProfileSet = false;

    void Start()
    {
        m_UIManager.SetConnectingToServerPanel();

        m_TCPClientConnection.Event_ConnectedToServer += PostServerConnectionDone;

        m_TCPClientConnection.Event_ReceivedClientID += SetPlayerID;
        m_TCPClientConnection.Event_ReceivedClientName += SetPlayerName;
        m_TCPClientConnection.Event_ReceivedClientReconnectionToken += SetPlayerReconnectionToken;

        m_TCPClientConnection.Event_Disconnected += OnDisconnected;
        m_TCPClientConnection.Event_Reconnected += OnReconnected;
        m_TCPClientConnection.Event_ReconnectionAccepted += OnReconnectionAccepted;
        m_TCPClientConnection.Event_ReconnectionFailed += OnReconnectionFailed;

        m_TCPClientConnection.ConnectToServer();
    }

    void SendHeartbeatPulse()
    {
        m_TCPClientConnection.SendSystemPacket(SystemPacketTypes.HeartBeat);
    }

    void PostServerConnectionDone()
    {
        m_HeartbeatServiceManager.SendHeartBeatPulse -= SendHeartbeatPulse;
        m_HeartbeatServiceManager.SendHeartBeatPulse += SendHeartbeatPulse;

        m_UIManager.SetLookForMatchPanel();
        m_UIManager.buttonPanel.OnButtonClickedEvent += LookForMatch;
    }

    void LookForMatch()
    {
        m_TCPClientConnection.SendSystemPacket(SystemPacketTypes.MatchMakingRequested);
        m_UIManager.buttonPanel.OnButtonClickedEvent -= LookForMatch;
        
        m_UIManager.SetMatchMakingPanel();
        m_TCPClientConnection.Event_GameStarted += OnGameStarted;
    }

    void OnGameStarted()
    {
        m_UIManager.SetGamePanel();

        m_TCPClientConnection.Event_GameStateUpdated += SetGamePanel;
    }

    void SetPlayerID(int playerID)
    {
        if (m_IsPlayerProfileSet) return;

        m_PlayerID = playerID;

        m_TCPClientConnection.Event_ReceivedClientID -= SetPlayerID;
    }

    void SetPlayerName(string playerName) 
    {
        if (m_IsPlayerProfileSet) return;

        m_PlayerName = playerName;

        m_TCPClientConnection.Event_ReceivedClientName -= SetPlayerName;
    }

    void SetPlayerReconnectionToken(string playerReconnectionToken)
    {
        if (m_IsPlayerProfileSet) return;

        m_PlayerReconnectionToken = playerReconnectionToken;

        m_TCPClientConnection.Event_ReceivedClientReconnectionToken -= SetPlayerReconnectionToken;

        m_IsPlayerProfileSet = true;
    }

    void OnDisconnected()
    {
        m_UIManager.SetReconnectingPanel(true);
    }

    void OnReconnected()
    {
        m_TCPClientConnection.SendReconnectionPacket(m_PlayerReconnectionToken);
    }
    void OnReconnectionAccepted()
    {
        m_UIManager.SetReconnectingPanel(false);
    }

    void OnReconnectionFailed()
    {
        string matchResultMessage = "You lost!";
        m_UIManager.SetMatchResultPanel(matchResultMessage);
        m_TCPClientConnection.Event_GameStateUpdated -= OnGameStateUpdated;
        m_UIManager.gamePanel.OnUseCardButtonClickedEvent -= SendCardAction;

        m_UIManager.buttonPanel.OnButtonClickedEvent += OnGameOver;
    }

    void SetGamePanel(GameState state)
    {
        m_UIManager.gamePanel.SetPlayerProfile(state.PlayerState_1, state.PlayerState_1.PlayerID == m_PlayerID);
        m_UIManager.gamePanel.SetPlayerProfile(state.PlayerState_2, state.PlayerState_2.PlayerID == m_PlayerID);
        m_UIManager.gamePanel.ResetGamePanel();
        m_UIManager.gamePanel.SetTurn(state.GameTurnPlayerID == m_PlayerID);

        m_UIManager.gamePanel.OnUseCardButtonClickedEvent += SendCardAction;

        m_TCPClientConnection.Event_GameStateUpdated -= SetGamePanel;
        m_TCPClientConnection.Event_GameStateUpdated += OnGameStateUpdated;
    }

    void OnGameStateUpdated(GameState state)
    {
        m_UIManager.gamePanel.SetPlayerStats(state.PlayerState_1.PlayerHealth, state.PlayerState_1.PlayerMana, 
            state.PlayerState_1.PlayerID == m_PlayerID);
        m_UIManager.gamePanel.SetPlayerStats(state.PlayerState_2.PlayerHealth, state.PlayerState_2.PlayerMana,
            state.PlayerState_2.PlayerID == m_PlayerID);

        if(!state.IsGameOver)
        {
            m_UIManager.gamePanel.ResetGamePanel();
            m_UIManager.gamePanel.SetTurn(state.GameTurnPlayerID == m_PlayerID);
        }
        else
        {
            string matchResultMessage = state.GameWinnerID == 0 ? "It was a draw!"
                : (state.GameWinnerID == m_PlayerID ? "You won!" : "You lost!");
            m_UIManager.SetMatchResultPanel(matchResultMessage);
            m_TCPClientConnection.Event_GameStateUpdated -= OnGameStateUpdated;
            m_UIManager.gamePanel.OnUseCardButtonClickedEvent -= SendCardAction;

            m_UIManager.buttonPanel.OnButtonClickedEvent += OnGameOver;
        }
    }

    void SendCardAction(GameActionTypes action)
    {
        m_TCPClientConnection.SendGameActionPacket(action);
    }

    void OnGameOver()
    {
        m_TCPClientConnection.SendSystemPacket(SystemPacketTypes.LeaveMatch);
        PostServerConnectionDone();

        m_UIManager.buttonPanel.OnButtonClickedEvent -= OnGameOver;
    }

    private void OnDestroy()
    {
        m_HeartbeatServiceManager.SendHeartBeatPulse -= SendHeartbeatPulse;
    }
}
