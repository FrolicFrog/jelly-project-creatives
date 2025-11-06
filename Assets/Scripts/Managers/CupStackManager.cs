using System.Collections.Generic;
using UnityEngine;
using BlockStackTypes;

namespace CupStackManagement
{
    public class CupStackManager
    {
        public static List<CupStacks> GetAdjacentCupStacks(string StackIdentifier, CupColors color, List<CupStacks> CupStacksList)
        {
            CupStacks Origin = null;
            string OriginIdentifer = "";

            foreach (var CupStack in CupStacksList)
            {
                if (CupStack.Cups.Count > 0 && CupStack.Cups.Peek().GetComponent<Cup>().StackIdentifier == StackIdentifier && CupStack.Colors.Count > 0 && CupStack.Colors.Peek() == color)
                {
                    OriginIdentifer = CupStack.Cups.Peek().GetComponent<Cup>().StackIdentifier;
                    Origin = CupStack;
                    break;
                }
            }

            if (Origin == null) return new List<CupStacks>() { };

            (int originX, int originY) = GetCoordinatesFromIdentifier(OriginIdentifer);
            if (originX == -1 || originY == -1) return new List<CupStacks>() { Origin };

            HashSet<string> visited = new();
            Queue<(int x, int y)> queue = new();
            List<CupStacks> result = new() { Origin };

            queue.Enqueue((originX, originY));
            visited.Add(OriginIdentifer);

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                foreach (var offset in GetNeighborOffsets(x))
                {
                    int nx = x + offset.x;
                    int ny = y + offset.y;
                    string identifier = nx + "_" + ny.ToString();
                    if (visited.Contains(identifier)) continue;

                    var stack = CupStacksList.Find(s =>
                        s.Cups != null && 
                        s.Cups.Count > 0 &&
                        s.Cups.Peek().GetComponent<Cup>().StackIdentifier == identifier &&
                        s.Colors.Count > 0 &&
                        s.Colors.Peek() == color
                    );

                    if (stack != null)
                    {
                        visited.Add(identifier);
                        queue.Enqueue((nx, ny));
                        result.Add(stack);
                    }
                }
            }

            return result;
        }

        private static List<(int x, int y)> GetNeighborOffsets(int x)
        {
            // even-q vertical layout
            if (x % 2 == 0)
            {
                return new List<(int x, int y)>
        {
            (1, 0), (-1, 0), (0, -1), (0, 1), (1, -1), (-1, -1)
        };
            }
            else
            {
                return new List<(int x, int y)>
        {
            (1, 0), (-1, 0), (0, -1), (0, 1), (1, 1), (-1, 1)
        };
            }
        }



        private static (int x, int y) GetCoordinatesFromIdentifier(string originIdentifer)
        {
            if (string.IsNullOrEmpty(originIdentifer) || originIdentifer.Length < 3)
            {
                Debug.LogError("Invalid identifier format: " + originIdentifer);
                return (-1, -1);
            }

            string[] coordinates = originIdentifer.Split("_");
            int x = int.Parse(coordinates[0]);
            int y = int.Parse(coordinates[1]);

            return (x, y);
        }

        public static void RevealMysteriousCupIfAny(List<CupStacks> adjacentCupStacksList)
        {
            foreach (CupStacks Stack in adjacentCupStacksList)
            {
                if (Stack.Cups.Count == 0) continue;

                Cup cup = Stack.Cups.Peek().GetComponentInChildren<Cup>();
                if (cup == null) continue;

                cup.RevealMysteriousCup();
            }
        }
        
    }
}
