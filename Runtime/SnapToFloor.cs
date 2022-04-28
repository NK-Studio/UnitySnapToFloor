using System;
using UnityEditor;
using UnityEngine;

namespace NKStudio.UnitySnapToFloor
{
    public class SnapToFloor : MonoBehaviour
    {
        private enum ResultType
        {
            X,
            Z
        }

        //스냅이 허용되는 높이
        private const float Height = 1000f;

        //경로를 지정하고, true를 해서 안보이게 하자, %는 윈도우는 컨트롤, 맥은 커맨드 키에 해당된다.
        [MenuItem("Edit/SnapToFloor _END")]
        public static void Snap2Surface()
        {
            int id = EditorPrefs.GetInt("SettingsInstanceID", -1);
            string path = AssetDatabase.GetAssetPath(id);
            if (string.IsNullOrEmpty(path))
            {
                bool isKr = Application.systemLanguage == SystemLanguage.Korean;
                Debug.LogError(isKr
                    ? "SnapToFloor 세팅에 Settings Asset이 참조되어있지 않습니다."
                    : "Settings Asset is not referenced in the SnapToFloor settings.");
                return;
            }
            
            //Selection은 현재 에디터에서 선택된 오브젝트를 뜻한다.
            foreach (var transform in Selection.transforms)
            {
#if SNAP2FLOOR_2D
                SpriteRenderer[] hasSpriteRenders = transform.GetComponentsInChildren<SpriteRenderer>();
                bool hasChildSpriteRender = false;
                Collider2D hasCollider = transform.GetComponent<Collider2D>();

                //자식이 SpriteRender를 가지고 있는지 체크합니다.
                foreach (var spriteRenderer in hasSpriteRenders)
                {
                    if (spriteRenderer != null)
                    {
                        hasChildSpriteRender = true;
                        goto FindSpriteRender;
                    }
                }

                FindSpriteRender:
                //경고 메세지
                if (!hasChildSpriteRender && hasCollider == null)
                {
                    Debug.LogError("Could not find sprite renderer and collider 2D");
                    return;
                }
                else if (hasCollider == null)
                {
                    Debug.LogError("Could not find collider 2D");
                    return;
                }
                else if (!hasChildSpriteRender)
                {
                    Debug.LogError("Could not find sprite renderer");
                    return;
                }

                #region 매쉬의 버텍스에 대한 월드 계산 위치

                Vector3 minMaxDistance = GetMinMaxRangeByVertex2D(transform);

                float footYPosition = GetMinYVertex2D(transform);

                #endregion

                float distance = minMaxDistance.z;

                float startPosition = minMaxDistance.x;

                //간격에 따른 알맞는 간격을 계산한다.
                float intervalValue = CalculateSeparationByDistance(distance);

                //간격에 알맞는 알갱이를 가져옴
                int numberOfGrains = CalculatePointCount(distance, intervalValue);
                int nowNumberOfGrains = numberOfGrains + 1;

                Vector3 position = transform.position;
                float? moveY = null;

                //원하는 알갱이 수 만큼 반복한다
                //- 원래는 내가 원하는 간격을 제시하면 그것에 맞는 알갱이를 뿌린다.
                for (int i = 0; i < nowNumberOfGrains; i++)
                {
                    //그려낼 위치에서 사이간격에 맞춰 그려냄
                    float xx = startPosition + intervalValue * i;

                    Vector3 drawPosition = new Vector3(xx, footYPosition, position.z);

                    //각각의 오브젝트의 위치에서 아래 방향으로 Ray를 쏜다.
                    RaycastHit2D[] hits = Physics2D.RaycastAll(drawPosition, Vector2.down, Height);

                    //각각 hit정보 확인
                    foreach (var hit in hits)
                    {
                        //자기 자신의 콜라이더를 맞춘 경우 pass : 예외 처리
                        if (hit.collider.gameObject == transform.gameObject)
                            continue;

                        if (moveY == null)
                            moveY = hit.distance;
                        else
                        {
                            if (moveY > hit.distance)
                                moveY = hit.distance;
                        }
                    }
                }

                position.y -= moveY ?? 0f;
                //hit된 위치로 이동시킨다.
                transform.position = position;
#endif
#if SNAP2FLOOR_3D
                #region 매쉬의 버텍스에 대한 월드 계산 위치

                Vector2 minMaxByX = GetMinMaxRangeByVertex3D(ResultType.X, transform);
                var vx1 = Vector3.zero;
                vx1.x = minMaxByX.x;

                var vx2 = Vector3.zero;
                vx2.x = minMaxByX.y;

                var minMaxByZ = GetMinMaxRangeByVertex3D(ResultType.Z, transform);
                var vz1 = Vector3.zero;
                vz1.x = minMaxByZ.x;

                var vz2 = Vector3.zero;
                vz2.x = minMaxByZ.y;

                float footYPosition = GetMinYVertex3D(transform);

                #endregion

                float distanceX = Vector3.Distance(vx1, vx2);
                float distanceZ = Vector3.Distance(vz1, vz2);

                float startPositionX = minMaxByX.x;
                float startPositionZ = minMaxByZ.x;

                //간격에 따른 알맞는 간격을 계산한다.
                float intervalValueX = CalculateSeparationByDistance(distanceX);
                float intervalValueZ = CalculateSeparationByDistance(distanceZ);

                //간격에 알맞는 알갱이를 가져옴
                int numberOfGrainsX = CalculatePointCount(distanceX, intervalValueX);
                int numberOfGrainsZ = CalculatePointCount(distanceZ, intervalValueZ);
                int nowNumberOfGrainsX = numberOfGrainsX + 1;
                int nowNumberOfGrainsZ = numberOfGrainsZ + 1;

                Vector3 position = transform.position;
                float? moveY = null;
                
                //원하는 알갱이 수 만큼 반복한다
                //- 원래는 내가 원하는 간격을 제시하면 그것에 맞는 알갱이를 뿌린다.
                for (int i = 0; i < nowNumberOfGrainsX; i++)
                {
                    for (int j = 0; j < nowNumberOfGrainsZ; j++)
                    {
                        //그려낼 위치에서 사이간격에 맞춰 그려냄
                        var xx = startPositionX + intervalValueX * i;
                        var zz = startPositionZ + intervalValueZ * j;

                        var drawPosition = new Vector3(xx, footYPosition, zz);

                        //각각의 오브젝트의 위치에서 아래 방향으로 Ray를 쏜다.
                        var hits = Physics.RaycastAll(drawPosition, Vector3.down, Height);

                        //각각 hit정보 확인
                        foreach (var hit in hits)
                        {
                            //자기 자신의 콜라이더를 맞춘 경우 pass : 예외 처리
                            if (hit.collider.gameObject == transform.gameObject)
                                continue;

                            if (moveY == null)
                                moveY = hit.distance;
                            else
                            {
                                if (moveY > hit.distance)
                                    moveY = hit.distance;
                            }
                        }
                    }
                }
                position.y -= moveY ?? 0f;
                //hit된 위치로 이동시킨다.
                transform.position = position;
#endif
            }
        }

        /// <summary>
        /// x,y는 min,max를 반환하고, z는 두 점의 거리를 반환합니다.
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        private static Vector3 GetMinMaxRangeByVertex2D(Transform tr)
        {
            Collider2D boxCollider2D = tr.GetComponent<Collider2D>();

            Bounds bounds = boxCollider2D.bounds;
            float y = bounds.min.y;

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            max.y = y;

            var distance = Vector2.Distance(min, max);

            return new Vector3(min.x, max.x, distance);
        }

        /// <summary>
        /// 바운딩 박스의 minY를 반환합니다.
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        private static float GetMinYVertex2D(Transform tr)
        {
            Collider2D boxCollider2D = tr.GetComponent<Collider2D>();

            Bounds bounds = boxCollider2D.bounds;

            float worldY = bounds.min.y;

            return worldY;
        }

        private static Vector2 GetMinMaxRangeByVertex3D(ResultType resultType, Transform tr)
        {
            var meshFilter = tr.GetComponent<MeshFilter>();

            //스키니드 매쉬 렌더러를 가진 경우 발 밑에서 쏘도록 처리합니다.
            if (meshFilter == null)
            {
                var position = tr.position;
                return resultType switch
                {
                    ResultType.X => new Vector2(position.x, position.x),
                    ResultType.Z => new Vector2(position.z, position.z),
                    _ => throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null)
                };
            }

            var mesh = meshFilter.sharedMesh;

            //Default로 버텍스 0을 넣어봅니다.
            //로컬좌표에 있는 Vertical 0을 월드좌표로 변환합니다.
            float min;
            float max;

            //초기화
            switch (resultType)
            {
                case ResultType.X:
                    min = tr.TransformPoint(mesh.vertices[0]).x;
                    max = tr.TransformPoint(mesh.vertices[0]).x;
                    break;
                case ResultType.Z:
                    min = tr.TransformPoint(mesh.vertices[0]).z;
                    max = tr.TransformPoint(mesh.vertices[0]).z;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
            }

            foreach (var point in mesh.vertices)
            {
                //로컬좌표에 있는 Vertical 0을 월드좌표로 변환합니다.
                Vector3 worldPoint = tr.TransformPoint(point);

                switch (resultType)
                {
                    case ResultType.X:
                    {
                        if (min > worldPoint.x)
                            min = worldPoint.x;

                        if (max < worldPoint.x)
                            max = worldPoint.x;
                        break;
                    }
                    case ResultType.Z:
                    {
                        if (min > worldPoint.z)
                            min = worldPoint.z;

                        if (max < worldPoint.z)
                            max = worldPoint.z;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
                }
            }

            return new Vector2(min, max);
        }

        private static float GetMinYVertex3D(Transform tr)
        {
            MeshFilter meshFilter = tr.GetComponent<MeshFilter>();

            //매쉬렌더러가 없으면 객체 피봇 위치를 반환
            if (meshFilter == null)
                return tr.position.y;

            var mesh = meshFilter.sharedMesh;

            //Default로 버텍스0을 넣어줍니다.
            //로컬좌표에 있는 매쉬 버텍스를 월드좌표로 변환합니다.
            Vector3 minY = tr.TransformPoint(mesh.vertices[0]);

            foreach (var point in mesh.vertices)
            {
                //로컬좌표에 있는 버텍스을 월드좌표로 변환합니다.
                Vector3 worldPoint = tr.TransformPoint(point);

                if (minY.y > worldPoint.y)
                    minY.y = worldPoint.y;
            }

            return minY.y;
        }

        private static float CalculateSeparationByDistance(float distance)
        {
            float numberOfGrain;
            var sampling = 3; //기본 샘플링
            do
            {
                sampling++;
                numberOfGrain = distance / sampling;
            } while (numberOfGrain > 0.2f);

            return numberOfGrain;
        }

        private static int CalculatePointCount(float distance, float intervalValue)
        {
            var result = (int) (distance / intervalValue);
            if (result <= 0)
                result = 0;
            return result;
        }
    }
}