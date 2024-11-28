using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Update the current generation text.
/// </summary>
public class UpdateGenerationText : MonoBehaviour
{
    public TextMeshProUGUI generationText;

    public void UpdateCurrentGeneration(int currentGeneration) 
    {
        generationText.text = "Current Generation: " + currentGeneration.ToString();
    }
}
