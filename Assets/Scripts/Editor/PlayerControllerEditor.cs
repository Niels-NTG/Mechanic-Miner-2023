using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    public VisualTreeAsset inspectorXML;
    
    private PlayerController playerController;
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();
        inspectorXML.CloneTree(inspector);

        VisualElement inspectorFoldout = inspector.Q("Default_Inspector");
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);
        
        playerController = (PlayerController) target;

        VisualElement inspectorMoveLeft = inspector.Query("MoveLeft");
        inspectorMoveLeft.RegisterCallback<ClickEvent>(_ =>
        {
            for (int i = 0; i < 120; i++)
            {
                playerController.rigidBody.AddForce(
                    playerController.MoveLeft()
                );    
            }
        });

        VisualElement inspectorMoveRight = inspector.Query("MoveRight");
        inspectorMoveRight.RegisterCallback<ClickEvent>(_ =>
        {
            for (int i = 0; i < 120; i++)
            {
                playerController.rigidBody.AddForce(
                    playerController.MoveRight()
                );    
            }
        });

        VisualElement inspectorJump = inspector.Query("Jump");
        inspectorJump.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.rigidBody.AddForce(
                playerController.Jump()
            );
        });
        
        VisualElement inspectorSpecial = inspector.Query("Special");
        inspectorSpecial.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.ToggleSpecial();
        });

        return inspector;
    }
}
