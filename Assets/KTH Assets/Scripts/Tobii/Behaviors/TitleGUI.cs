//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;
﻿using System.Collections;
using UnityEngine;
using Rect = UnityEngine.Rect;

/**
 * This script shows how the Activatable Behavior can be used with GUI.Button
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
 * For more information about the Activatable Behavior, see the Developer's Guide.
 */
public class TitleGUI : MonoBehaviour
{
    private const double Z = 1000;
    private const string SpinButtonId = "spin";
    private const string StopButtonId = "stop";
    private const int MenuHintWidth = 270;
    private const int MenuHintHeight = 60;
    private const int MenuHintMargin = 10;

    private static readonly Rect MenuBounds = new Rect(10, 10, 170, 250);
    private static readonly Rect SpinButtonBounds = new Rect(20, 40, 150, 100);
    private static readonly Rect StopButtonBounds = new Rect(20, 150, 150, 100);

    private EyeXHost _eyeXHost;
    private string _activatedInteractorId;
    private string _activationFocusInteractorId; // interactor looked at while pressing down activation key
    private bool _shouldClearFocus;

    public GameObject target;

    public void Awake()
    {
        _eyeXHost = EyeXHost.GetInstance();
    }

    public void OnEnable()
    {
        // Register activatable interactors for the GUI buttons when the game object is enabled.
        var spinInteractor = new EyeXInteractor(SpinButtonId, EyeXHost.NoParent, EyeXBehaviors.Activatable, OnEyeXEvent);
        spinInteractor.Location = CreateLocation(SpinButtonBounds);
        _eyeXHost.RegisterInteractor(spinInteractor);

        var stopInteractor = new EyeXInteractor(StopButtonId, EyeXHost.NoParent, EyeXBehaviors.Activatable, OnEyeXEvent);
        stopInteractor.Location = CreateLocation(StopButtonBounds);
        _eyeXHost.RegisterInteractor(stopInteractor);
    }

    public void OnDisable()
    {
        // Unregister the interactors when the game object is disabled.
        _eyeXHost.UnregisterInteractor(SpinButtonId);
        _eyeXHost.UnregisterInteractor(StopButtonId);
    }

    public void OnGUI()
    {
        if (_shouldClearFocus)
        {
            GUI.FocusControl("");
            _shouldClearFocus = false;
        }

        // Draw the GUI.
        GUI.Box(MenuBounds, "GUI demo");

        // Draw Spin button, and set up handling for it
        GUI.SetNextControlName(SpinButtonId);
        if (GUI.Button(SpinButtonBounds, "Take it for a spin") ||   // Either the button has been clicked,
            _activatedInteractorId == SpinButtonId)                 // or the corresponding interactor has been activated
        {
            StartCoroutine("ShowActivationFeedback", SpinButtonId);
            StartSpinning();
        }
        // else, if user is looking at button while pressing down activation key
        else if (_activationFocusInteractorId == SpinButtonId)
        {
            GUI.FocusControl(SpinButtonId);
        }

        // Draw Stop button, and set up handling for it
        GUI.SetNextControlName(StopButtonId);
        if (GUI.Button(StopButtonBounds, "Stop it") ||  // Either the button has been clicked,
            _activatedInteractorId == StopButtonId)     // or the corresponding interactor has been activated
        {
            StartCoroutine("ShowActivationFeedback", StopButtonId);
            StopSpinning();
        }
        // else, if user is looking at button while pressing down activation key
        else if (_activationFocusInteractorId == StopButtonId)
        {
            GUI.FocusControl(StopButtonId);
        }

        // Draw instructions label
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.Label(new Rect(Screen.width - MenuHintWidth - MenuHintMargin, MenuHintMargin, MenuHintWidth, MenuHintHeight),
            "Activate a button by pressing the Applications key while looking at the button.\nPress space bar to open the game menu.");

        // Make sure activation events are only handled once.
        _activatedInteractorId = null;
    }

    private void StartSpinning()
    {
        print("Start spinning command given");
        var rigidbody = target.GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        rigidbody.GetComponent<ConstantForce>().enabled = true;
    }

    private void StopSpinning()
    {
        print("Stop spinning command given");
        var rigidbody = target.GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }

    private IEnumerator ShowActivationFeedback(string buttonId)
    {
        GUI.FocusControl(buttonId);
        yield return new WaitForSeconds(0.1f);
        _shouldClearFocus = true;
    }

    private static ProjectedRect CreateLocation(Rect bounds)
    {
        return new ProjectedRect { isValid = true, rect = bounds, relativeZ = Z };
    }

    private void OnEyeXEvent(string interactorId, InteractionEvent event_)
    {
        // NOTE: this method is called from a worker thread, so it must not access any game objects.
        // Therefore, we store the state in a variable and handle the state change in the Update() method.

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
                        // Since the GUI buttons are visible during game play, we need to take care when to
                        // give the user visual feedback: The user may just glance over the GUI buttons without wanting
                        // to click any of them, so we only set a highlight while the activation button is pressed down,
                        // because then we know the user wants to click one of the buttons.
                        var hasActivationFocus = focusChangedParams.HasActivationFocus != EyeXBoolean.False;
                        _activationFocusInteractorId = hasActivationFocus ? interactorId : string.Empty;
                    }
                }
            }
        }
    }
}
