using UnityEngine;
using UnityEngine.Scripting;

// The website's dev mode. On the game page (gabrielwlogue.com/stella-hostis/), pressing "Hack my website"
// opens a cheat panel, and the page passes each button press in here with
// unityInstance.SendMessage("WebDevBridge", method, value). Every one of them marks the run as a dev run,
// which keeps its score off the highscore list (see GameStats.OfferHighscore).
//
// It makes its own GameObject when the game starts, so there's nothing to set up in the scene. SendMessage
// finds it by that name, so the page and this class have to keep agreeing on "WebDevBridge".
// Values arrive as strings: the page sends numbers as text, so no method here depends on how
// SendMessage would convert a JavaScript number.
//
// [Preserve] keeps the build's code stripping from removing methods that nothing in the game calls
// directly, since only the page ever does.
[Preserve]
public class WebDevBridge : MonoBehaviour
{
    private GameStats gameStats;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        new GameObject(nameof(WebDevBridge)).AddComponent<WebDevBridge>();
    }

    //Looked up on first use rather than in Start, in case the page calls in before the first frame
    GameStats Stats
    {
        get
        {
            if (gameStats == null) gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
            return gameStats;
        }
    }

    [Preserve]
    public void EnableDevMode()
    {
        Stats.EnableDevMode();
    }

    [Preserve]
    public void SkipToLevel(string level)
    {
        if (int.TryParse(level, out int number)) Stats.JumpToLevel(number);
    }

    [Preserve]
    public void SpawnEnemy(string type)
    {
        if (!Stats.SpawnDevEnemy(type))
        {
            Debug.Log($"[WebDevBridge] Couldn't spawn Enemy {type}: no level in progress, or no empty slot");
        }
    }

    [Preserve]
    public void SetInfiniteLife(string on)
    {
        Stats.EnableDevMode();
        Stats.infiniteLife = on == "1";
    }
}
