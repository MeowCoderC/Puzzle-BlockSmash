namespace BlockSmash
{
    using System.Collections.Generic;
    using UnityEngine;

    public class CellPool : MonoBehaviour
    {
        [SerializeField] private Cell      cellPrefab;
        [SerializeField] private Transform poolParent; 

        private Queue<Cell> pool = new();

        private void Awake()
        {
            if (this.poolParent == null) this.poolParent = this.transform;
        }

        public Cell Get()
        {
            if (this.pool.Count > 0)
            {
                var cell = this.pool.Dequeue();
                cell.gameObject.SetActive(true);
                return cell;
            }

            return Instantiate(this.cellPrefab, this.poolParent);
        }

        public void Return(Cell cell)
        {
            if (cell == null) return;
            
            cell.gameObject.SetActive(false);
            cell.transform.SetParent(this.poolParent);
            this.pool.Enqueue(cell);
        }
    }
}