
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WordlePanelHandler : MonoBehaviour
{
    private UIDocument wordlePanel;
    private List<List<VisualElement>> cellsRows = new List<List<VisualElement>>();
    private List<VisualElement> currentCellsRow = new List<VisualElement>();
    private List<VisualElement> keyboardPanelKeys = new List<VisualElement>();

    private string matchWord;
    private Label matchWordLabel;

    private string currentWord;

    private Button replayButton;

    private WordsHandler wordsHandler = new WordsHandler();

    private enum letterState {DontHave,Have,Correct}
    private enum GameState {Playing,NotPlaying}
    private GameState currentGameState = GameState.NotPlaying;

    private void OnEnable()
    {
        wordlePanel = GetComponent<UIDocument>();

        var root = wordlePanel.rootVisualElement;


        GetCellsList(root);
        GetkeyboardKeysList(root);

        GetMatchWordLabel(root);

        BindReplayButton(root);
        BindQuitButton(root);

        currentWord = "";
        BindKeyboardPanelKeys();
    }

    private void BindReplayButton(VisualElement root)
    {
        replayButton = root.Q<Button>("replay-button");
        replayButton.clicked += () => OnReplayInput();
    }

    private void Start()
    {
        InitMatch();
    }

    private void InitMatch() 
    {
        matchWordLabel.text = string.Empty;
        matchWord = RandomFiveLettersWord();

        currentWord = string.Empty;
        currentCellsRow = cellsRows[0];


        InitCells();
        InitKeyboardPanel();

        replayButton.SetEnabled(false);

        currentGameState = GameState.Playing;
    }

    private void InitKeyboardPanel()
    {
        foreach (var key in keyboardPanelKeys)
        {
            key.RemoveFromClassList("key-correct");
            key.RemoveFromClassList("key-have");
            key.RemoveFromClassList("key-donthave");
        }
    }

    private void BindQuitButton(VisualElement root)
    {
        var quitButton = root.Q<Button>("leave-button");
        quitButton.clicked += () => Application.Quit();
    }


    private void Update()
    {
        if (currentGameState == GameState.Playing) 
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnSendInput();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                OnEraseInput();
            }
            else if (Input.anyKeyDown)
            {
                char input = Input.inputString[0];
                if (char.IsLetter(input))
                {
                    OnLetterInput(input);
                }
            }
        }

    }
    private void GetMatchWordLabel(VisualElement root)
    {
        matchWordLabel = root.Q<Label>("word-label");
    }

    private void BindKeyboardPanelKeys()
    {
        foreach (Button key in keyboardPanelKeys) 
        {
            if (key.name.IndexOf('-') == 1)
            {
                key.clicked += () => OnLetterInput(key.name[0]);
            }
            else if (key.name.StartsWith("send")) 
            {
                key.clicked += OnSendInput;
            }
            else if (key.name.StartsWith("erase"))
            {
                key.clicked += OnEraseInput;
            }
        }
        
    }
    
    private void OnLetterInput(char letter)
    {
        if (currentGameState == GameState.Playing) {
            if (currentWord.Length < 5)
            {
                currentCellsRow[currentWord.Length].Q<Label>("cell-text").text = $"{letter}";
                currentWord += letter;
            }
        }
    }


    private void OnEraseInput()
    {
        if (currentGameState == GameState.NotPlaying) return;
        if (currentWord.Length < 1) return;
        currentCellsRow[currentWord.Length-1].Q<Label>("cell-text").text = "";
        if (currentWord.Length == 1)
        {
            currentWord = "";
        }
        else 
        {
            currentWord = currentWord.Substring(0, currentWord.Length - 1);
        }
    }

    private void OnSendInput() 
    {
        if (currentGameState == GameState.NotPlaying) return;
        if (currentWord.Length < 5) return;
        if (!WordsHandler.words.Contains(currentWord)) return;
        if (currentWord == matchWord)
        {
            currentCellsRow.ForEach(cell => cell.AddToClassList("cell-correct"));
            EndGame();
        }
        else
        {
            // check each letter and change the cells of the current row and the keyboardpanel keys
            for (int i = 0; i < matchWord.Length; i++)
            {
                if (currentWord[i] == matchWord[i])
                {
                    currentCellsRow[i].AddToClassList("cell-correct");

                    UpdateKeyboardKeyState(currentWord[i], letterState.Correct);
                }
                else if (matchWord.Contains(currentWord[i]))
                {
                    if(matchWord.Count(x => x == currentWord[i]) == 1)
                    {
                        // if it only has it once, check if ther is a correct match. If there is,
                        // do not mark this cell and its key as Have
                        var index = matchWord.IndexOf(currentWord[i]);
                        if (currentWord[index] == matchWord[index]) 
                        {
                            continue;
                        }
                    }

                    currentCellsRow[i].AddToClassList("cell-have");

                    UpdateKeyboardKeyState(currentWord[i], letterState.Have);
                }
                else
                {
                    currentCellsRow[i].AddToClassList("cell-donthave");

                    UpdateKeyboardKeyState(currentWord[i], letterState.DontHave);
                }
            }

            var currentCellsRowIndex = cellsRows.IndexOf(currentCellsRow);
            if (currentCellsRowIndex < 5)
            {
                currentCellsRow = cellsRows[++currentCellsRowIndex];
                currentWord = "";
            }
            else 
            {
                EndGame();
            }
        }
    }

    private void OnReplayInput() 
    {
        replayButton.SetEnabled(false);
        InitMatch();
    }

    private void EndGame()
    {
        matchWordLabel.text = matchWord;

        replayButton.SetEnabled(true);

        currentGameState = GameState.NotPlaying;
    }

    private void UpdateKeyboardKeyState(char letter,letterState state) 
    {
        var keyboardkey = keyboardPanelKeys.Find(key => key.name.StartsWith(letter));
        if (keyboardkey != null) 
        {
            switch (state)
            {
                case letterState.DontHave:
                    keyboardkey.AddToClassList("key-donthave");
                    break;
                case letterState.Have:
                    keyboardkey.AddToClassList("key-have");
                    break;
                case letterState.Correct:
                    keyboardkey.AddToClassList("key-correct");
                    break;
            }
        }
    }

    private void InitCells()
    {
        foreach(var cellRow in cellsRows) 
        {
            foreach (var cell in cellRow) 
            {
                cell.Q<Label>("cell-text").text = "";

                cell.RemoveFromClassList("cell-correct");
                cell.RemoveFromClassList("cell-have");
                cell.RemoveFromClassList("cell-donthave");
            }
        }
    }

    private void GetkeyboardKeysList(VisualElement root)
    {
        var keyboardContainer = root.Q<VisualElement>("keyboard-container");
        var keyboardRows = keyboardContainer.Children().ToList();
        foreach(var row in keyboardRows) 
        {
            keyboardPanelKeys.AddRange(row.Children().ToList());
        }
    }

    private void GetCellsList(VisualElement root)
    {
        var rowContainer = root.Q<VisualElement>("rows-container");
        var rows = rowContainer.Children().ToList();

        // Generate List Rows
        cellsRows.Add(new List<VisualElement>());
        cellsRows.Add(new List<VisualElement>());
        cellsRows.Add(new List<VisualElement>());
        cellsRows.Add(new List<VisualElement>());
        cellsRows.Add(new List<VisualElement>());
        cellsRows.Add(new List<VisualElement>());

        foreach (var row in rows)
        {
            cellsRows[rows.IndexOf(row)].AddRange(row.Children().ToList());
        }
    
    }


    private string RandomFiveLettersWord() 
    {
        string word = WordsHandler.words[Random.Range(0, WordsHandler.words.Count - 1)];
        return word;
    }

}