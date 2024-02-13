using UnityEngine;
using System.Collections;

public class CharacterAnimation : MonoBehaviour
{
    public float NormalizedTime { get; set; }
    public GameObject impostor;
    public GameObject characterMesh;

    private Animator animator;
    private LOD oldLOD;
    private Impostor impostorScript;

    public void Start()
    {
        animator = transform.Find("CharacterMesh").GetComponent<Animator>();
        impostorScript = impostor.GetComponent<Impostor>();
        DefaultSettings();
    }

    private void DefaultSettings()
    {
        oldLOD = LOD.Low;

        characterMesh.SetActive(false);
        impostor.SetActive(true);
    }

    public void Update()
    {
        NormalizedTime += Time.deltaTime;
        if (NormalizedTime > 1f) {
            NormalizedTime = NormalizedTime - (int)NormalizedTime;
        }
    }

    private void Impostor(LOD newLOD)
    {
        UpdateNormalizedTime();

        if (newLOD == LOD.Medium) {
            impostorScript.UpdateMaterial(Materials.MediumQuality);
        } else {
            impostorScript.UpdateMaterial(Materials.LowQuality);
            if (newLOD == LOD.Minimal) {
                impostorScript.SetMinimalLOD(true);
            }
        }

        characterMesh.SetActive(false);
        impostor.SetActive(true);
        impostorScript.ForcedUpdate();
    }

    private void Mesh()
    {
        impostor.SetActive(false);
        characterMesh.SetActive(true);
        animator.Play("WalkForward", 0, NormalizedTime);
    }

    public void SetLOD(LOD newLOD)
    {
        if (newLOD == LOD.High) {
            Mesh();
        } else {
            Impostor(newLOD);
        }

        if (oldLOD == LOD.Minimal) {
            impostorScript.SetMinimalLOD(false);
        }

        oldLOD = newLOD;
    }

    private void UpdateNormalizedTime()
    {
        NormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime -
            Mathf.Floor(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    }

}