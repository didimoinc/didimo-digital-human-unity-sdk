using System.Collections.Generic;
using Unity.Netcode;
using Didimo;
using Didimo.Core.Animation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientRemoteController : NetworkBehaviour
{
    public GameObject[] ovrObjects;
    public Animator avatarAnimator;
    public VRArmsIK vrarmsik;
    public HandPoserVR handPoser;
    public OculusLipSyncVR oculuslipsyncvr;
    public DidimoAnimator didimoAnimator;
    public DidimoEyeShadowController eyeShadowController;
    public DidimoIrisController irisController;
    public LegacyAnimationPoseController legacyAnimationPoseController;
    public Animation animationComp;
    public AnimationSequencer animationSequencer;
    public UIInstancer uIInstancer;

    public NetworkVariable<int> slotID = new NetworkVariable<int>(-1);
    private DVRNetworkManager dVR;

    public Material faceMask;
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
            ActivateComponents();
        }
        else
        {
            DeactivateComponents();
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

    private void DeactivateComponents()
    {
        foreach (var ovrObject in ovrObjects)
        {
            ovrObject.SetActive(false);
        }

        vrarmsik.enabled = false;
        avatarAnimator.enabled = false;
        handPoser.enabled = false;
        oculuslipsyncvr.enabled = false;
        didimoAnimator.enabled = false;
        irisController.enabled = false;
        legacyAnimationPoseController.enabled = false;
        animationComp.enabled = false;
        animationSequencer.enabled = false;
        uIInstancer.enabled = false;
    }

    private void ActivateComponents()
    {
        foreach (var ovrObject in ovrObjects)
        {
            ovrObject.SetActive(true);
        }

        vrarmsik.enabled = true;
        avatarAnimator.enabled = true;
        handPoser.enabled = true;
        oculuslipsyncvr.enabled = true;
        didimoAnimator.enabled = true;
        irisController.enabled = true;
        legacyAnimationPoseController.enabled = true;
        animationComp.enabled = true;
        animationSequencer.enabled = true;
        uIInstancer.enabled = true;

        skins.ForEach(s => s.gameObject.SetActive(false));
        faceSkin.sharedMaterial = faceMask;
    }

    public void SlotIDValueChanged(int oldValue, int newValue)
    {
        dVR.bots[newValue].SetActive(false);
    }
}