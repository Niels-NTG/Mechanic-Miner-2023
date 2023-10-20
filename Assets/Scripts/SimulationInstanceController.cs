using UnityEngine;

public class SimulationInstanceController : MonoBehaviour
{
    public LevelGenerator levelGenerator;
    public GameObject playerAgent;
    public TextMesh playerDebugLabel;

    private PhysicsScene2D physicsScene;
    [Range(1, 20)] public float physicsTimeScale = 10f;

    private void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        physicsScene = gameObject.scene.GetPhysicsScene2D();
    }

    private void FixedUpdate()
    {
        physicsScene.Simulate(Time.fixedDeltaTime * physicsTimeScale);
    }
}
