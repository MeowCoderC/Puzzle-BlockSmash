namespace BlockSmash
{
    using UnityEngine;

    public enum CellType
    {
        InBlock,
        InBoard,  
        Selected  
    }

    public class Cell : MonoBehaviour
    {
        [SerializeField] 
        private SpriteRenderer sr;

        private int  baseOrderInLayer;
        private bool isOrderInitialized = false;
        
        public Sprite CurrentSprite => this.sr != null ? this.sr.sprite : null;

        private void Reset()
        {
            this.sr = this.GetComponentInChildren<SpriteRenderer>();
        }

        private void Awake()
        {
            if (this.sr != null && !this.isOrderInitialized)
            {
                this.baseOrderInLayer   = this.sr.sortingOrder;
                this.isOrderInitialized = true;
            }
        }

        public void Init(Sprite sprite)
        {
            if (this.sr != null && sprite != null)
            {
                this.sr.sprite = sprite;
            }

            if (this.sr != null && !this.isOrderInitialized)
            {
                this.baseOrderInLayer   = this.sr.sortingOrder;
                this.isOrderInitialized = true;
            }
        }

        public void SetType(CellType type)
        {
            if (this.sr == null) return;

            switch (type)
            {
                case CellType.InBlock:
                    this.sr.sortingOrder = this.baseOrderInLayer;       
                    break;
                case CellType.InBoard:
                    this.sr.sortingOrder = this.baseOrderInLayer + 10; 
                    break;
                case CellType.Selected:
                    this.sr.sortingOrder = this.baseOrderInLayer + 100; 
                    break;
            }
        }
        
        public void SetAlpha(float alpha)
        {
            if (this.sr != null)
            {
                Color c = this.sr.color;
                c.a           = alpha;
                this.sr.color = c;
            }
        }
    }
}