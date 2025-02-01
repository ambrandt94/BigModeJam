using ChainLink.Core;
using Sirenix.OdinInspector;

public class WorldManager : Singleton<WorldManager> {
    private WorldGenerator generator;
    private AstarPath aStar;

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

    private void Start()
    {
        generator = GetComponent<WorldGenerator>();
        aStar = FindObjectOfType<AstarPath>(true);
        aStar.scanOnStartup = false;
        Generate();
    }
}
