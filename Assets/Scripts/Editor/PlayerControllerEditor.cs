using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    public VisualTreeAsset inspectorXML;
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();
        inspectorXML.CloneTree(inspector);

        VisualElement inspectorFoldout = inspector.Q("Default_Inspector");
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);

        PlayerController playerController = (PlayerController) target;

        VisualElement inspectorMoveLeft = inspector.Query("MoveLeft");
        inspectorMoveLeft.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.MoveLeft();
        });

        VisualElement inspectorMoveRight = inspector.Query("MoveRight");
        inspectorMoveRight.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.MoveRight();
        });

        VisualElement inspectorJump = inspector.Query("Jump");
        inspectorJump.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.Jump();
        });
        
        VisualElement inspectorSpecial = inspector.Query("Special");
        inspectorSpecial.RegisterCallback<ClickEvent>(_ =>
        {
            playerController.ToggleSpecial();
        });

        return inspector;
    }
}
