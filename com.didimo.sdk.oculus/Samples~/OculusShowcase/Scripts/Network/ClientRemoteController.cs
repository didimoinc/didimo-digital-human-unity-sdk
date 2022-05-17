using System.Collections.Generic;
using Unity.Netcode;
using Didimo;
using Didimo.Core.Animation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientRemoteController : NetworkBehaviour
{
    public GameObject[]                  ovrObjects;
    public Animator                      avatarAnimator;
    public VRArmsIK                      vrarmsik;
    public HandPoserVR                   handPoser;
    public OculusLipSyncVR               oculuslipsyncvr;
    public DidimoAnimator                didimoAnimator;
    public DidimoEyeShadowController     eyeShadowController;
    public DidimoIrisController          irisController;
    public LegacyAnimationPoseController legacyAnimationPoseController;
    public Animation                     animationComp;
    public AnimationSequencer            animationSequencer;
    public UIInstancer                   uIInstancer;

    public  NetworkVariable<int> slotID = new NetworkVariable<int>(-1);
    private DVRNetworkManager    dVR;

    public Material            faceMask;
    public SkinnedMeshRenderer faceSkin;

    public List<Renderer> skins;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOwner)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public override void OnNetworkSpawn()
    {
        dVR = FindObjectOfType<DVRNetworkManager>();

        if (slotID.Value != -1)
        {
            SlotIDValueChanged(-1, slotID.Value);
        }

        slotID.OnValueChanged += SlotIDValueChanged;

        if (IsOwner)
        {
            // This is spawned by the server, so we can't destroy it ourselves when changing scene.
            // It needs to be despawned by the server
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            ActivateComponents(true);
        }
        else
        {
            ActivateComponents(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        slotID.OnValueChanged -= SlotIDValueChanged;
        if (dVR != null)
        {
            dVR.bots[slotID.Value].SetActive(true);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        slotID.OnValueChanged -= SlotIDValueChanged;
    }

    private void ActivateComponents(bool activate)
    {
        foreach (var ovrObject in ovrObjects)
        {
            ovrObject.SetActive(activate);
        }

        vrarmsik.enabled = activate;
        avatarAnimator.enabled = activate;
        handPoser.enabled = activate;
        oculuslipsyncvr.enabled = activate;
        didimoAnimator.enabled = activate;
        irisController.enabled = activate;
        legacyAnimationPoseController.enabled = activate;
        animationComp.enabled = activate;
        animationSequencer.enabled = activate;
        uIInstancer.enabled = activate;

        if (activate)
        {
            skins.ForEach(s => s.gameObject.SetActive(false));
            faceSkin.sharedMaterial = faceMask;
        }
    }

    public void SlotIDValueChanged(int oldValue, int newValue) { dVR.bots[newValue].SetActive(false); }
}