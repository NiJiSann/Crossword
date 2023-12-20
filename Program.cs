using System;
using System.Collections.Generic;
using System.Linq;

namespace SabGames.Words
{
    public enum Direction
    {
        Right,
        Down
    }

    public struct Letter
    {
        public int x;
        public int y;
        public char character;
    }

    public struct WordC
    {
        public int startX;
        public int startY;
        public List<Letter> letters;
        public Direction direction;

        public void Print()
        {
            foreach (var letter in letters)
            {
                Console.Write(letter.character);
            }
        }
    }

    public class CrossWordGenerator
    {
        private static int _width = 20;
        private static int _height = 20;
        private static int _currentAttempt = 0;
        public static List<WordC> _words = new List<WordC>();
        private static char[,] Matrix => ListToMatrix(_words, '*');
        public static List<string> wordList = new List<string>();
        static Random rand = new Random();
        private static int _callClear;

        static void Main(string[] args)
        {
            var words = new List<string>
            {
                //"TTTTTTU",
                //"TAS",
                //"SSSQSQ",
                //"PPQPP",
                //"PPQPP"
            };
            var matrix = GenerateCrossword(words);
            PrintMatrix(matrix);
            foreach (var word in _words)
            {
                Console.WriteLine("");
                word.Print();
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"Attempts: {_currentAttempt}");
            Console.WriteLine(_callClear);
            Console.WriteLine("");

        }

        static void PrintMatrix(char[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j]} ");
                }

                Console.WriteLine();
            }
        }

        public static char[,] GenerateCrossword(List<string> originalWords)
        {
            wordList.Clear();
            _words.Clear();
            int maxAttempts = 100;
            _currentAttempt = 0;
            do
            {
                if (_currentAttempt>1)
                {
                    wordList.Clear();
                    _words.Clear();
                    _currentAttempt = 0;
                }
                var shuffledWords = originalWords.OrderBy(x => Guid.NewGuid()).ToList();
                int startX = _width / 4;
                int startY = _height / 4;

                var startDirection = (Direction)rand.Next(0, 2);
                var startWord = CreateWord(shuffledWords[0], startDirection, startX, startY);
                _words.Add(startWord);


                for (int i = 1; i < shuffledWords.Count; i++)
                {
                    TryCreateWord(shuffledWords[i]);
                }

                _currentAttempt++;
                if (_currentAttempt > maxAttempts)
                {
                    //Debug.Log("CANT CREATE!");
                    break;
                }
            } while (_words.Count < originalWords.Count);

            return ListToMatrix(_words, '*');
        }

        static void TryCreateWord(string currentWord)
        {
            bool canPlace = false;
            foreach (var placedWord in _words)
            {
                var letters = placedWord.letters;
                for (int i = 0; i < letters.Count; i++)
                {
                    for (int j = 0; j < currentWord.Length; j++)
                    {
                        if (letters[i].character == currentWord[j])
                        {
                            if (CanPlaceWord(currentWord, j, letters[i], placedWord))
                            {
                                var direction = placedWord.direction == Direction.Down ? Direction.Right : Direction.Down;
                                var startX = 0;
                                var startY = 0;
                                if (direction == Direction.Down)
                                {
                                    startX = letters[i].x;
                                    startY = letters[i].y - j;
                                }

                                else
                                {
                                    startX = letters[i].x - j;
                                    startY = letters[i].y;
                                }
                                _words.Add(CreateWord(currentWord, direction, startX, startY));

                                canPlace = true;
                                break;
                            }
                        }
                    }

                    if (canPlace)
                        break;
                }

                if (canPlace)
                    break;
            }
        }

        static bool CanPlaceWord(string word, int currentConnectedIndex, Letter connectedCharacter, WordC connectedWord)
        {
            var direction = connectedWord.direction == Direction.Down ? Direction.Right : Direction.Down;

            for (int i = 0; i < word.Length; i++)
            {
                var startX = 0;
                var startY = 0;

                if (direction == Direction.Down)
                {
                    startX = connectedCharacter.x;
                    startY = connectedCharacter.y - currentConnectedIndex;

                    if (Matrix[startX, startY - 1 + i] != '*' && connectedWord.letters.Count(letter => letter.x == startX && letter.y == startY - 1 + i) == 0
                        || Matrix[startX, startY + 1 + i] != '*' && connectedWord.letters.Count(letter => letter.x == startX && letter.y == startY + 1 + i) == 0
                        || Matrix[startX - 1, startY + i] != '*' && connectedWord.letters.Count(letter => letter.x == startX - 1 && letter.y == startY + i) == 0
                        || Matrix[startX + 1, startY + i] != '*' && connectedWord.letters.Count(letter => letter.x == startX + 1 && letter.y == startY + i) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    startX = connectedCharacter.x - currentConnectedIndex;
                    startY = connectedCharacter.y;

                    if (Matrix[startX + i, startY - 1] != '*' && connectedWord.letters.Count(letter => letter.x == startX + i && letter.y == startY - 1) == 0
                        || Matrix[startX + i, startY + 1] != '*' && connectedWord.letters.Count(letter => letter.x == startX + i && letter.y == startY + 1) == 0
                        || Matrix[startX + i - 1, startY] != '*' && connectedWord.letters.Count(letter => letter.x == startX + i - 1 && letter.y == startY) == 0
                        || Matrix[startX + i + 1, startY] != '*' && connectedWord.letters.Count(letter => letter.x == startX + i + 1 && letter.y == startY) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        static WordC CreateWord(string word, Direction direction, int startX, int startY)
        {
            if (_currentAttempt > 1)
            {
                _callClear++;
                wordList.Clear();
                _words.Clear();
            }
            
            WordC newWord;
            newWord.direction = direction;
            newWord.startX = startX;
            newWord.startY = startY;
            newWord.letters = new List<Letter>();
            for (int i = 0; i < word.Length; i++)
            {
                Letter letter;
                letter.character = word[i];
                if (direction == Direction.Down)
                {
                    letter.x = startX;
                    letter.y = startY + i;
                }
                else
                {
                    letter.x = startX + i;
                    letter.y = startY;
                }
                newWord.letters.Add(letter);
            }
            wordList.Add(word);
            return newWord;
        }

        static char[,] ListToMatrix(List<WordC> words, char placeholder)
        {
            var matrix = new char[_width, _height];
            for (var index0 = 0; index0 < matrix.GetLength(0); index0++)
                for (var index1 = 0; index1 < matrix.GetLength(1); index1++)
                {
                    matrix[index0, index1] = placeholder;
                }

            for (int i = 0; i < words.Count; i++)
            {
                var letters = words[i].letters;
                foreach (var letter in letters)
                {
                    matrix[letter.x, letter.y] = letter.character;
                }
            }

            return matrix;
        }
    }
}
