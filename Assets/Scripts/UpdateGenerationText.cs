using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateGenerationText : MonoBehaviour
{
    public TextMeshProUGUI generationText;

    public void UpdateCurrentGeneration(int currentGeneration) 
    {
        generationText.text = "Current Generation: " + currentGeneration.ToString();
    }
}
