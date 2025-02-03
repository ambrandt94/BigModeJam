using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class WorldManager : Singleton<WorldManager> {
    private WorldGenerator generator;
    private AstarPath aStar;

    [SerializeField]
    private Transform gameFinishedLocation;
    [SerializeField]
    private GameObject playerObject;
    [SerializeField]
    private GameObject gameFinishCam;

    [Button("Trigger Game Finish")]
    public void TriggerGameFinish()
    {
        gameFinishCam.SetActive(true);
        Rigidbody playerBody = playerObject.GetComponent<Rigidbody>();
        FlyBehaviour fly = playerObject.GetComponent<FlyBehaviour>();
        fly.enabled = false;
        ThirdPersonOrbitCamBasic cam = playerObject.GetComponentInChildren<ThirdPersonOrbitCamBasic>();
        cam.enabled = false;
        playerObject.transform.position = gameFinishedLocation.transform.position;
        playerObject.transform.rotation = gameFinishedLocation.transform.rotation;
        playerBody.isKinematic = true;
        playerBody.useGravity = false;
        playerBody.angularVelocity = Vector3.zero;
        playerBody.linearVelocity = Vector3.zero;
        //Destroy(playerBody);
        Animator anim = playerObject.GetComponent<Animator>();
        anim.applyRootMotion = false;
        anim.SetBool("EndGame", true);
        anim.SetBool("Fly", false);
    }

    [Button("Generate Level")]
    public void Generate()
    {
        generator.OnFinished += ScanAstar;
        generator.Generate();
    }

    [Button("Scan Astar")]
    private void ScanAstar() {
        aStar.gameObject.SetActive(true);
        aStar.Scan();
    }

    IEnumerator LateStartRoutine()
    {
        yield return new WaitForSeconds(1);
        FlyBehaviour flyBehaviour = playerObject.GetComponent<FlyBehaviour>();
        flyBehaviour.ToggleFly();
    }

    private void Start()
    {
        StartCoroutine(LateStartRoutine());   
        generator = GetComponent<WorldGenerator>();
        aStar = FindObjectOfType<AstarPath>(true);
        aStar.scanOnStartup = false;
        Generate();
    }
}
