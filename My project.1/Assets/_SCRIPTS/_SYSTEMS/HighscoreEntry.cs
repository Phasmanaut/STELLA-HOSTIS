using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

// Arcade-style initials entry at the end of a run, then sends the score to the highscore list
// on the website (gabrielwlogue.com/highscores.html). GameStats adds this component when the run
// ends, so it doesn't need to be set up in the scene.
public class HighscoreEntry : MonoBehaviour
{
    private const string ScoresUrl = "https://gabrielwlogue.com/api/scores";
    private const int InitialsLength = 3;
    private const float EntryFontSize = 20f; //the screen text's normal size (GAME OVER makes it bigger)

    private TextMeshPro screenText;
    private int score;
    private int level;
    private string initials = "";
    private bool acceptingInput;

    //Touch screens and controllers have no keyboard, so initials are picked arcade-style with the stick
    private char currentLetter = 'A'; //the letter being picked for the next slot
    private float letterRepeatTimer; //holding the stick up/down keeps scrolling through letters
    private bool backHeld; //so one push left only removes one letter
    private bool padMode; //a controller is being used, so show its prompts. Typing on the keyboard switches back

    //The shapes of the JSON sent to and received from the website
    [Serializable] public class ScoreSubmission { public string name; public int score; public int level; }
    [Serializable] public class SubmitResult { public int rank; }

    public void Begin(TextMeshPro screenText, int score, int level)
    {
        this.screenText = screenText;
        this.score = score;
        this.level = level;

        screenText.fontSize = EntryFontSize;
        acceptingInput = true;
        ShowInitials();
    }

    void Update()
    {
        if (!acceptingInput) return;

        if (!MobileControls.Active) UpdateKeyboardEntry();
        if (acceptingInput) UpdatePickerEntry(); //the keyboard may have just saved or skipped
    }

    //Desktop: type the letters, backspace to fix one, enter to save, esc to skip
    void UpdateKeyboardEntry()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Skip();
            return;
        }

        if (padMode && Input.inputString.Length > 0)
        {
            padMode = false; //back on the keyboard, so show the typing prompts again
            ShowInitials();
        }

        //inputString holds the characters typed this frame: letters, '\b' for backspace, '\n' or '\r' for enter
        foreach (char c in Input.inputString)
        {
            bool isLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'); //A-Z only, the website rejects anything else
            if (isLetter && initials.Length < InitialsLength)
            {
                initials += char.ToUpperInvariant(c);
                ShowInitials();
            }
            else if (c == '\b' && initials.Length > 0)
            {
                initials = initials.Substring(0, initials.Length - 1);
                ShowInitials();
            }
            else if ((c == '\n' || c == '\r') && initials.Length == InitialsLength)
            {
                acceptingInput = false;
                StartCoroutine(Submit());
                return;
            }
        }
    }

    //Touch screens and controllers: stick up/down picks a letter, fire (A) locks it in and saves once all 3 are in,
    //a firm push left (or B) goes back, and Start skips saving
    void UpdatePickerEntry()
    {
        Vector2 stick = MobileControls.Stick;
        bool nextPressed = MobileControls.FirePressed;
        bool backPressed = false;

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 padStick = pad.leftStick.ReadValue() + pad.dpad.ReadValue();
            bool padNext = pad.buttonSouth.wasPressedThisFrame;
            bool padBack = pad.buttonEast.wasPressedThisFrame;
            bool padSkip = pad.startButton.wasPressedThisFrame;

            if (!padMode && (padStick.magnitude > MobileControls.DeadZone || padNext || padBack || padSkip))
            {
                padMode = true; //the controller was picked up, so show its prompts
                ShowInitials();
            }
            if (padSkip)
            {
                Skip();
                return;
            }

            stick += padStick;
            nextPressed |= padNext;
            backPressed = padBack;
        }

        float vertical = stick.y;
        int scroll = vertical > MobileControls.DeadZone ? 1 : vertical < -MobileControls.DeadZone ? -1 : 0;
        if (scroll == 0)
        {
            letterRepeatTimer = 0f; //so the next push changes the letter straight away
        }
        else if (initials.Length < InitialsLength)
        {
            letterRepeatTimer -= Time.deltaTime;
            if (letterRepeatTimer <= 0f)
            {
                currentLetter = (char)('A' + (currentLetter - 'A' + scroll + 26) % 26); //wraps around Z <-> A
                letterRepeatTimer = 0.2f;
                ShowInitials();
            }
        }

        bool pushedLeft = stick.x < -0.7f && Mathf.Abs(vertical) < 0.5f; //a firm push left, not a diagonal
        if (((pushedLeft && !backHeld) || backPressed) && initials.Length > 0)
        {
            initials = initials.Substring(0, initials.Length - 1);
            ShowInitials();
        }
        backHeld = pushedLeft;

        if (nextPressed)
        {
            if (initials.Length < InitialsLength)
            {
                initials += currentLetter;
                ShowInitials();
            }
            else
            {
                acceptingInput = false;
                StartCoroutine(Submit());
            }
        }
    }

    void ShowInitials()
    {
        if (padMode || MobileControls.Active)
        {
            if (initials.Length < InitialsLength)
            {
                string slots = $"{initials}[{currentLetter}]" + new string('_', InitialsLength - initials.Length - 1); //e.g. "A[B]_"
                string hint = padMode ? "STICK: A-Z  A: NEXT  B: BACK" : "STICK: A-Z  FIRE: NEXT";
                screenText.text = $"SCORE {score}\nINITIALS: {slots}\n{hint}";
            }
            else
            {
                string hint = padMode ? "A: SAVE  B: BACK  START: SKIP" : "FIRE: SAVE  LEFT: BACK";
                screenText.text = $"SCORE {score}\nINITIALS: {initials}\n{hint}";
            }
            return;
        }

        string typed = initials.PadRight(InitialsLength, '_'); //e.g. "AB_"
        screenText.text = $"SCORE {score}\nENTER INITIALS: {typed}\nENTER: SAVE  ESC: SKIP";
    }

    void Skip()
    {
        acceptingInput = false;
        screenText.text = $"SCORE {score}\nNOT SAVED";
    }

    IEnumerator Submit()
    {
        screenText.text = $"SCORE {score}\nSAVING...";

        string json = JsonUtility.ToJson(new ScoreSubmission { name = initials, score = score, level = level });
        using (UnityWebRequest request = UnityWebRequest.Post(ScoresUrl, json, "application/json"))
        {
            request.timeout = 10; //seconds
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                int rank = JsonUtility.FromJson<SubmitResult>(request.downloadHandler.text).rank;
                screenText.text = $"{initials}  {score}  SAVED!\nRANK #{rank}";
            }
            else
            {
                Debug.LogWarning($"[HighscoreEntry] Couldn't save score: {request.error} {request.downloadHandler.text}");
                screenText.text = padMode ? "COULDN'T SAVE SCORE\nA: RETRY  START: SKIP"
                    : MobileControls.Active ? "COULDN'T SAVE SCORE\nFIRE: RETRY"
                    : "COULDN'T SAVE SCORE\nENTER: RETRY  ESC: SKIP";
                acceptingInput = true; //initials are still filled in, so enter (or fire, or A) sends it again
            }
        }
    }
}
