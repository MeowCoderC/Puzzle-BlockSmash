namespace BlockSmash
{
    using UnityEngine;
    using System.Collections.Generic;

    public class ShapeHandManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private BoardController boardController;
        [SerializeField] private Camera          mainCamera;
        [SerializeField] private CellPool        cellPool;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform[]    spawnPoints; 
        [SerializeField] private DraggableShape draggableShapePrefab; 
        
        private IInput     input;
        private LevelData  currentLevelData;
        private ThemeColor currentTheme;
        private int        currentWaveIndex = 0;

        private List<DraggableShape> currentShapes = new();
        private DraggableShape       draggingShape = null;
        private Vector3              dragOffset;

        private Queue<DraggableShape> shapePool = new();

        private void Awake()
        {
            this.input = this.GetComponent<IInput>();
            if (this.input == null)
            {
                Debug.LogWarning("[ShapeHandManager] No IInput found. Adding default MouseInput.");
                this.input = this.gameObject.AddComponent<MouseInput>();
            }
            
            if (this.mainCamera == null) this.mainCamera = Camera.main;
        }

        public void SetupHand(LevelData levelData, ThemeColor theme)
        {
            this.currentLevelData = levelData;
            this.currentTheme     = theme;
            this.currentWaveIndex = 0;

            this.ClearAllSpawns();
            this.SpawnNextWave();
        }

        public void SpawnNextWave()
        {
            if (this.currentLevelData == null || this.cellPool == null) return;

            this.ClearAllSpawns();

            List<Shape> shapesToSpawn = new();

            if (this.currentWaveIndex < this.currentLevelData.PredefinedWaves.Count)
            {
                shapesToSpawn.AddRange(this.currentLevelData.PredefinedWaves[this.currentWaveIndex].shapes);
                this.currentWaveIndex++;
                Debug.Log($"[ShapeHandManager] Spawning predefined wave {this.currentWaveIndex}.");
            }
            else if (this.currentLevelData.HasGeneratorModule)
            {
                bool[,] boardState = this.boardController != null ? this.boardController.GetBoardState() : null;
                var availablePool  = new List<Shape>(this.currentLevelData.AvailableShapes);
                
                shapesToSpawn = this.currentLevelData.GeneratorModule.GenerateShapes(3, availablePool, boardState);
                Debug.Log("[ShapeHandManager] Spawning randomly generated wave.");
            }
            else 
            {
                Debug.LogWarning("[ShapeHandManager] No shapes generated. Out of predefined waves and no random generator configured.");
                return;
            }

            for (int i = 0; i < shapesToSpawn.Count && i < this.spawnPoints.Length; i++)
            {
                if (this.spawnPoints[i] == null || shapesToSpawn[i] == null) continue;

                Shape shapeToSpawn = shapesToSpawn[i];
                DraggableShape shapeInstance = this.GetShapeFromPool();

                shapeInstance.Initialize(
                    shapeToSpawn, 
                    this.currentTheme, 
                    this.spawnPoints[i].position, 
                    this.boardController != null ? this.boardController.CellSize : 1f, 
                    this.boardController != null ? this.boardController.Spacing : 0.1f, 
                    this.cellPool.Get 
                );

                this.currentShapes.Add(shapeInstance);
            }
        }

        public void ClearAllSpawns()
        {
            if (this.currentShapes.Count == 0) return;

            foreach (var shape in this.currentShapes)
            {
                if (this.cellPool != null) shape.ClearCells(this.cellPool.Return); 
                shape.gameObject.SetActive(false);
                this.shapePool.Enqueue(shape);
            }
            
            this.currentShapes.Clear();
        }


        private void Update()
        {
            if (this.input == null || this.boardController == null) return;

            Vector3 mouseWorldPos = this.mainCamera.ScreenToWorldPoint(this.input.PointerPosition);
            mouseWorldPos.z = 0f;

            if (this.input.PointerDown)
            {
                this.HandlePointerDown(mouseWorldPos);
            }
            else if (this.input.PointerHold && this.draggingShape != null)
            {
                this.HandlePointerHold(mouseWorldPos);
            }
            else if (this.input.PointerUp && this.draggingShape != null)
            {
                this.HandlePointerUp();
            }
        }

        private void HandlePointerDown(Vector3 mouseWorldPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
            if (hit.collider != null)
            {
                var shape = hit.collider.GetComponent<DraggableShape>();
                if (shape != null)
                {
                    this.draggingShape = shape;
                    this.dragOffset    = shape.transform.position - mouseWorldPos;
                    this.dragOffset   += new Vector3(0, 1.5f, 0); 
                    this.draggingShape.Pickup();
                }
            }
        }

        private void HandlePointerHold(Vector3 mouseWorldPos)
        {
            this.draggingShape.transform.position = mouseWorldPos + this.dragOffset;

            Vector3 topLeftPos = this.draggingShape.GetTopLeftCellWorldPosition();
            
            if (this.boardController.TryGetGridCoordinate(topLeftPos, out int gridX, out int gridY))
            {
                this.boardController.ShowPreview(this.draggingShape.ShapeData, gridX, gridY, this.draggingShape.CurrentSprite);
            }
            else
            {
                this.boardController.ClearPreview();
            }
        }

        private void HandlePointerUp()
        {
            Vector3 topLeftPos = this.draggingShape.GetTopLeftCellWorldPosition();

            this.boardController.ClearPreview(); 

            if (this.boardController.TryGetGridCoordinate(topLeftPos, out int gridX, out int gridY))
            {
                if (this.boardController.TryPlaceShape(this.draggingShape.ShapeData, gridX, gridY, this.draggingShape.CurrentSprite))
                {
                    this.ProcessSuccessfulDrop();
                    return;
                }
            }

            this.draggingShape.ReturnToStart();
            this.draggingShape = null;
        }

        private void ProcessSuccessfulDrop()
        {
            if (this.cellPool != null) this.draggingShape.ClearCells(this.cellPool.Return);
            this.currentShapes.Remove(this.draggingShape);
            
            this.draggingShape.gameObject.SetActive(false);
            this.shapePool.Enqueue(this.draggingShape);
            this.draggingShape = null;

            if (this.currentShapes.Count == 0)
            {
                this.SpawnNextWave();
            }
        }


        private DraggableShape GetShapeFromPool()
        {
            if (this.shapePool.Count > 0)
            {
                var shape = this.shapePool.Dequeue();
                shape.gameObject.SetActive(true);
                return shape;
            }

            return Instantiate(this.draggableShapePrefab, this.transform);
        }
    }
}