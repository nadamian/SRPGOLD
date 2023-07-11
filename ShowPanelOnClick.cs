using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPanelOnClick : MonoBehaviour
{
    public GameObject panel;
    // Start is called before the first frame update
    void OnClick()
    {
        panel.SetActive(true);
    }
}
