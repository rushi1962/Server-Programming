using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_LoadingText;

    public void SetLoadingText(string text)
    {
        m_LoadingText.text = text;
    }
}
