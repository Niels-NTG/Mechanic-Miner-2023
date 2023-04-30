using UnityEngine;

public class SimulationInstanceController : MonoBehaviour
{
    public LevelGenerator levelGenerator;
    public GameObject playerAgent;

    private PhysicsScene2D physicsScene;
    public float physicsTimeScale = 1f;
    
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
