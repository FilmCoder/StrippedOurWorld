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

namespace SKStudios.Common.LevelBuilding {
    /// <summary>
    ///     func_vehicle 2005, boys and girls. It's back and worse than ever.
    ///     #resurrect func_vehicle
    /// </summary>
    [ExecuteInEditMode]
    // ReSharper disable once InconsistentNaming
    public class Func_Vehicle : MonoBehaviour {
        private VehicleNode _currentNode;


        [SerializeField] private MeshFilter _mesh;

        [SerializeField] private MeshRenderer _meshRend;

        public VehicleNode Base;

        public bool Moving;
        public GameObject PlatformObj;
        public VehicleNode Tip;

        public MeshFilter Mesh {
            get {
                if (_mesh == null)
                    _mesh = PlatformObj.GetComponent<MeshFilter>();
                return _mesh;
            }
        }

        public MeshRenderer MeshRend {
            get {
                if (_meshRend == null)
                    _meshRend = PlatformObj.GetComponent<MeshRenderer>();
                return _meshRend;
            }
        }


        private void Update()
        {
#if UNITY_EDITOR
            if (Tip == null)
                Tip = VehicleNode.CreateNode(this);

            if (Base == null) {
                var current = Tip;
                while (current.Prev != null) current = current.Prev;
                Base = current;
            }
#endif
        }

        private void FixedUpdate()
        {
            if (Moving)
                PlatformObj.GetComponent<Rigidbody>().MovePosition(
                    NextPos(_currentNode, PlatformObj.transform.position, out _currentNode));
            else
                return;

            if (_currentNode == Tip)
                Moving = false;
        }


        private void Start()
        {
            _currentNode = Base;
            PlatformObj.transform.position = Base.transform.position;
        }

        public static Vector3 NextPos(VehicleNode a, Vector3 position, out VehicleNode node)
        {
            if (!a.Next) {
                node = a;
                return position;
            }

            var speed = a.GetSpeedAtPosition(position);
            var newPos = (a.Next.transform.position - a.transform.position).normalized * speed +
                         position;
            node = Vector3.Distance(newPos, a.transform.position) >
                   Vector3.Distance(a.transform.position, a.Next.transform.position)
                ? a.Next
                : a;
            return newPos;
        }
    }
}