using Packets;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CardButton : MonoBehaviour
{
    public Action<GameActionTypes> OnCardSelected; 

    [SerializeField] Text m_AmountText;
    [SerializeField] Text m_CostText;

    [SerializeField] Image m_ImageComponent;

    [SerializeField] GameActionTypes m_ActionType;

    private bool m_Enabled;

    public void SetAmountText(int amount)
    {
        m_AmountText.text = amount.ToString();
    }

    public void SetCostText(int cost) 
    {
        m_CostText.text = cost.ToString();
    }

    public void SetEnabled(bool enabled)
    {
        m_Enabled = enabled;
    }

    public void ResetCard()
    {
        m_ImageComponent.color = new Color(1f, 1f, 1f, 0f);
    }

    public void OnCardButtonClicked()
    {
        if (!m_Enabled) return;

        m_ImageComponent.color = new Color(1f, 1f, 1f, 0.25f);
        OnCardSelected?.Invoke(m_ActionType);
    }
}
