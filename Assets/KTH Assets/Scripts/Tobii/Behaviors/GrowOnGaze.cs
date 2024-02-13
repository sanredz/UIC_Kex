//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using Tobii.EyeX.Client;
using Tobii.EyeX.Client.Interop;
using Tobii.EyeX.Framework;
using UnityEngine;

/// <summary>
/// Unity script for a game object that grows dynamically when it senses the user's gaze.
/// </summary>
public class GrowOnGaze : EyeXInteractionBehaviour
{
    private static readonly Vector3 NormalScale = new Vector3(1.0f, 1.0f, 1.0f);
    private static readonly Vector3 LargeScale = new Vector3(1.5f, 1.5f, 1.5f);

    private bool _hasFocus;
    private float _scaleFactor = 0;

    public float speed = 10.0f;

    /// <summary>
    /// Update interactor bounds and transform
    /// </summary>
    public new void Update()
    {
        base.Update();

        // Update the scale factor depending on whether the eye gaze is on the object or not.
        if (_hasFocus)
        {
            _scaleFactor = Mathf.Clamp01(_scaleFactor + speed * Time.deltaTime);
        }
        else
        {
            _scaleFactor = Mathf.Clamp01(_scaleFactor - speed * Time.deltaTime);
        }

        transform.localScale = Vector3.Slerp(NormalScale, LargeScale, _scaleFactor);
    }

    protected override void OnEyeXEvent(string interactorId, InteractionEvent event_)
    {
        // NOTE: this method is called from a worker thread, so it must not access any game objects.
        // Therefore, we store the state in a variable and handle the state change in the Update() method.
        
        foreach (var behavior in event_.Behaviors)
        {
            GazeAwareEventParams eventData;
            if (behavior.TryGetGazeAwareEventParams(out eventData))
            {
                _hasFocus = eventData.HasGaze != EyeXBoolean.False;
            }
        }
    }
}
