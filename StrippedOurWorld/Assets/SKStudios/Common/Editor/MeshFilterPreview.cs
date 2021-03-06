﻿using UnityEditor;
using UnityEngine;

namespace SKStudios.Common.Editor {
    [CustomEditor(typeof(MeshFilter))]
    [CanEditMultipleObjects]
    public class MeshFilterPreview : UnityEditor.Editor {
        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_previewRenderUtility != null)
                _previewRenderUtility.Cleanup();
        }

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            var controlId = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            var current = Event.current;
            switch (current.GetTypeForControl(controlId)) {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f) {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId) GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId) {
                        scrollPosition -= current.delta * (!current.shift ? 1 : 3) /
                                          Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }

                    break;
            }

            return scrollPosition;
        }

        #region MeshFilterPreview

        private PreviewRenderUtility _previewRenderUtility;


        public virtual MeshFilter TargetMeshFilter { get; set; }

        public virtual MeshRenderer TargetMeshRenderer { get; set; }

        private Vector2 _drag;

        private void ValidateData()
        {
            if (_previewRenderUtility == null)
                _previewRenderUtility = new PreviewRenderUtility();

            if (!TargetMeshFilter)
                TargetMeshFilter = target as MeshFilter;

            if (!TargetMeshRenderer && TargetMeshFilter)
                TargetMeshRenderer = TargetMeshFilter.GetComponent<MeshRenderer>();
        }

        public bool HasPreviewGUI_s()
        {
            ValidateData();
            return true;
        }

        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
                _drag = Vector2.zero;
        }

        public Transform MoveCamera()
        {
            //TargetMeshFilter.transform.localPosition = new Vector3(0, 0.05f, -0.383f);
            TargetMeshFilter.transform.localRotation = Quaternion.identity;
            TargetMeshFilter.transform.localScale = Vector3.one;

            var targetTransform = ((Component) target).transform;

            for (var i = 0; i < TargetMeshRenderer.sharedMaterials.Length; i++)
                _previewRenderUtility.DrawMesh(TargetMeshFilter.sharedMesh, targetTransform.localToWorldMatrix,
                    TargetMeshRenderer.sharedMaterials[i], i);

            var previewCam = _previewRenderUtility.camera;
            if (previewCam == null)
                return null;
            previewCam.transform.position =
                targetTransform.position /*- targetTransform.up * 0.3f + targetTransform.forward * 0.4f;*/;

            previewCam.farClipPlane = 1000;
            previewCam.nearClipPlane = 0.1f;
            previewCam.fieldOfView = 20f;
            previewCam.ResetProjectionMatrix();

            //Set to match rotation of renderer to preview first
            previewCam.transform.rotation = targetTransform.rotation;
            previewCam.transform.rotation *= Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));

            previewCam.transform.position +=
                previewCam.transform.forward * -(3f * targetTransform.lossyScale.magnitude);
            return previewCam.transform;
        }

        public void OnPreviewGUI_s(Rect r, GUIStyle background)
        {
            _drag = Drag2D(_drag, r);
            if (Event.current.type == EventType.Repaint) {
                if (TargetMeshRenderer == null) {
                    EditorGUI.DropShadowLabel(r, "Mesh Renderer Required");
                }
                else {
                    if (_previewRenderUtility == null)
                        _previewRenderUtility = new PreviewRenderUtility();

                    _previewRenderUtility.BeginPreview(r, null);


                    _previewRenderUtility.camera.Render();
                    var resultRender = _previewRenderUtility.EndPreview();

                    GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
                }
            }
        }

        #endregion
    }
}