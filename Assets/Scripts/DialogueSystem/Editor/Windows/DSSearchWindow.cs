using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Windows
{
    using Elements;
    using Enumerations;

    public class DSSearchWindow : ScriptableObject//, ISearchWindowProvider
    {
        private DSGraphView graphView;

        public void Initialize(DSGraphView dsGraphView)
        {
            graphView = dsGraphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice"))
                {
                    level = 2,
                    userData = DSDialogueType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice"))
                {
                    level = 2,
                    userData = DSDialogueType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group"))
                {
                    level = 2,
                    userData = new Group()
                }
            };

            return searchTreeEntries;
        }

        //public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        //{
        //    switch (SearchTreeEntry.userData)
        //    {
        //        case DSDialogueType.SingleChoice:
        //            {
        //                DSSingleChoiceNode singleChoiceNode = (DSSingleChoiceNode)graphView.CreateNode(DSDialogueType.SingleChoice, context.screenMousePosition);
        //                return true;
        //            }
        //        case DSDialogueType.MultipleChoice:
        //            {
        //                DSMultipleChoiceNode multipleChoiceNode = (DSMultipleChoiceNode)graphView.CreateNode(DSDialogueType.MultipleChoice, context.screenMousePosition);
        //                return true;
        //            }
        //        case Group _: //You can type underscore when you don't need to use the instance of a class in a case
        //            {
        //                return true;
        //            }
        //    }
        //}
    }
}
