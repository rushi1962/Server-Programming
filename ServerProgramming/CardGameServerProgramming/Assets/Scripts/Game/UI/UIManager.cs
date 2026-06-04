using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public LoadingPanel loadingPanel;
    public ButtonPanel buttonPanel;
    public GamePanel gamePanel;

    private void ResetPanels()
    {
        loadingPanel.gameObject.SetActive(false);
        buttonPanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(false);
    }

    public void SetConnectingToServerPanel()
    {
        ResetPanels();

        loadingPanel.gameObject.SetActive(true);
        loadingPanel.SetLoadingText("Connecting To Server...");
    }

    public void SetMatchMakingPanel()
    {
        ResetPanels();

        loadingPanel.gameObject.SetActive(true);
        loadingPanel.SetLoadingText("Looking for a match...");
    }

    public Action SetLookForMatchPanel()
    {
        ResetPanels();

        buttonPanel.gameObject.SetActive(true);
        buttonPanel.SetTexts("Connected to server!", "Look for match");
        return buttonPanel.OnButtonClickedEvent;
    }

    public Action SetMatchResultPanel(string MatchResultMessage)
    {
        ResetPanels();

        buttonPanel.gameObject.SetActive(true);
        buttonPanel.SetTexts(MatchResultMessage, "Leave match");
        return buttonPanel.OnButtonClickedEvent;
    }

    public void SetGamePanel()
    {
        ResetPanels();

        gamePanel.gameObject.SetActive(true);
    }
}
