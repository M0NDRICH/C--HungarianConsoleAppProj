using System;
using System.Collections.Generic;
using System.Text;

namespace FinalProject_QuantitativeMethods
{
    internal static class Hungarian
    {
        static int _row = 0;
        static int _column = 0;
        static int[][]? array2D;
        static string[][]? tempArr;
        static int numberOfLinesToCoverZero;

        public static async Task  MethodAsync<T>(List<T> rowNames, List<T> columnNames, int[][] arr)
        {
            _row = rowNames.Count;
            _column = columnNames.Count;

            array2D = new int[_row][];
            tempArr = new string[_row][];

            for (int y = 0 ; y < _row; y++)
            {
                array2D[y] = new int[_column];
                for (int x = 0 ; x < _column; x++)
                {
                    array2D[y][x] = arr[y][x];
                    Console.Write(arr[y][x]);
                }
                Console.WriteLine(" ");
            }

            // Printing the current matrix before the first step
            PrintMatrix(rowNames, columnNames, arr);

            array2D = Step1(arr);

            PrintMatrix(rowNames, columnNames, array2D);

            array2D = Step2(array2D);

            PrintMatrix(rowNames, columnNames, array2D);

            //if (Step3(array2D, rowNames) == true)
            //{
            //    PrintMatrix(rowNames, columnNames, array2D);
            //}
            tempArr = await Step3Async(array2D, rowNames);

            //if (tempArr == null) { break; }

            PrintTempMatrix(rowNames, columnNames, tempArr);
            //for (int i = 0; i < tempArr.Length; i++)
            //{
            //    for (int j = 0; j < tempArr[i].Length; j++)
            //    {
            //        Console.Write(tempArr[i][j]);
            //    }
            //    Console.WriteLine(" ");
            //}
            for (int i = 0; i < 3; i++)
            {
                Console.Write("hello world");
            }

            //resultArr = await Step4Async(array2D, rownames);

           // PrintTempMatrix(rowNames, columnNames, tempArr);
        }

        #region -- The Steps --
        public static int[][] Step1(int[][] arr)
        {
            int[][] new2dArrayValues = new int[arr.Length][];
            int rowMin;
            int[] currentArrRow;

            for (int i = 0; i < arr.Length; i++)
            {
                currentArrRow = arr[i];
                rowMin = GetMin(currentArrRow);
                List<int> result = SubtractionWithMinValue(currentArrRow, n => n - rowMin).ToList();

                //assigning new values based on the result after subtracting the min value in each row
                for (int j = 0; j < currentArrRow.Length; j++)
                {
                    new2dArrayValues[i] = result.ToArray();
                }
            }
            return new2dArrayValues;
        }

        public static int[][] Step2(int[][] arr)
        {
            int Dimension = arr.Length;
            int[][] new2DArrayValues = new int[Dimension][];
            int[][] temp2DArr = new int[Dimension][];
            int[] tempArr = new int[Dimension];
            List<int> result = new List<int>();
            int columnMin;

            for(int i = 0; i < Dimension; i++)
            {
                new2DArrayValues[i] = new int[Dimension];
            }

            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    tempArr[x] = arr[x][y];
                }

                columnMin = GetMin(tempArr);
                result = [.. SubtractionWithMinValue(tempArr, n => n - columnMin)];

                for (int x = 0; x < Dimension; x++)
                {
                    new2DArrayValues[x][y] = result[x];
                }
            }

            return new2DArrayValues;
        }

        public static async Task<string[][]> Step3Async<T>(int[][] arr, List<T> rowNames)
        {
            // for zero locations row or column
            Dictionary<string, int> values = new Dictionary<string, int>();

            // for storing bool if the zero is covered or not by the vertical or horizontal line
            Dictionary<string, bool> isValueCover = new Dictionary<string, bool>();

            string[][] tempMatrix = new string[arr.Length][]; // this will hold the copies of the values of matrix

            int numberOfLines = rowNames.Count;
            int currentNumberOfLines = 0;
            int currentValue;
            int numOfValues;
            int zeroCounter = 0;

            // for initialization
            for (int i = 0; i < arr.Length; i++)
            {
                tempMatrix[i] = new string[arr.Length];
            }

            //for counting zero in rows
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < numberOfLines; j++)
                {
                    currentValue = arr[i][j];
                    if (currentValue == 0) zeroCounter++;
                }
                string key = string.Format("Row {0}", i);
                values[key] = zeroCounter;
                zeroCounter = 0; //Reset the counter for each row
            }

            //for counting zero in columns
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < numberOfLines; j++)
                {
                    currentValue = arr[j][i];
                    if (currentValue == 0) zeroCounter++;
                }
                string key = string.Format("Column {0}", i);
                values[key] = zeroCounter;
                zeroCounter = 0; //Reset the counter for each column
            }

            numOfValues = values.Count();
            //numOfValues > 0
            int loopIterator = 0;
            while (loopIterator < arr[0].Length)
            {
                if (loopIterator == 0)
                { 
                    //For row
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string targetRow = string.Format("Row {0}", i);
                        if (values[targetRow] >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            isValueCover[targetRow] = true;
                            currentNumberOfLines++;
                            numOfValues--;

                            for(int j = 0; j < arr.Length; j++)
                            {
                                tempMatrix[i][j] = "X";
                            }
                        }
                        else
                        {
                            for (int j = 0; j < arr.Length; j++)
                            {
                                tempMatrix[i][j] = (arr[i][j]).ToString();
                            }
                        }
                    }

                    for (int i = 0; i < arr.Length; i++)
                    {
                        string targetColumn = string.Format("Column {0}", i);
                        if (values[targetColumn] >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            isValueCover[targetColumn] = true;
                            currentNumberOfLines++;
                            numOfValues--;

                            for (int j = 0; j < arr.Length; j++)
                            {
                                if (tempMatrix[j][i] != "X")
                                {
                                    tempMatrix[j][i] = "X";
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < arr.Length; j++)
                            {
                                if (tempMatrix[j][i] != "X")
                                {
                                    tempMatrix[j][i] = (arr[j][i]).ToString();
                                }
                            }
                        }
                    }
                }
                //PrintMatrixString(rowNames, rowNames, tempMatrix);
                //PrintLoop(tempMatrix.Length, tempMatrix);
                //if (numOfValues  >= 1)
                //{
                //    for (int i = 0; i < tempMatrix.Length; i++)
                //    {
                //        for (int j = 0; j < tempMatrix[i].Length; j++)
                //        {
                //            Console.Write(tempMatrix[i][j]);
                //        }
                //        Console.WriteLine(" ");
                //    }
                //}
                numberOfLinesToCoverZero = currentNumberOfLines;
                Console.WriteLine("Number of lines " + numberOfLinesToCoverZero);
                if (currentNumberOfLines == rowNames.Count)
                    return await Task.FromResult(tempMatrix);

                int nullCounter = 0;

                for(int i = 0; i < tempMatrix.Length; i++)
                {
                    if (tempMatrix[i].Contains(null))
                    {
                        nullCounter++;
                    }
                }

                if (nullCounter == 0)
                {
                    bool isAllZeroNotCovered = FindVal(tempMatrix, 0);
                    if(!isAllZeroNotCovered)
                    {
                        for (int i = 0; i < tempMatrix.Length; i++)
                        {
                            for (int j = 0; j < tempMatrix[i].Length; j++)
                            {
                                Console.Write(tempMatrix[i][j]);
                            }
                            Console.WriteLine(" ");
                        }
                        numberOfLinesToCoverZero = currentNumberOfLines;
                        Console.WriteLine("Number of lines " + numberOfLinesToCoverZero);

                        return await Task.FromResult(tempMatrix);
                    }
                    else
                    {
                        for (int i = 0; i < tempMatrix.Length; i++)
                        {
                           for (int j = 0;j < tempMatrix[i].Length;j++)
                           {
                                double random = Random.Shared.NextDouble();

                                if (tempMatrix[i][j] == "0")
                                {
                                    bool coverRow = Random.Shared.Next(2) == 0; // 50/50 chance

                                    if (coverRow)
                                    {
                                        for (int k = 0; k < arr.Length; k++) tempMatrix[i][k] = "X";
                                    }
                                    else
                                    {
                                        for (int k = 0; k < arr.Length; k++) tempMatrix[k][j] = "X";
                                    }
                                    currentNumberOfLines++;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < tempMatrix.Length; i++)
                    {
                        for (int j = 0; j < tempMatrix[i].Length; j++)
                        {
                            Console.Write(tempMatrix[i][j]);
                        }
                        Console.WriteLine(" ");
                    }
                }

                loopIterator++;
            }


            return await Task.FromResult(tempMatrix);
        }

        //public static async Task<string[][]> Step4<T>(int[][] arr, T[] rowNames)
        //{


        //    return arr;
        //}

        #endregion

        #region -- Print Matrix --
        public static void PrintMatrix<T>(List<T> rowNames, List<T> columnNames, int[][] values)
        {
            //for calculating the space for column names
            int space = 0;
            for(int i = 0; i < rowNames.Count; i++)
            {
                if (i == 0)
                {
                    space = (rowNames[i]!).GetLength();
                    continue;
                }
                if (space < rowNames[i]!.GetLength()) space = rowNames[i]!.GetLength();
            }

            PrintLoop(space, " ");

            PrintLoop(columnNames.Count, columnNames.ToList(), "| {0} ");

            Console.WriteLine("| ");

            
            // For each value in Matrix
            for (int y = 0; y < columnNames.Count; y++)
            {
                //for printing the names of each row
                Console.Write((rowNames[y])?.ToString());
                //for row names distance
                PrintLoop(Convert.ToInt16(space - rowNames[y]?.GetLength()), " ");

                //for printing the actual number/values of the matrix
                for(int x = 0 ; x < rowNames.Count + 1; x++)
                {
                    if (x >= rowNames.Count) break;
                    //int valueLength = (values[y][x]).ToString().Length;
                    int valueLength = (values[y][x]).GetLength();
                    int distance = columnNames[x]!.GetLength();
                    int spacing = distance % 2 == 0 ? (distance / 2) : ((distance / 2)+1);

                    Console.Write("|");
                    PrintLoop(spacing - valueLength, " ");
                    Console.Write(" {0} ", values[y][x]);
                    PrintLoop((distance % 2 == 0 ? spacing : (--spacing)), " ");

                }
                Console.WriteLine("| ");
            }
        }

        public static void PrintTempMatrix<T>(List<T> rowNames, List<T> columnNames, string[][] values)
        {
            //for calculating the space for column names
            int space = 0;
            for (int i = 0; i < rowNames.Count; i++)
            {
                if (i == 0)
                {
                    space = (rowNames[i]!).GetLength();
                    continue;
                }
                if (space < rowNames[i]!.GetLength()) space = rowNames[i]!.GetLength();
            }

            PrintLoop(space, " ");

            PrintLoop(columnNames.Count, columnNames.ToList(), "| {0} ");

            Console.WriteLine("| ");


            // For each value in Matrix
            for (int y = 0; y < columnNames.Count; y++)
            {
                //for printing the names of each row
                Console.Write((rowNames[y])?.ToString());
                //for row names distance
                PrintLoop(Convert.ToInt16(space - rowNames[y]?.GetLength()), " ");

                //for printing the actual number/values of the matrix
                for (int x = 0; x < rowNames.Count + 1; x++)
                {
                    if (x >= rowNames.Count) break;
                    //int valueLength = (values[y][x]).ToString().Length;
                    int valueLength = (values[y][x]).GetLength();
                    int distance = columnNames[x]!.GetLength();
                    int spacing = distance % 2 == 0 ? (distance / 2) : ((distance / 2) + 1);

                    Console.Write("|");
                    PrintLoop(spacing - valueLength, " ");
                    Console.Write(" {0} ", values[y][x]);
                    PrintLoop((distance % 2 == 0 ? spacing : (--spacing)), " ");

                }
                Console.WriteLine("| ");
            }
        }
        #endregion

        #region -- Calculation Methods --

        public static IEnumerable<T> SubtractionWithMinValue<T>(T[] List, Func<T?, T?>? func)
        {
            for (int x = 0; x < List.Length; x++)
            {
                yield return func!(List[x])!;
            }
        }

        #endregion

        #region -- Other Methods --
        static void PrintLoop<T>(int iteratorLimit, T obj, string? optionalStringFormat = null)
        {
            //if ((obj)?.GetType() == typeof(List<T>))
            if (obj is List<string> list)
            {
                //List<T>? list = (obj as List<T>);
                if(optionalStringFormat != null)
                {
                    for (int i = 0; i < iteratorLimit; i++)
                    {
                        Console.Write(optionalStringFormat!, list[i]);
                    }
                }
                else
                {
                    foreach (var item in list!)
                    {
                        Console.WriteLine(item);
                    }
                }
            }
            else
            {
                for (int i = 0; i < iteratorLimit; i++)
                {
                    Console.Write(obj);
                }
            }
        }

        static int GetMin<T>(IEnumerable<T> list)
        {
            T[] arr = list.ToArray();
            if (arr.Length == 0) return 0;

            T min = arr[0];
            foreach (T item in arr)
            {
                if (Convert.ToInt16(item) < Convert.ToInt16(min))
                {
                    min = item;
                }
            }
            return Convert.ToInt16(min);
        }

        static bool FindVal<T>(T[][] arr2D, int target)
        {
            for (int y = 0; y < arr2D.Length; y++)
            {
                for (int x = 0;  x < arr2D[y].Length; x++)
                {
                    if (int.TryParse(arr2D[y][x]!.ToString()!, out int result))
                    {
                        if (result == target)
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return false;
        }

        //static int CheckNumOfLines(T[][])

        #endregion
    }
    public static class ExtensionHelper
    {
        /// <summary>
        /// obj must be string and this return its length
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetLength<T>(this T obj)
        {
            return obj!.ToString()!.Length;
        }
    }


}
