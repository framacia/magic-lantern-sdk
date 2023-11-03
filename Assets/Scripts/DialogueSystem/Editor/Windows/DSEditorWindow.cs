using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DS.Windows
{
    using Utilities;

    public class DSEditorWindow : EditorWindow
    {
        private readonly string defaultFileName = "DialoguesFileName";

        [MenuItem("Window/DS/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AddStyles();
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddStyles();
        }

        #region Elements Addition
        private void AddGraphView()
        {
            DSGraphView graphView = new DSGraphView(this);
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            TextField fileNameTextField = DSElementUtility.CreateTextField(defaultFileName, "File Name:");

            Button saveButton = DSElementUtility.CreateButton("Save");

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }
        #endregion
    }
}

