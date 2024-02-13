//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------
using Tobii.EyeX.Client;

/// <summary>
/// Unity script for a game object that occludes other EyeX interactors.
/// </summary>
public class Occluder : EyeXInteractionBehaviour 
{
    protected override void OnEyeXEvent(string interactorid, InteractionEvent event_)
    {
        // Do nothing, just occlude other interactors.
    }
}
