using MonoGame.GameManager.GameMath;
using System.Linq;

namespace Tic_Tac_Toe
{
    public class Bot
    {
        private readonly FieldValue botValue;
        private readonly FieldValue playerValue;

        public Bot(FieldValue botValue, FieldValue playerValue)
        {
            this.botValue = botValue;
            this.playerValue = playerValue;
        }

        public (int x, int y) ChooseField(FieldValue[,] fieldValues)
        {
            // Check win
            var maybeField = FieldsCalculation.CheckSpaceToWinByFieldValue(fieldValues, botValue); 
            if (maybeField.HasValue)
                return maybeField.Value;

            // Check don't lose
            maybeField = FieldsCalculation.CheckSpaceToWinByFieldValue(fieldValues, playerValue);
            if (maybeField.HasValue)
                return maybeField.Value;

            // Random position
            return GetRandomNoneField(fieldValues);
        }

        private (int x, int y) GetRandomNoneField(FieldValue[,] fieldValues)
        {
            var noneFields = FieldsCalculation.ForEachAllFields()
                .Where(item => fieldValues[item.x, item.y] == FieldValue.None)
                .ToList();

            return noneFields[RandomGenerator.Random(0, noneFields.Count - 1)];
        }
    }
}
