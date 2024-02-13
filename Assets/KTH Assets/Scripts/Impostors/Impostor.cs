using UnityEngine;
using System.Collections;

public class Impostor : MonoBehaviour
{
    public int ShirtColor { get; set; }

    public int updateAnimationFrameCount = 2;
    public int updateRotationFrameCount = 5;
    private int numberOfAngles;
    private int halfOfAngles;
    private int quarterOfAngles;
    private int numberOfFrames;
    private int quality;
    private int frameRotation;
    private int frameAnimation;
    private Transform impostorTransform;
    private Transform parentTransform;
    private Renderer impostorRenderer;
    private CharacterAnimation characterAnimation;
    private Mesh quad;
    private int frameIndex;
    private int currentAngleIndexX;
    private int currentAngleIndexY;
    private bool minimalLOD;

    void Start()
    {
        numberOfAngles = Settings.numberOfAngles;
        halfOfAngles = Settings.numberOfAngles / 2;
        quarterOfAngles = Settings.numberOfAngles / 4;
        numberOfFrames = Settings.numberOfFrames;
        impostorTransform = transform;
        parentTransform = impostorTransform.parent;
        impostorRenderer = GetComponent<Renderer>();
        characterAnimation = transform.parent.GetComponent<CharacterAnimation>();
        quad = gameObject.GetComponent<MeshFilter>().mesh;
        quad.triangles = new int[]{2, 1, 0, 3, 0, 1}; // rotate quad faces
        quad.uv = Materials.GetUV(0, 0, 0);
        quality = Materials.MediumQuality;
        SetMaterial();
    }

    public void Update()
    {
        bool replaceUVs = false;
        if (minimalLOD) {
            LookAtCamera();
            return;
        }
        if (frameRotation >= updateRotationFrameCount) {
            frameRotation = 0;
            if (UpdateRotation()) {
                replaceUVs = true;
            }
            LookAtCamera();
        }
        if (frameAnimation >= updateAnimationFrameCount) {
            replaceUVs = true;
            frameAnimation = 0;
            UpdateAnimation();
        }
        if (replaceUVs) {
            quad.uv = Materials.GetUV(currentAngleIndexX, currentAngleIndexY, frameIndex);
        }
        frameRotation++;
        frameAnimation++;
    }

    public void ForcedUpdate()
    {
        UpdateRotation();
        LookAtCamera();
        UpdateAnimation();
        quad.uv = Materials.GetUV(currentAngleIndexX, currentAngleIndexY, frameIndex);
    }

    private void LookAtCamera()
    {
        impostorTransform.LookAt(Settings.cameraTransform.position);
    }
    
    public void UpdateAnimation()
    {
        SetAnimationPercentage(characterAnimation.NormalizedTime);
    }
    
    public bool UpdateRotation()
    {
        Vector3 cameraToObject = Settings.cameraTransform.position - parentTransform.position;
        int indexX = GetIndexX(cameraToObject);
        int indexY = GetIndexY(cameraToObject);
        if (indexY != currentAngleIndexY || indexX != currentAngleIndexX) {
            currentAngleIndexX = indexX;
            currentAngleIndexY = indexY;
            return true;
        }
        return false;
    }

    public void UpdateMaterial(int q)
    {
        if (quality == q) {
            return;
        }
        quality = q;
        SetMaterial();
    }
    
    private int GetIndexX(Vector3 cameraToObject)
    {
        Vector3 levelCameraToObject = cameraToObject;
        levelCameraToObject.y = 0;
        float angle = Vector3.Angle(cameraToObject, levelCameraToObject) - 10f;
        angle = Mathf.Max(angle, 0);    
        return Mathf.RoundToInt((angle / 90f) * (quarterOfAngles - 1));
    }

    public void SetMinimalLOD(bool enabled)
    {
        minimalLOD = enabled;
        if (enabled) {
            characterAnimation.NormalizedTime = 0;
        }
    }
    
    private int GetIndexY(Vector3 cameraToObject)
    {
        Vector3 parentRight = parentTransform.right;
        Vector3 parentForward = parentTransform.forward;
        parentRight.y = cameraToObject.y = parentForward.y = 0;
        float angle = Vector3.Angle(cameraToObject, parentForward);
        
        int index = Mathf.RoundToInt((angle * numberOfAngles) / 360f);
        if (index != 0 && index != halfOfAngles) {
            if (Vector3.Dot(cameraToObject, parentRight) > 0f) {
                index = numberOfAngles - index;
            }
        }
        return index;
    }
    
    public float GetAnimationPercentage()
    {
        return ((float)frameIndex / (float)numberOfFrames);
    }
    
    public void SetAnimationPercentage(float percent)
    {
        frameIndex = Mathf.RoundToInt(percent * numberOfFrames);
        frameIndex = (frameIndex == numberOfFrames) ? 0 : frameIndex;
    }
    
    private void SetMaterial()
    {
        impostorRenderer.sharedMaterial = Materials.GetMaterial(ShirtColor, quality);
    }
}
