using System.Collections.Generic;

namespace Tic_Tac_Toe.Models
{
    public class FieldsCalculationResult
    {
        private const int MaxHorizontal = 3;
        private const int MaxVertical = 3;
        private const int MaxDiagonal = 2;
        public readonly Dictionary<FieldValue, int>[] Horizontal = new Dictionary<FieldValue, int>[MaxHorizontal];
        public readonly Dictionary<FieldValue, int>[] Vertical = new Dictionary<FieldValue, int>[MaxVertical];
        public readonly Dictionary<FieldValue, int>[] Diagonal = new Dictionary<FieldValue, int>[MaxDiagonal];

        public FieldsCalculationResult()
        {
            SetDefaultValues(Horizontal);
            SetDefaultValues(Vertical);
            SetDefaultValues(Diagonal);
        }

        private void SetDefaultValues(Dictionary<FieldValue, int>[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = new Dictionary<FieldValue, int>
                {
                    { FieldValue.X, 0 },
                    { FieldValue.O, 0 },
                    { FieldValue.None, 0 }
                };
            }
        }
    }
}
