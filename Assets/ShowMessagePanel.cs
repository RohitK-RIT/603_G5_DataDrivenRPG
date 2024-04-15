using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class ShowMessagePanel : MonoBehaviour
{
    [TextArea(20, 20)]
    public string text; //what text to display on the ui. 
    public TextMeshProUGUI tutorialText; //the ui elemeent that displays the text


    private void OnTriggerEnter(Collider other)
    {
        tutorialText.text = text;
    }

}
