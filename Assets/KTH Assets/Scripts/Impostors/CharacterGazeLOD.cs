using UnityEngine;
using System.Collections;

public enum LOD
{
    High,
    Medium,
    Low,
    Minimal
}

public class CharacterGazeLOD : MonoBehaviour
{
    public GameObject impostor;
    public GameObject characterMesh;
    private CharacterAnimation characterAnimation;
    private LOD currentLOD;
    private bool coolDown;
    private float coolDownTimer;

    public void Start()
    {
        characterAnimation = gameObject.GetComponent<CharacterAnimation>();
    }

    public void Update()
    {
        if (CoolDown()) {
            return;
        }

        LOD lod;

        if (impostor.activeSelf) {
            lod = GazeDistance.Instance.CalculateLOD(impostor);
        } else {
            lod = GazeDistance.Instance.CalculateLOD(characterMesh);
        }

        if (currentLOD == lod) {
            return;
        }

        if (lod == LOD.High) {
            SetCoolDown();
        }

        characterAnimation.SetLOD(lod);

        currentLOD = lod;
    }

    private void SetCoolDown()
    {
        coolDownTimer = Settings.LODcooldownTime;
        coolDown = true;
    }

    private bool CoolDown()
    {
        if (coolDown) {
            if ((coolDownTimer -= Time.deltaTime) < 0) {
                coolDown = false;
            }
            return true;
        }
        return false;
    }
}
