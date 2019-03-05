// © 2018, SKStudios LLC. All Rights Reserved.
// 
// The software, artwork and data, both as individual files and as a complete software package known as 'PortalKit Pro', or 'MirrorKit Pro'
// without regard to source or channel of acquisition, are bound by the terms and conditions set forth in the Unity Asset 
// Store license agreement in addition to the following terms;
// 
// One license per seat is required for Companies, teams, studios or collaborations using PortalKit Pro and/or MirrorKit Pro that have over 
// 10 members or that make more than $10,000 USD per year. 
// 
// Addendum;
// If PortalKit Pro or MirrorKit pro constitute a major portion of your game's mechanics, please consider crediting the software and/or SKStudios.
// You are in no way obligated to do so, but it would be sincerely appreciated.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace SKStudios.Common.LevelBuilding {
    /// <summary>
    ///     A node for a <see cref="Func_Vehicle" /> to move along
    /// </summary>
    [ExecuteInEditMode]
    public class VehicleNode : MonoBehaviour {
        private static readonly HideFlags HideFlags = HideFlags.HideInHierarchy;

        [SerializeField] private Func_Vehicle _vehicle;

        [HideInInspector] public VehicleNode Next;
        [HideInInspector] public VehicleNode Prev;
        public float Speed = 1;

        public Func_Vehicle Vehicle {
            get {
                if (_vehicle == null) {
                    if (transform.parent == null)
                        return null;
                    _vehicle = transform.parent.GetComponent<Func_Vehicle>();
                    ;
                }

                return _vehicle;
            }
            set { _vehicle = value; }
        }

        public static VehicleNode CreateNode(Func_Vehicle vehicle)
        {
#if UNITY_EDITOR
            var obj = EditorUtility.CreateGameObjectWithHideFlags("Node", HideFlags);
            var newNode = obj.AddComponent<VehicleNode>();
            obj.transform.SetParent(vehicle.transform);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            newNode.Prev = vehicle.Tip;

            if (vehicle.Tip != null) {
                vehicle.Tip.Next = newNode;
                vehicle.Tip = newNode;
            }


            return newNode;
#endif
            return null;
        }

        public float GetSpeedAtPosition(Vector3 pos)
        {
            if (Next == null) return 0;
            var distance = Vector3.Distance(transform.position, Next.transform.position);
            return Mathf.Lerp(Speed, Next.Speed, Vector3.Distance(transform.position, pos) / distance) * Time.deltaTime;
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position, Vector3.one * 0.15f);
        }
#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            if (Selection.activeGameObject == Vehicle.gameObject) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white * 0.8f;
            Gizmos.DrawWireMesh(Vehicle.Mesh.sharedMesh);
        }
#endif

#if UNITY_EDITOR
        [SerializeField] private int _instanceId;

        private void Awake()
        {
            if (Application.isPlaying)
                return;

            if (_instanceId == 0) {
                _instanceId = GetInstanceID();
                return;
            }

            if (_instanceId != GetInstanceID() && GetInstanceID() < 0) {
                var oldObj =
#if UNITY_EDITOR
                    EditorUtility.InstanceIDToObject(_instanceId) as VehicleNode;
#else
                null;
#endif
                if (oldObj == null)
                    return;

                transform.SetParent(oldObj.transform.parent);
                Selection.activeGameObject = gameObject;
                _instanceId = GetInstanceID();

                if (oldObj == Vehicle.Tip) {
                    Prev = Vehicle.Tip;
                    Vehicle.Tip.Next = this;
                    Vehicle.Tip = this;
                }
                else if (oldObj == Vehicle.Base) {
                    Next = Vehicle.Base;
                    Vehicle.Base.Prev = this;
                    Vehicle.Base = this;
                }
                else {
                    Prev = oldObj;
                    Next = Prev.Next;
                    Next.Prev = this;
                    Prev.Next = this;
                }
            }
        }

        private void Start()
        {
            gameObject.hideFlags = HideFlags;
        }

        private void OnDestroy()
        {
            if (Vehicle) {
                if (Vehicle.Tip == this)
                    Vehicle.Tip = Prev;
                else if (Vehicle.Base == this) Vehicle.Base = Next;

                if (Next && Prev) {
                    Next.Prev = Prev;
                    Prev.Next = Next;
                }
            }
        }
#endif
    }
}