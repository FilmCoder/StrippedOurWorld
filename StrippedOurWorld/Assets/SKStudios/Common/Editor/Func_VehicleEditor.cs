using UnityEditor;
using UnityEngine;

namespace SKStudios.Common.LevelBuilding {
    [CustomEditor(typeof(Func_Vehicle))]
    [InitializeOnLoad]
    public class FuncVehicleEditor : UnityEditor.Editor {
        private const float NodeAlpha = 0.5f;
        private const float HandleSize = 0.3f;
        private static readonly Color NodeColor = new Color(1, 1f, 1f, NodeAlpha);
        private static readonly Color NodeStartColor = new Color(0.748f, 0.6901961f, 0f, NodeAlpha);
        private static readonly Color NodeEndColor = new Color(0.1f, 0.1f, 0.9f, NodeAlpha);
        private VehicleNode _currentPreviewNode;
        private readonly float _defaultStepSize = 0.4f;
        private Vector3 _previewPosition;

        public FuncVehicleEditor()
        {
            SceneView.onSceneGUIDelegate -= SceneGui;
            SceneView.onSceneGUIDelegate += SceneGui;
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            _currentPreviewNode = ((Func_Vehicle) target).Base;
            _previewPosition = _currentPreviewNode.transform.position;
        }


        private void SceneGui(SceneView view)
        {
            var targetVehicle = (Func_Vehicle) target;
            if (targetVehicle == null)
                return;
            var currentNode = targetVehicle.Base;

            if (Event.current.type == EventType.Repaint)
                while (currentNode != null) {
                    if (currentNode.Next) {
                        var distance = Vector3.Distance(currentNode.Next.transform.position,
                            currentNode.transform.position);
                        var totalDistance = distance;
                        while (distance > 0) {
                            var oldDistance = distance;
                            distance -= _defaultStepSize;
                            distance = Mathf.Max(0, distance);
                            var currentDistance = oldDistance >= _defaultStepSize ? _defaultStepSize : oldDistance;
                            var heading = (currentNode.Next.transform.position - currentNode.transform.position)
                                .normalized;
                            var newPos = currentNode.transform.position + heading * (totalDistance - oldDistance);
                            Handles.color = Color.Lerp(Color.blue * 0.8f, Color.red * 0.8f,
                                currentNode.GetSpeedAtPosition(newPos));
                            Handles.ArrowHandleCap(0, newPos,
                                Quaternion.LookRotation(heading),
                                currentDistance, EventType.Repaint);
                        }
                    }

                    currentNode = currentNode.Next;
                }


            currentNode = targetVehicle.Base;
            while (currentNode != null)
                do {
                    Handles.lighting = true;
                    Handles.color = targetVehicle.Base == currentNode ? NodeStartColor :
                        targetVehicle.Tip == currentNode ? NodeEndColor : NodeColor;
                    if (Handles.Button(currentNode.transform.position, Quaternion.identity, HandleSize,
                        HandleSize / 2f,
                        Handles.SphereHandleCap))
                        Selection.activeGameObject = currentNode.gameObject;
                } while ((currentNode = currentNode.Next) != null);

            Handles.matrix = Matrix4x4.TRS(_previewPosition, targetVehicle.PlatformObj.transform.rotation,
                targetVehicle.PlatformObj.transform.lossyScale);
            Handles.CubeHandleCap(0, _previewPosition, Quaternion.identity, 1f, EventType.Repaint);
            targetVehicle.PlatformObj.transform.position = targetVehicle.Base.transform.position;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            var targetVehicle = (Func_Vehicle) target;
            _previewPosition = Func_Vehicle.NextPos(_currentPreviewNode, _previewPosition, out _currentPreviewNode);


            if (_currentPreviewNode == targetVehicle.Tip) {
                _currentPreviewNode = targetVehicle.Base;
                _previewPosition = _currentPreviewNode.transform.position;
            }
        }
    }
}