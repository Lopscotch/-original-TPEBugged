using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public delegate void MainCameraChangeEvent(Camera camera);

    public static MainCameraChangeEvent OnMainCameraChange;

    public static void InvokeOnMainCameraChange(Camera camera)
    {
        if (OnMainCameraChange != null)
            InvokeOnMainCameraChange(camera);
    }

    [SerializeField]
    Camera m_MainCamera;
    [SerializeField]
    bool m_LockCursor = true;
    [SerializeField]
    bool m_UseFog = true;
    [SerializeField]
    Color m_Color;
    [SerializeField]
    internal float m_FogDensity = 0.02f;
    [SerializeField]
    internal float m_FogStartDepth = 0f;
    [SerializeField]
    internal float m_FogEndDepth = 1000f;

    [SerializeField]
    GameObject[] m_HUDMaskObjects;

    float m_DefaultFieldOfView;

    public Camera MainCamera { get { return m_MainCamera; } }

    public float DefaultFieldOfView { get { return m_DefaultFieldOfView; } }

    void Start()
    {
        MainEngine.AttachCameraHandler(this);

        if(m_UseFog)
            StartCoroutine(SetFog());

        PlayerOriginObject.OnPlayerSpawn += PlayerOriginObject_OnPlayerSpawn;

        OnMainCameraChange += CameraHandler_OnMainCameraChange;

        if (m_LockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDestroy()
    {
        PlayerOriginObject.OnPlayerSpawn -= PlayerOriginObject_OnPlayerSpawn;
    }

    private void CameraHandler_OnMainCameraChange(Camera camera)
    {
        ChangeMainCamera(camera);
    }

    private void PlayerOriginObject_OnPlayerSpawn(PlayerEntity player)
    {
        if (player.PlayerCamera != null)
            SetMainCamera(player.PlayerCamera);
    }

    internal void SetMainCamera(Camera camera)
    {
        if (camera != null)
        {
            m_MainCamera = camera;
            m_DefaultFieldOfView = m_MainCamera.fieldOfView;
        }
    }

    internal void ChangeMainCamera(Camera camera)
    {
        if (camera == null)
            return;

        if (m_MainCamera != null)
            m_MainCamera.enabled = false;

        if (!camera.enabled)
            camera.enabled = true;

        SetMainCamera(camera);
    }

    IEnumerator SetFog()
    {
        yield return new WaitForEndOfFrame();

        RenderSettings.fogDensity = m_FogDensity;
        RenderSettings.fogColor = m_Color;
        RenderSettings.fog = true;

        RenderSettings.fogStartDistance = m_FogStartDepth;
        RenderSettings.fogEndDistance = m_FogEndDepth;
    }

    public void SetFogActive(bool active)
    {
        RenderSettings.fog = active;
    }

    public void DisplayHudMask(int index)
    {
        if(m_HUDMaskObjects.Length > index)
        {
            if (m_HUDMaskObjects[index] != null)
                m_HUDMaskObjects[index].SetActive(true);
        }
    }

    public void HideHudMask(int index)
    {
        if (m_HUDMaskObjects.Length > index)
        {
            if (m_HUDMaskObjects[index] != null)
                m_HUDMaskObjects[index].SetActive(false);
        }
    }

    public void ZoomIn(float amt)
    {
        m_MainCamera.fieldOfView -= amt;
    }

    public void ZoomOut(float amt = 0)
    {
        if (amt == 0)
            m_MainCamera.fieldOfView = m_DefaultFieldOfView;

        else
            m_MainCamera.fieldOfView += amt;
    }
}
