using UnityEngine;
using System;
using System.Collections.Generic;     // 리스트(List) 사용을 위해 추가.
using Random = UnityEngine.Random;     // Random이 Unity 엔진의 난수 생성기를 사용하도록 지정.

namespace Completed
{
    public class BoardManager : MonoBehaviour
    {
        // Serializable 특성을 사용하면 인스펙터(Inspector)에서 하위 속성을 가진 클래스를 임베드(embed)할 수 있습니다.
        [Serializable]
        public class Count
        {
            public int minimum;             // Count 클래스의 최소값.
            public int maximum;             // Count 클래스의 최대값.

            // 할당 생성자.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int columns = 8;                                     // 게임 보드의 열 수.
        public int rows = 8;                                        // 게임 보드의 행 수.
        public Count wallCount = new Count(5, 9);                   // 레벨 당 무작위 벽의 최소 및 최대 개수.
        public Count foodCount = new Count(1, 5);                   // 레벨 당 무작위 음식 아이템의 최소 및 최대 개수.
        public GameObject exit;                                     // 출구를 생성하기 위한 프리팹.
        public GameObject[] floorTiles;                             // 바닥 프리팹 배열.
        public GameObject[] wallTiles;                              // 벽 프리팹 배열.
        public GameObject[] foodTiles;                              // 음식 프리팹 배열.
        public GameObject[] enemyTiles;                             // 적 프리팹 배열.
        public GameObject[] outerWallTiles;                         // 외벽 타일 프리팹 배열.

        private Transform boardHolder;                              // Board 오브젝트의 Transform 참조를 저장하는 변수.
        private List<Vector3> gridPositions = new List<Vector3>();  // 타일을 배치할 수 있는 가능한 위치의 목록(List).

        // gridPositions 리스트를 초기화하고 새로운 보드를 생성할 준비를 합니다.
        void InitialiseList()
        {
            // gridPositions 리스트를 비웁니다.
            gridPositions.Clear();

            // x 축(열)을 순환합니다.
            for (int x = 1; x < columns - 1; x++)
            {
                // 각 열 내에서 y 축(행)을 순환합니다.
                for (int y = 1; y < rows - 1; y++)
                {
                    // 각 인덱스에 대해 현재 위치의 x 및 y 좌표를 사용하여 새 Vector3를 gridPositions 리스트에 추가합니다.
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }

        // 게임 보드의 외벽과 바닥(배경)을 설정합니다.
        void BoardSetup()
        {
            // Board를 인스턴스화하고 boardHolder를 해당 Transform으로 설정합니다.
            boardHolder = new GameObject("Board").transform;

            // x 축을 따라 반복하면서 -1부터 시작하여 모서리를 채울 때까지 floor 또는 outerwall edge 타일을 배치합니다.
            for (int x = -1; x < columns + 1; x++)
            {
                // y 축을 따라 반복하면서 -1부터 시작하여 floor 또는 outerwall 타일을 배치합니다.
                for (int y = -1; y < rows + 1; y++)
                {
                    // floorTiles 배열에서 무작위 타일을 선택하여 인스턴스화할 GameObject를 준비합니다.
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    // 현재 위치가 보드의 가장자리인지 확인하고, 그렇다면 outerWallTiles 배열에서 무작위 외벽 타일을 선택합니다.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                    // 선택한 프리팹을 Instantiate를 사용하여 루프에서 얻은 그리드 위치에 인스턴스화하고 GameObject로 캐스팅합니다.
                    GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    // 새로 인스턴스화한 객체의 부모를 boardHolder로 설정합니다. 이렇게 하면 게임 오브젝트 계층 구조가 깔끔하게 유지됩니다.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }

        // RandomPosition은 gridPositions 리스트에서 무작위 위치를 반환합니다.
        Vector3 RandomPosition()
        {
            // 정수 randomIndex를 선언하고 0부터 gridPositions 리스트의 항목 수 사이의 난수로 설정합니다.
            int randomIndex = Random.Range(0, gridPositions.Count);

            // Vector3 타입의 randomPosition 변수를 선언하고 gridPositions 리스트의 randomIndex 항목을 할당합니다.
            Vector3 randomPosition = gridPositions[randomIndex];

            // 사용한 항목을 리스트에서 제거하여 재사용되지 않도록 합니다.
            gridPositions.RemoveAt(randomIndex);

            // 무작위로 선택된 Vector3 위치를 반환합니다.
            return randomPosition;
        }

        // LayoutObjectAtRandom은 생성할 개체의 배열과 최소 및 최대 개체 수 범위를 입력받습니다.
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            // minimum과 maximum 범위 내에서 생성할 개체 수를 무작위로 선택합니다.
            int objectCount = Random.Range(minimum, maximum + 1);

            // 무작위로 선택한 개체 수 objectCount가 될 때까지 개체를 인스턴스화합니다.
            for (int i = 0; i < objectCount; i++)
            {
                // randomPosition에 gridPosition 리스트의 사용 가능한 Vector3 위치 중 하나를 얻어와서 위치를 선택합니다.
                Vector3 randomPosition = RandomPosition();

                // tileArray에서 무작위 타일을 선택하여 tileChoice에 할당합니다.
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                // RandomPosition에서 반환한 위치에 Quaternion.identity를 사용하여 tileChoice를 인스턴스화합니다.
                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }

        // SetupScene은 레벨을 초기화하고 이전 함수들을 호출하여 게임 보드를 배치합니다.
        public void SetupScene(int level)
        {
            // 외벽과 바닥을 생성합니다.
            BoardSetup();

            // gridPositions 리스트를 초기화합니다.
            InitialiseList();

            // wallCount의 최소 및 최대 개수를 기반으로 무작위 벽 타일을 인스턴스화합니다.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            // foodCount의 최소 및 최대 개수를 기반으로 무작위 음식 타일을 인스턴스화합니다.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            // 현재 레벨 번호에 기반하여 적의 개수를 결정합니다. 로그 함수를 사용하여 지수적으로 증가합니다.
            int enemyCount = (int)Mathf.Log(level, 2f);

            // enemyCount의 최소 및 최대 개수를 기반으로 무작위로 적 타일을 인스턴스화합니다.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

            // 게임 보드의 우측 상단에 출구 타일을 인스턴스화합니다.
            Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
        }
    }
}
