using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using DeviceApplication = UnityEngine.Device.Application; //the Device versions also report what the Device Simulator is pretending to be
using DeviceScreen = UnityEngine.Device.Screen;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// On-screen joystick and fire button for phones and tablets, including mobile browsers.
// Touching the left half of the screen grabs the joystick (it jumps to your thumb); touching
// anywhere else fires. While the phone is held upright, a popup asks the player to turn it sideways
// and the game pauses. It creates itself when the game starts on a mobile device, so nothing
// needs to be set up in the scene. To try it in the editor, open the Device Simulator
// (Window > General > Device Simulator), which pretends to be a phone and turns clicks into touches.
[DefaultExecutionOrder(-100)] //read the touches before the player and gun use them this frame
public class MobileControls : MonoBehaviour
{
    public const float DeadZone = 0.35f; //how far the stick has to move (out of 1) before it counts as a direction

    //Read by GameInput and HighscoreEntry. On desktop these stay zero/false.
    public static bool Active { get; private set; }
    public static Vector2 Stick { get; private set; } //-1 to 1 on each axis
    public static bool Fire { get; private set; } //held down
    public static bool FirePressed { get; private set; } //true only on the frame the fire button goes down

    //Sizes and positions are in canvas units, laid out for a 1920x1080 screen and scaled to fit
    private const float StickRadius = 130f;
    private const float KnobRadius = 55f;
    private const float ButtonRadius = 75f;
    private static readonly Vector2 StickRestPosition = new Vector2(260f, 260f); //from the bottom-left corner
    private static readonly Vector2 ButtonPosition = new Vector2(-230f, 230f); //from the bottom-right corner
    private static readonly Color ButtonColor = new Color(1f, 0.35f, 0.3f, 0.35f);
    private static readonly Color ButtonPressedColor = new Color(1f, 0.35f, 0.3f, 0.65f);

    //The fire button is only a hint (any touch outside the joystick fires), so it fades away once the player has used it a few times
    private const int TapsBeforeHidingButton = 1;
    private const float ButtonFadeSeconds = 1f;

    private Canvas canvas;
    private RectTransform stickBase;
    private RectTransform stickKnob;
    private Image buttonImage;
    private CanvasGroup buttonGroup; //fades the button and its label together
    private int fireTaps;
    private int stickFingerId = -1; //which finger is on the joystick (-1 = none)
    private GameObject rotatePopup; //covers the screen and pauses the game while the phone is held upright

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateOnMobile()
    {
        if (!DeviceApplication.isMobilePlatform) return; //also true in mobile browsers and in the Device Simulator
        new GameObject("MobileControls").AddComponent<MobileControls>();
    }

    void Awake()
    {
        Active = true;
        Input.simulateMouseWithTouches = false; //otherwise every touch, even on the joystick, counts as a left click and fires
        EnhancedTouchSupport.Enable(); //touches are read through the Input System, which the Device Simulator also feeds
        BuildControls();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; //never leave the game paused by the rotate popup
        EnhancedTouchSupport.Disable();
        Active = false;
        Stick = Vector2.zero;
        Fire = false;
        FirePressed = false;
    }

    void Update()
    {
        //The play area only fits sideways, so while the phone is upright: show the popup, pause, and ignore touches
        bool upright = DeviceScreen.height > DeviceScreen.width;
        if (upright != rotatePopup.activeSelf)
        {
            rotatePopup.SetActive(upright);
            Time.timeScale = upright ? 0f : 1f;
        }
        if (upright)
        {
            Stick = Vector2.zero;
            Fire = false;
            FirePressed = false;
            stickFingerId = -1;
            return;
        }

        bool stickHeld = false;
        bool fireHeld = false;

        foreach (Touch touch in Touch.activeTouches)
        {
            bool lifted = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            Vector2 canvasPosition = touch.screenPosition / canvas.scaleFactor;

            //A new touch on the left half grabs the joystick, which jumps to where the thumb landed
            if (touch.phase == TouchPhase.Began && stickFingerId == -1 && touch.screenPosition.x < DeviceScreen.width * 0.5f)
            {
                stickFingerId = touch.touchId;
                Vector2 canvasSize = new Vector2(DeviceScreen.width, DeviceScreen.height) / canvas.scaleFactor;
                stickBase.anchoredPosition = new Vector2(
                    Mathf.Clamp(canvasPosition.x, StickRadius, canvasSize.x - StickRadius),
                    Mathf.Clamp(canvasPosition.y, StickRadius, canvasSize.y - StickRadius)); //keep the ring fully on screen
            }

            if (touch.touchId == stickFingerId)
            {
                if (lifted)
                {
                    stickFingerId = -1;
                    continue;
                }
                stickHeld = true;
                Vector2 offset = Vector2.ClampMagnitude(canvasPosition - stickBase.anchoredPosition, StickRadius);
                stickKnob.anchoredPosition = offset;
                Stick = offset / StickRadius;
            }
            else if (!lifted)
            {
                fireHeld = true; //any other finger fires
            }
        }

        if (!stickHeld)
        {
            Stick = Vector2.zero;
            stickBase.anchoredPosition = StickRestPosition;
            stickKnob.anchoredPosition = Vector2.zero;
        }

        FirePressed = fireHeld && !Fire;
        Fire = fireHeld;
        UpdateFireButton();
    }

    void UpdateFireButton()
    {
        if (!buttonGroup.gameObject.activeSelf) return; //already faded away

        buttonImage.color = Fire ? ButtonPressedColor : ButtonColor;
        if (FirePressed) fireTaps++;

        if (fireTaps >= TapsBeforeHidingButton)
        {
            buttonGroup.alpha = Mathf.MoveTowards(buttonGroup.alpha, 0f, Time.deltaTime / ButtonFadeSeconds);
            if (buttonGroup.alpha <= 0f) buttonGroup.gameObject.SetActive(false);
        }
    }

    //Builds the joystick and fire button in code, so the scene doesn't need any setup
    void BuildControls()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; //on top of the HUD

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f; //size by screen height, so wide phones don't get tiny controls

        Sprite ring = CircleSprite(true);
        Sprite disc = CircleSprite(false);

        stickBase = CreateImage("Joystick", transform, ring, StickRadius, new Color(1f, 1f, 1f, 0.4f), Vector2.zero);
        stickBase.anchoredPosition = StickRestPosition;
        stickKnob = CreateImage("Knob", stickBase, disc, KnobRadius, new Color(1f, 1f, 1f, 0.55f), new Vector2(0.5f, 0.5f));

        RectTransform button = CreateImage("FireButton", transform, disc, ButtonRadius, ButtonColor, new Vector2(1f, 0f));
        button.anchoredPosition = ButtonPosition;
        buttonImage = button.GetComponent<Image>();
        buttonGroup = button.gameObject.AddComponent<CanvasGroup>();
        buttonGroup.blocksRaycasts = false;

        CreateLabel(button, "FIRE", 30f, new Color(1f, 1f, 1f, 0.85f));

        //"Turn your phone" popup: a dark full-screen panel, built last so it covers the controls
        rotatePopup = new GameObject("RotatePopup", typeof(RectTransform), typeof(Image));
        RectTransform popup = (RectTransform)rotatePopup.transform;
        popup.SetParent(transform, false);
        popup.anchorMin = Vector2.zero;
        popup.anchorMax = Vector2.one;
        popup.sizeDelta = Vector2.zero;
        Image dim = rotatePopup.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.85f);
        dim.raycastTarget = false;

        TextMeshProUGUI message = CreateLabel(popup, "TURN YOUR PHONE SIDEWAYS TO PLAY", 64f, Color.white);
        message.rectTransform.sizeDelta = new Vector2(-80f, 0f); //side margins, so it wraps instead of touching the edges
        message.enableAutoSizing = true; //shrinks to fit narrow phones
        message.fontSizeMin = 24f;
        message.fontSizeMax = 64f;
        rotatePopup.SetActive(false); //Update shows it when the phone is upright
    }

    TextMeshProUGUI CreateLabel(RectTransform parent, string text, float fontSize, Color color)
    {
        TextMeshProUGUI label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(parent, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.sizeDelta = Vector2.zero;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    RectTransform CreateImage(string name, Transform parent, Sprite sprite, float radius, Color color, Vector2 anchor)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = Vector2.one * radius * 2f;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false; //touches are read directly in Update, not through UI clicks
        return rect;
    }

    //Draws a smooth-edged white circle (filled, or just a ring) so no image files are needed
    static Sprite CircleSprite(bool ringOnly)
    {
        const int size = 128;
        const float ringThickness = 8f;
        float radius = size / 2f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                float alpha = Mathf.Clamp01(radius - distance); //solid inside, fading over the last pixel so the edge isn't jagged
                if (ringOnly) alpha *= Mathf.Clamp01(distance - (radius - ringThickness)); //hollow out the middle
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
