//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Tobii.EyeX.Client;
using Tobii.EyeX.Client.Interop;
using Tobii.EyeX.Framework;
using UnityEngine;
using Rect = UnityEngine.Rect;

/**
 * This script shows how the Activatable Behavior can be used with GUITextures
 * 
 * An activation can be triggered on an activatable interactor by looking at it
 * and pressing the activation key (by default the Applications key)
 * on the keyboard. The activation is triggered when the key is released, so the
 * user can move around their gaze point between different interactors while holding 
 * down the activation key, and then release the key to activate the one currently
 * looked at. 
 * 
 * While the activation key is pressed down, the interactor that the user looks at
 * has activation focus. An interactor with activation focus should be highlighted
 * so the user knows which interactor will be activated when the activation key
 * is released.
 * 
 * In a full screen menu situation where the user has to click one of the buttons,
 * it is appropriate to highlight buttons as soon as the user is looking at them.
 * This can be done using the tentative activation focus. 
 * 
 * For more information about the Activatable Behavior, see the Developer's Guide.
 */
public class ActivatableMenuItems : MonoBehaviour
{
    private EyeXHost _eyeXHost;
    private GameMenu _gameMenu;
    private readonly List<Transform> _menuItems = new List<Transform>();

    private string _activatedInteractorId = string.Empty;
    private string _focusedInteractorId = string.Empty;   // interactor looked at

    public bool showInteractorBounds = false;

    /// <summary>
    /// Initialize EyeX host and game menu on Awake
    /// </summary>
    public void Awake()
    {
        _eyeXHost = EyeXHost.GetInstance();

        _gameMenu = GameObject.Find("Game Menu").GetComponent<GameMenu>();

        // add all GUITextures under game menu to the list of menu items
        foreach (Transform childTransform in transform)
        {
            _menuItems.Add(childTransform);
        }
    }

    /// <summary>
    /// Register an activatable interactor for each menu item when the game object is enabled.
    /// </summary>
    public void OnEnable()
    {
        foreach (var menuItem in _menuItems)
        {
            var interactorId = menuItem.name;
            var interactor = new EyeXInteractor(interactorId, EyeXHost.NoParent, EyeXBehaviors.ActivatableWithTentativeFocus, OnEyeXEvent);
            interactor.Location = CreateLocation(menuItem, _gameMenu.IsVisible);
            _eyeXHost.RegisterInteractor(interactor);
        }
    }

    /// <summary>
    /// Unregister the interactors when the game object is disabled.
    /// </summary>
    public void OnDisable()
    {
        foreach (var menuItem in _menuItems)
        {
            var interactorId = menuItem.name;
            _eyeXHost.UnregisterInteractor(interactorId);
        }
    }

    /// <summary>
    /// Update interactor location and act on events
    /// </summary>
    void Update()
    {
        foreach (Transform menuItem in _menuItems)
        {
            var interactorId = menuItem.name;

            // Update location
            var interactor = _eyeXHost.GetInteractor(interactorId);
            interactor.Location = CreateLocation(menuItem, _gameMenu.IsVisible);

            // Check if activated
            if (_activatedInteractorId.Equals(interactorId))
            {
                switch (interactorId)
                {
                    case "Resume":
                        _gameMenu.IsVisible = false;
                        break;
                    case "Quit":
                        QuitApplication();
                        break;
                }

                _activatedInteractorId = string.Empty;
            }

            // Highlight the menu item the user looks at
            if (_focusedInteractorId.Equals(interactorId))
            {
                menuItem.GetComponent<GUITexture>().color = new Color(0.65f, 0.65f, 0.65f, 1);
            }
            else
            {
                menuItem.GetComponent<GUITexture>().color = Color.grey;
            }
        }
    }

    /// <summary>
    /// Draws interactor bounds if enabled
    /// </summary>
    public void OnGUI()
    {
        if (showInteractorBounds)
        {
            DrawInteractorBounds();
        }
    }

    private void DrawInteractorBounds()
    {
        foreach (Transform menuItem in _menuItems)
        {
            var interactorId = menuItem.GetInstanceID().ToString();
            var location = CreateLocation(menuItem, _gameMenu.IsVisible);
            if (location.isValid)
            {
                GUI.Box(location.rect, interactorId);
            }
        }
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnEyeXEvent(string interactorId, InteractionEvent event_)
    {
        // Note that this method is called on a worker thread, so we MAY NOT access any game objects from here.
        // Therefore, we store the state in variables and handle the state change in the Update() method.

        foreach (var behavior in event_.Behaviors)
        {
            ActivatableEventType eventType;
            if (behavior.TryGetActivatableEventType(out eventType))
            {
                if (eventType == ActivatableEventType.Activated)
                {
                    _activatedInteractorId = interactorId;
                }

                if (eventType == ActivatableEventType.ActivationFocusChanged)
                {
                    ActivationFocusChangedEventParams focusChangedParams;
                    if (behavior.TryGetActivationFocusChangedEventParams(out focusChangedParams))
                    {
                        // Since the only thing the user can do in the game menu is to
                        // click one of the buttons, we highlight as soon as the user
                        // looks at the button (tentative focus) and also while the
                        // user presses the activation key (activation focus).
                        var hasTentativeFocus = focusChangedParams.HasTentativeActivationFocus != EyeXBoolean.False;
                        var hasActivationFocus = focusChangedParams.HasActivationFocus != EyeXBoolean.False;
                        _focusedInteractorId = hasTentativeFocus || hasActivationFocus ? interactorId : string.Empty;
                    }
                }
            }
        }
    }

    private static ProjectedRect CreateLocation(Transform menuItem, bool isVisible)
    {
        var width = menuItem.GetComponent<GUITexture>().texture.width;
        var height = menuItem.GetComponent<GUITexture>().texture.height;
        var xPosition = menuItem.position.x * Screen.width + menuItem.GetComponent<GUITexture>().pixelInset.x;
        var yPositionUpperLeft = menuItem.position.y * Screen.height + menuItem.GetComponent<GUITexture>().pixelInset.y + height;
        var locationRect = new Rect(xPosition, Screen.height - yPositionUpperLeft, width, height);

        return new ProjectedRect() { isValid = isVisible, rect = locationRect, relativeZ = 1000 };
    }
}
