using GameLogic;
using Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Action<GameActionTypes> OnUseCardButtonClickedEvent;

    [SerializeField] Text m_PlayerName;
    [SerializeField] Text m_PlayerHealth;
    [SerializeField] Text m_PlayerMana;
    [SerializeField] CardButton m_PlayerAttackButton;
    [SerializeField] CardButton m_PlayerHealButton;
    [SerializeField] CardButton m_PlayerManaButton;
    [SerializeField] GameObject m_PlayerUseCardButton;

    [SerializeField] Text m_EnemyName;
    [SerializeField] Text m_EnemyHealth;
    [SerializeField] Text m_EnemyMana;
    [SerializeField] CardButton m_EnemyAttackButton;
    [SerializeField] CardButton m_EnemyHealButton;
    [SerializeField] CardButton m_EnemyManaButton;

    [SerializeField] Text m_TurnText;

    string m_PlayerNameString = "", m_EnemyNameString = "";
    GameActionTypes m_SelectedActionType;

    private void Start()
    {
        m_PlayerAttackButton.OnCardSelected += CardSelected;
        m_PlayerHealButton.OnCardSelected += CardSelected;
        m_PlayerManaButton.OnCardSelected += CardSelected;
        m_PlayerUseCardButton.GetComponent<Button>().onClick.AddListener(OnUseCardButtonClicked);
    }


    public void SetPlayerProfile(PlayerState state, bool isOwnPlayer)
    {
        if (isOwnPlayer)
        {
            m_PlayerNameString = state.PlayerName;

            m_PlayerName.text = m_PlayerNameString;
            m_PlayerHealth.text = state.PlayerHealth.ToString();
            m_PlayerMana.text = state.PlayerMana.ToString();

            m_PlayerAttackButton.SetAmountText(state.PlayerAttackAmount);
            m_PlayerAttackButton.SetCostText(state.PlayerAttackCost);

            m_PlayerHealButton.SetAmountText(state.PlayerHealAmount);
            m_PlayerHealButton.SetCostText(state.PlayerHealCost);

            m_PlayerManaButton.SetAmountText(state.PlayerManaBoostAmount);
            m_PlayerManaButton.SetCostText(state.PlayerManaBoostCost);
        }
        else 
        {
            m_EnemyNameString = state.PlayerName;

            m_EnemyName.text = m_EnemyNameString;
            m_EnemyHealth.text = state.PlayerHealth.ToString();
            m_EnemyMana.text = state.PlayerMana.ToString();

            m_EnemyAttackButton.SetAmountText(state.PlayerAttackAmount);
            m_EnemyAttackButton.SetCostText(state.PlayerAttackCost);

            m_EnemyHealButton.SetAmountText(state.PlayerHealAmount);
            m_EnemyHealButton.SetCostText(state.PlayerHealCost);

            m_EnemyManaButton.SetAmountText(state.PlayerManaBoostAmount);
            m_EnemyManaButton.SetCostText(state.PlayerManaBoostCost);
        }
    }

    public void SetPlayerStats(int health, int mana, bool isOwnPlayer)
    {
        if (isOwnPlayer)
        { 
            m_PlayerHealth.text = health.ToString();
            m_PlayerMana.text = mana.ToString();
        }
        else 
        { 
            m_EnemyHealth.text = health.ToString();
            m_EnemyMana.text= mana.ToString();
        }
    }

    public void ResetGamePanel()
    {
        m_PlayerAttackButton.ResetCard();
        m_PlayerHealButton.ResetCard();
        m_PlayerManaButton.ResetCard();
        m_PlayerAttackButton.SetEnabled(false);
        m_PlayerHealButton.SetEnabled(false);
        m_PlayerManaButton.SetEnabled(false);

        m_EnemyAttackButton.ResetCard();
        m_EnemyHealButton.ResetCard();
        m_EnemyManaButton.ResetCard();
        m_EnemyAttackButton.SetEnabled(false);
        m_EnemyHealButton.SetEnabled(false);
        m_EnemyManaButton.SetEnabled(false);

        m_PlayerUseCardButton.SetActive(false);
    }

    public void SetTurn(bool isOwnTurn)
    {
        m_TurnText.text = isOwnTurn ? m_PlayerNameString : m_EnemyNameString;

        if (isOwnTurn) 
        {
            m_PlayerAttackButton.SetEnabled(true);
            m_PlayerHealButton.SetEnabled(true);
            m_PlayerManaButton.SetEnabled(true);
        }
    }

    private void CardSelected(GameActionTypes actionType)
    {
        m_SelectedActionType = actionType;

        switch (actionType) 
        {
            case GameActionTypes.GameAction_Attack:
                m_PlayerHealButton.ResetCard();
                m_PlayerManaButton.ResetCard();
                break;

            case GameActionTypes.GameAction_Heal:
                m_PlayerAttackButton.ResetCard();
                m_PlayerManaButton.ResetCard();
                break;

            case GameActionTypes.GameAction_ManaBoost:
                m_PlayerAttackButton.ResetCard();
                m_PlayerHealButton.ResetCard();
                break;
        }

        m_PlayerUseCardButton.SetActive(true);
    }

    public void OnUseCardButtonClicked()
    {
        OnUseCardButtonClickedEvent?.Invoke(m_SelectedActionType);
        ResetGamePanel();
    }

    private void OnDestroy()
    {
        m_PlayerAttackButton.OnCardSelected -= CardSelected;
        m_PlayerHealButton.OnCardSelected -= CardSelected;
        m_PlayerManaButton.OnCardSelected -= CardSelected;
        m_PlayerUseCardButton.GetComponent<Button>().onClick.RemoveListener(OnUseCardButtonClicked);
    }
}
