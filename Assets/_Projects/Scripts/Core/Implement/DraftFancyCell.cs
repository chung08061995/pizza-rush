using FancyScrollView;

namespace DraftUtils
{
    public abstract class DraftFancyCell<T> : FancyScrollRectCell<T, FancyContext>
    {
        public override void UpdateContent(T itemData)
        {
            SetData(itemData);
        }

        protected abstract void SetData(T data);
        
        public override void UpdatePosition(float position)
        {
            base.UpdatePosition(position);
            // Có thể override để làm hiệu ứng animation dựa trên position nếu cần
        }
    }
}
