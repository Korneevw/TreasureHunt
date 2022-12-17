namespace TreasureHunt
{
    internal struct Vector
    {
        public int X { get; init; }
        public int Y { get; init; }
        public Vector(int x, int y) 
        { 
            X = x; 
            Y = y; 
        }
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
        public static Vector operator +(Vector v, Vector w)
        {
            return new Vector(v.X + w.X, v.Y + w.Y);
        }
        public static bool operator ==(Vector v, Vector w)
        {
            return (v.X == w.X) && (v.Y == w.Y);
        }
        public static bool operator !=(Vector v, Vector w)
        {
            return (v.X != w.X) || (v.Y != w.Y);
        }
    }
    internal static class Moves
    {
        public static Move North { get; } = new Move() { PositionChangeVector = new Vector(0, 1), FacingDirection = Move.FacingDirections.North };
        public static Move South { get; } = new Move() { PositionChangeVector = new Vector(0, -1), FacingDirection = Move.FacingDirections.South };
        public static Move East { get; } = new Move() { PositionChangeVector = new Vector(1, 0), FacingDirection = Move.FacingDirections.East };
        public static Move West { get; } = new Move() { PositionChangeVector = new Vector(-1, 0), FacingDirection = Move.FacingDirections.West };
        public static Dictionary<Move.FacingDirections, Move> MovesFromDirections { get; } = new Dictionary<Move.FacingDirections, Move>()
        {
            [Move.FacingDirections.North] = North,
            [Move.FacingDirections.South] = South,
            [Move.FacingDirections.East] = East,
            [Move.FacingDirections.West] = West,
        };
    }
    internal struct Move
    {
        public enum FacingDirections
        {
            North,
            East,
            South,
            West,
        }
        public FacingDirections FacingDirection { get; init; }
        public Vector PositionChangeVector { get; init; }
        public Move(FacingDirections facingDirection, Vector positionChangeVector)
        {
            FacingDirection = facingDirection;
            PositionChangeVector = positionChangeVector;
        }
        public override string ToString()
        {
            return $"{FacingDirection}";
        }
        public static Vector operator +(Vector v, Move m)
        {
            return new Vector(v.X + m.PositionChangeVector.X, v.Y + m.PositionChangeVector.Y);
        }
    }
    internal interface IFacingDirectionOperator
    {
        public Move.FacingDirections TurnRight(Move.FacingDirections initialDirection);
        public Move.FacingDirections TurnLeft(Move.FacingDirections initialDirection);
        public Move.FacingDirections TurnAround(Move.FacingDirections initialDirection);
        public string ToString(Move.FacingDirections direction);
    }
    internal interface IInputReader
    {
        public IFacingDirectionOperator FacingDirectionOperator { get; set; }
        public Vector ReadDestinationPoint();
        public List<Move> ReadMoves();
    }
    internal class InputReader : IInputReader
    {
        public IFacingDirectionOperator FacingDirectionOperator { get; set; } = new FacingDirectionOperator();
        public Vector ReadDestinationPoint()
        {
            while (true)
            {
                Console.WriteLine("Введите координаты клада через пробел.");
                string? pointString = Console.ReadLine();
                if (pointString is null) continue;
                int[] coordinates = pointString.Split().Select(int.Parse).ToArray();
                return new Vector(coordinates[0], coordinates[1]);
            }
        }
        public List<Move> ReadMoves()
        {
            List<Move> readMoves = new List<Move>();
            Move.FacingDirections facingDirection = Move.FacingDirections.North;
            Console.WriteLine("Начинайте вводить указания карты. \"стоп\" для прекращения.");
            while (true)
            {
                string? actionString = Console.ReadLine();
                if (actionString is null) continue;
                string[] actionStringSplit = actionString.Split();
                int repeatTimes = 1;
                if (actionStringSplit.Length > 1)
                {
                    repeatTimes = int.Parse(actionStringSplit[1]);
                }
                string action = actionStringSplit[0];
                if (action == "стоп") break;
                for (int i = 0; i < repeatTimes; i++)
                {
                    switch (action)
                    {
                        case "разворот":
                            facingDirection = FacingDirectionOperator.TurnAround(facingDirection);
                            break;
                        case "направо":
                            facingDirection = FacingDirectionOperator.TurnRight(facingDirection);
                            break;
                        case "налево":
                            facingDirection = FacingDirectionOperator.TurnLeft(facingDirection);
                            break;
                        case "вперёд":
                            readMoves.Add(Moves.MovesFromDirections[facingDirection]);
                            break;
                        default:
                            continue;
                    }
                }
            }
            return readMoves;
        }
    }
    internal class FacingDirectionOperator : IFacingDirectionOperator
    {
        public Move.FacingDirections TurnRight(Move.FacingDirections initialDirection)
        {
            return (Move.FacingDirections)((int)initialDirection + 1 > 3 ? 0 : (int)initialDirection + 1);
        }
        public Move.FacingDirections TurnLeft(Move.FacingDirections initialDirection)
        {
            return (Move.FacingDirections)((int)initialDirection - 1 < 0 ? 3 : (int)initialDirection - 1);
        }
        public Move.FacingDirections TurnAround(Move.FacingDirections initialDirection)
        {
            return initialDirection switch
            {
                Move.FacingDirections.North => Move.FacingDirections.South,
                Move.FacingDirections.South => Move.FacingDirections.North,
                Move.FacingDirections.East => Move.FacingDirections.West,
                Move.FacingDirections.West => Move.FacingDirections.East,
                _ => throw new NotImplementedException($"Unable to turn around from {initialDirection}")
            };
        }
        public string ToString(Move.FacingDirections direction)
        {
            return direction switch
            {
                Move.FacingDirections.North => "Север",
                Move.FacingDirections.East => "Восток",
                Move.FacingDirections.South => "Юг",
                Move.FacingDirections.West => "Запад",
                _ => throw new NotImplementedException($"Unable to get string representation of {direction}"),
            };
        }
    }
    internal class TreasureHunt
    {
        public IFacingDirectionOperator FacingDirectionOperator { get; set; } = new FacingDirectionOperator();
        public IInputReader InputReader { get; set; } = new InputReader();
        private int FindDistanceBetweenTwoPoints(Vector from, Vector to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
        private List<Move>? FindShortestPossiblePathBetweenTwoPoints(Vector startingPoint, Vector destinationPoint, List<Move> availableMoves)
        {
            List<Move> bestPathMoves = new List<Move>();
            Move? nextBestMove = null;
            int? nextShortestDistance = null;
            Vector currentPosition = startingPoint;
            int currentDistance = FindDistanceBetweenTwoPoints(currentPosition, destinationPoint);
            while (true)
            {
                for (int i = 0; i < availableMoves.Count; i++)
                {
                    Move currentMove = availableMoves[i];
                    Vector positionAfterMovement = currentPosition + currentMove;
                    int distanceAfterMovement = FindDistanceBetweenTwoPoints(positionAfterMovement, destinationPoint);
                    if ((nextShortestDistance is null || distanceAfterMovement < nextShortestDistance) && distanceAfterMovement < currentDistance)
                    {
                        nextBestMove = currentMove;
                        nextShortestDistance = distanceAfterMovement;
                    }
                }
                if (nextBestMove is not null)
                {
                    availableMoves.Remove((Move)nextBestMove);
                    bestPathMoves.Add((Move)nextBestMove);
                    currentPosition = currentPosition + (Move)nextBestMove;
                    currentDistance = FindDistanceBetweenTwoPoints(currentPosition, destinationPoint);
                    nextBestMove = null;
                }
                else
                {
                    return null;
                }

                if (currentPosition == destinationPoint)
                {
                    break;
                }
            }
            return bestPathMoves;
        }
        private void OutputShortestPath(List<Move> shortestPath)
        {
            Console.WriteLine($"Минимальное количество указаний карты: {shortestPath.Count}");
            Console.WriteLine($"Направление взгляда: {FacingDirectionOperator.ToString(shortestPath.Last().FacingDirection)}");
        }
        public void Solve()
        {
            Vector destinationPath = InputReader.ReadDestinationPoint();
            List<Move> availableMoves = InputReader.ReadMoves();
            List<Move>? result = FindShortestPossiblePathBetweenTwoPoints(new Vector(0, 0), destinationPath, availableMoves);
            if (result is null)
            {
                Console.WriteLine("Найти кратчайший путь до клада не получилось :(");
                return;
            }
            OutputShortestPath(result);
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            TreasureHunt hunt = new TreasureHunt();
            hunt.Solve();
        }
    }
}