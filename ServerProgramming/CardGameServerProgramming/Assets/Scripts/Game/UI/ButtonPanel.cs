using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPanel : MonoBehaviour
{
    public Action OnButtonClickedEvent;

    [SerializeField] TextMeshProUGUI m_MainText;
    [SerializeField] Text m_ButtonText;
    [SerializeField] GameObject m_ButtonGO;

    private void Start()
    {
        m_ButtonGO.GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    public void ResetPanel()
    {
        m_ButtonGO.SetActive(true);
    }

    public void SetTexts(string mainText, string buttonText)
    {
        m_MainText.text = mainText;
        m_ButtonText.text = buttonText;
    }

    private void OnButtonClicked()
    {
        OnButtonClickedEvent?.Invoke();
    }

    private void OnDestroy()
    {
        m_ButtonGO.GetComponent<Button>().onClick.RemoveListener(OnButtonClicked);
        m_ButtonGO.SetActive(false);
    }

}
