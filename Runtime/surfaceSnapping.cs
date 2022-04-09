// using UnityEditor;
// using UnityEngine;
//
// namespace NKStudio
// {
//     public class SurfaceSnapping : MonoBehaviour
//     {
//
//         //경로를 지정하고, true를 해서 안보이게 하자, %는 윈도우는 컨트롤, 맥은 커맨드 키에 해당된다.
//         [MenuItem("Edit/Surface Snapping _END")]
//         public static void Snap2Surface()
//         {
//             //Selection은 현재 에디터에서 선택된 오브젝트를 뜻한다.
//             foreach (var transform in Selection.transforms)
//             {
//                 //버텍스 Y높이가 제일 낮은 것을 찾습니다. (바닥부분의 버텍스 높이를 찾기 위함)
//                 Vector3 origin = GetMinYVertex(transform);
//                 var position = transform.position;
//                 origin.x = position.x;
//                 origin.z = position.z;
//
//                 //각각의 오브젝트의 위치에서 아래 방향으로 Ray를 쏜다.
//                 var hits = Physics.RaycastAll(origin, Vector3.down, 10f);
//
//                 //각각 hit정보 확인
//                 foreach (var hit in hits)
//                 {
//                     //자기 자신의 콜라이더를 맞춘 경우 pass : 예외 처리
//                     if (hit.collider.gameObject == transform.gameObject)
//                         continue;
//
//                     position.y -= hit.distance;
//
//                     //hit된 위치로 이동시킨다.
//                     transform.position = position;
//                     break;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 해당 객체의 버텍스중 Y값이 제일 작은 버텍스의 Word좌표 Vector3를 반환합니다.
//         /// </summary>
//         /// <param name="tr"></param>
//         /// <returns></returns>
//         private static Vector3 GetMinYVertex(Transform tr)
//         {
//             MeshFilter meshFilter = tr.GetComponent<MeshFilter>();
//
//             //매쉬렌더러가 없으면 객체 피봇 위치를 반환
//             if (meshFilter == null)
//                 return tr.transform.position;
//
//             var mesh = meshFilter.sharedMesh;
//
//             //Default로 버텍스0을 넣어줍니다.
//             //로컬좌표에 있는 매쉬 버텍스를 월드좌표로 변환합니다.
//             Vector3 minY = tr.TransformPoint(mesh.vertices[0]);
//
//             foreach (var point in mesh.vertices)
//             {
//                 //로컬좌표에 있는 버텍스을 월드좌표로 변환합니다.
//                 Vector3 worldPoint = tr.TransformPoint(point);
//
//                 if (minY.y > worldPoint.y)
//                     minY.y = worldPoint.y;
//             }
//
//             return minY;
//         }
//     }
// }