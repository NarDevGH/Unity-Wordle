using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HowToPlayPanelHandler : MonoBehaviour
{
    [SerializeField] private UIDocument panel;
    [SerializeField] private WordlePanelHandler wordlePanelHandler;
    // obtener ui document
    //bindear boton exit a OnExit metodo

    private void Awake()
    {
        panel = GetComponent<UIDocument>();

        var root = panel.rootVisualElement;
        root.Q<Button>("exit-button").clicked+=()=>OnExitPressed();
    }

    private void OnExitPressed()
    {
        panel.sortingOrder = -1;

        if (wordlePanelHandler.CurrentGameState == WordlePanelHandler.GameState.NotPlaying) 
        {
            wordlePanelHandler.CurrentGameState = WordlePanelHandler.GameState.Playing;
        }
    }
}
