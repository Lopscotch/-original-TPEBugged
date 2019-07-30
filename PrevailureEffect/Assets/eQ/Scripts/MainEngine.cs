using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEngine : MonoBehaviour
{
    static List<CharacterController> m_PlayerDictionary = new List<CharacterController>();

    public List<CharacterController> PlayerDictionary { get { return m_PlayerDictionary; } }

    public delegate void MainUpdateEvent();
    public static event MainUpdateEvent OnMainUpdate;

    public static void InvokeOnUpdate()
    {
        if (OnMainUpdate != null)
            OnMainUpdate.Invoke();
    }

    public delegate void OnFixedUpdate();
    public static event OnFixedUpdate OnMainFixedUpdate;

    public static void InvokeOnFixedUpdate()
    {
        if (OnMainFixedUpdate != null)
            OnMainFixedUpdate.Invoke();
    }

    public delegate void OnMainEnabledEvent();
    public static event OnMainEnabledEvent OnMainEnable;

    public static void InvokenMainEnable ()
    {
        if (OnMainEnable != null)
            OnMainEnable.Invoke();
    }

    void Update()
    {
        InvokeOnUpdate();
    }

    private void FixedUpdate()
    {
        InvokeOnFixedUpdate();
    }

    private void OnEnable()
    {
        InvokenMainEnable();
    }
}
