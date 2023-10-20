using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public VisualTreeAsset inspectorXML;

    private LevelGenerator levelGenerator;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();
        inspectorXML.CloneTree(inspector);

        VisualElement inspectorFoldout = inspector.Q("Default_Inspector");
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);

        levelGenerator = (LevelGenerator) target;

        VisualElement inspectorGenerate = inspector.Query("Generate");
        inspectorGenerate.RegisterCallback<ClickEvent>(_ =>
        {
            levelGenerator.Generate();
        });

        VisualElement inspectorClear = inspector.Query("Clear");
        inspectorClear.RegisterCallback<ClickEvent>(_ =>
        {
            levelGenerator.Clear();
        });

        return inspector;
    }
}
