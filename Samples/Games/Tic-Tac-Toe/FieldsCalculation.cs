using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Tic_Tac_Toe.Models;
using Tic_Tac_Toe.Screens;

namespace Tic_Tac_Toe
{
    public static class FieldsCalculation
    {
        private static List<List<(int x, int y)>> fieldsToWin;
        public static List<List<(int x, int y)>> FieldsToWin
        {
            get
            {
                if (fieldsToWin == null)
                    fieldsToWin = GenerateFieldsToWin();

                return fieldsToWin;
            }
        }

        public static FieldsCalculationResult CalculateFieldsValuesPosibilities(FieldValue[,] fieldValues)
        {
            var result = new FieldsCalculationResult();

            ForEachAllFields().ToList().ForEach(item =>
            {
                var x = item.x;
                var y = item.y;

                var fieldValue = fieldValues[x, y];

                result.Horizontal[y][fieldValue]++;
                result.Vertical[x][fieldValue]++;

                // check diagonal 1
                if (x == y)
                    result.Diagonal[0][fieldValue]++;

                // check diagonal 2
                if (x + y == 2)
                    result.Diagonal[1][fieldValue]++;
            });

            return result;
        }

        public static Maybe<(int x, int y)> CheckSpaceToWinByFieldValue(FieldValue[,] fieldValues, FieldValue fieldValueCheck)
        {
            // check all possibilities to win if there are 2 fieldValueCheck and 1 none
            var itemsToWin = FieldsToWin.FirstOrDefault(items =>
            {
                var totalFieldValueCheck = 0;
                var totalNone = 0;

                items.ForEach(item =>
                {
                    if (fieldValues[item.x, item.y] == fieldValueCheck)
                        totalFieldValueCheck++;
                    else if (fieldValues[item.x, item.y] == FieldValue.None)
                        totalNone++;
                });

                return totalFieldValueCheck == 2 && totalNone == 1;
            });

            if (itemsToWin == null)
                return Maybe.None;

            // return the none field
            return itemsToWin.First(item => fieldValues[item.x, item.y] == FieldValue.None);
        }

        public static IEnumerable<(int x, int y)> ForEachAllFields()
        {
            for (var x = 0; x < TicTacToeScreen.TotalGameSquares; x++)
            {
                for (var y = 0; y < TicTacToeScreen.TotalGameSquares; y++)
                {
                    yield return (x, y);
                }
            }
        }

        private static List<List<(int x, int y)>> GenerateFieldsToWin()
        {
            var horizontal = new List<(int x, int y)>[3];
            var vertical = new List<(int x, int y)>[3];
            var diagonal = new List<(int x, int y)>[2];

            ForEachAllFields().ToList().ForEach(item =>
            {
                var x = item.x;
                var y = item.y;

                horizontal[y] = horizontal[y] ?? new List<(int x, int y)>();
                horizontal[y].Add((x, y));

                vertical[x] = vertical[x] ?? new List<(int x, int y)>();
                vertical[x].Add((x, y));

                // check diagonal 1
                if (x == y)
                {
                    diagonal[0] = diagonal[0] ?? new List<(int x, int y)>();
                    diagonal[0].Add((x, y));
                }

                // check diagonal 2
                if (x + y == 2)
                {
                    diagonal[1] = diagonal[1] ?? new List<(int x, int y)>();
                    diagonal[1].Add((x, y));
                }
            });

            return horizontal.ToList()
                .Concat(vertical.ToList())
                .Concat(diagonal.ToList())
                .ToList();
        }
    }
}
