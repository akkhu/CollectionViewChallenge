using System;
namespace CollectionViewChallenge.Models
{
    public class CollectionViewIndex
    {
        public static CollectionViewIndex For(int itemIndex, int span)
            => new CollectionViewIndex
            {
                ItemIndex = itemIndex,
                RowIndex = itemIndex / span,
                ColIndex = (itemIndex) % span
            };

        public int ItemIndex { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
    }

}
