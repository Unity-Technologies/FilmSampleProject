using UnityEngine;

namespace MWU.Shared.Utilities
{
    /// <summary>
    /// Debug draw bones in the editor
    /// </summary>
    public class ShowBones : MonoBehaviour
    {
        private Transform rootNode;
        private Transform[] childNodes;

        private void Start()
        {
            if (rootNode == null)
                rootNode = transform;
        }

        private void OnDrawGizmosSelected()
        {
            if (childNodes == null)
            {
                //get all joints to draw
                PopulateChildren();
            }

            foreach (var child in childNodes)
            {
                if (child == rootNode)
                {
                    //list includes the root, if root then larger, green cube
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(child.position, new Vector3(.1f, .1f, .1f));
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(child.position, child.parent.position);
                    Gizmos.DrawCube(child.position, new Vector3(.01f, .01f, .01f));
                }
            }
        }

        private void PopulateChildren()
        {
            childNodes = transform.GetComponentsInChildren<Transform>();
        }
    }
}