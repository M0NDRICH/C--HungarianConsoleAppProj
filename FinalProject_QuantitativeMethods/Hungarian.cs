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
        static int[][]? step2Arr2D;
        static int numberOfLinesToCoverZero;

        public static async Task MethodAsync<T>(List<T> rowNames, List<T> columnNames, int[][] arr)
        {
            _row = rowNames.Count;
            _column = columnNames.Count;

            array2D = new int[_row][]; // holds the actual matrix
            step2Arr2D = new int[_row][];
            tempArr = new string[_row][]; // holds the matrix with both horizontal and vertical lines

            for (int y = 0; y < _row; y++)
            {
                array2D[y] = new int[_column];
                step2Arr2D[y] = new int[_column];
                for (int x = 0; x < _column; x++)
                {
                    array2D[y][x] = arr[y][x];
                    Console.Write(arr[y][x]);
                }
                Console.WriteLine(" ");
            }

            // Print the current matrix before the first step
            PrintMatrix(rowNames, columnNames, arr);

            array2D = Step1(arr);

            PrintMatrix(rowNames, columnNames, array2D);

            array2D = Step2(array2D);

            PrintMatrix(rowNames, columnNames, array2D);

            Console.WriteLine("Step 3");
            step2Arr2D = array2D;
            tempArr = await Step3Async(array2D, rowNames);


            PrintTempMatrix(rowNames, columnNames, tempArr);

            var step4result = await Step4Async(tempArr, step2Arr2D, rowNames);
            bool isItpass = CountTheCoverLines(step4result, arr.Length);

            Console.WriteLine(isItpass);
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

            for (int i = 0; i < Dimension; i++)
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
            // for number of zero per row or column
            Dictionary<string, int> values = new Dictionary<string, int>();

            // the absolute coordinates of zero 
            Dictionary<string, int> zeroLocs = new Dictionary<string, int>();

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
                    if (currentValue == 0)
                    {
                        var location = string.Format("{0},{1}", i, j);
                        zeroLocs[location] = currentValue;
                        zeroCounter++; 
                    }
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
            /*
            while (loopIterator < arr[0].Length)
            {
                if (loopIterator == 0)
                {
                    //For row
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string targetRow = string.Format("Row {0}", i);
                        // if zero is equal or more than half size of the dimension then consider to make it cross or cover it
                        if (values[targetRow] >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1)) 
                        {
                            isValueCover[targetRow] = true;
                            currentNumberOfLines++;
                            numOfValues--;

                            for (int j = 0; j < arr.Length; j++)
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
                    //For column
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
                numberOfLinesToCoverZero = currentNumberOfLines;
                Console.WriteLine("Number of lines " + numberOfLinesToCoverZero);
                if (currentNumberOfLines == rowNames.Count)
                    return await Task.FromResult(tempMatrix);

                int nullCounter = 0;

                for (int i = 0; i < tempMatrix.Length; i++)
                {
                    if (tempMatrix[i].Contains(null))
                    {
                        nullCounter++;
                    }
                }

                if (nullCounter == 0)
                {
                    bool isAllZeroNotCovered = FindVal(tempMatrix, 0);
                    if (!isAllZeroNotCovered)
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
                            for (int j = 0; j < tempMatrix[i].Length; j++)
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
            */
            // for drawing the vertical/horizontal lines 
            bool isDone = false; 
            /*
            while (!isDone)
            {
                for (int y = 0; y < arr.Length; y++)
                {
                    var currentRow = string.Format("Row {0}", y);
                    var zeroInRow = values[currentRow];

                    if (zeroInRow == 0)
                    {
                        for (int i = 0; i < arr.Length; i++)
                        {
                            tempMatrix[y][i] = (arr[y][i]).ToString();
                        }
                        continue;
                    }
                    
                    for (int x = 0; x < arr.Length; x++)
                    {
                        if (arr[y][x] != 0)
                        {
                            tempMatrix[y][x] = (arr[y][x]).ToString();
                            //continue;
                        };

                        var currentColumn = string.Format("Column {0}", x);
                        var zeroInColumn = values[currentColumn];
                        if (zeroInColumn <= 1)
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                if ((arr[i][y]) == 0)
                                tempMatrix[i][y] = (arr[i][y]).ToString();
                            }
                            //continue;
                        }

                        if (zeroInColumn >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                tempMatrix[i][y] = "X";
                            }
                            //continue;
                        }
                        else if (zeroInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            for (int i = 0; i < arr.Length; i++)
                            {
                                tempMatrix[y][i] = "X";
                            }
                            //continue;
                        }
                    }
                }

                if (FindVal(tempMatrix, 0))
                {
                    for (int y = 0; y < arr.Length; y++)
                    {
                        for (int x = 0; x < arr[y].Length; x++)
                        {
                            double random = Random.Shared.NextDouble();

                            if (tempMatrix[y][x] == "0")
                            {
                                bool coverRow = Random.Shared.Next(2) == 0; // 50/50 chance

                                if (coverRow)
                                {
                                    for (int k = 0; k < arr.Length; k++) tempMatrix[y][x] = "X";
                                }
                                else
                                {
                                    for (int k = 0; k < arr.Length; k++) tempMatrix[y][x] = "X";
                                }
                            }
                        }
                    }
                    break;
                }
                isDone = true;
            }
            */

            while (!isDone)
            {
                for(int y = 0; y < arr.Length; y++)
                {
                    var keyRow = string.Format("Row {0}", y);
                    var zeroCounterInRow = values[keyRow];
                    if (zeroCounterInRow != 0)
                    {
                        bool thereAreNoZeroInColumn = false;
                        
                        for (int x = 0; x < arr[y].Length; x++)
                        {
                            var location = string.Format("Column {0}", x);
                            int zeroCounterInColumn = values[location];

                            if (arr[y][x] == 0 && zeroCounterInColumn > 1)
                            {
                                for (int i = 0; i < arr[y].Length; i++)
                                {
                                    tempMatrix[i][x] = "X";
                                }
                            }
                            else if (arr[y][x] == 0 && zeroCounterInColumn == 1)
                            {
                                tempMatrix[y][x] = (arr[y][x]).ToString();
                            }
                            else
                            {
                                tempMatrix[y][x] = (arr[y][x]).ToString();
                            }

                            if (zeroCounterInColumn == 0 && x == arr[y].Length-1)
                            {
                                thereAreNoZeroInColumn = true;
                                break;
                            }
                        }

                        if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            bool isCovered = false;

                            for(int x =  0; x < arr[y].Length; x++)
                            {
                                if (tempMatrix[y][x] == "X") isCovered = true;
                            }

                            if (!isCovered)
                            {
                                for (int x = 0; x < arr[y].Length; x++)
                                {
                                    tempMatrix[y][x] = "X";
                                }
                            }
                        }
                        else if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1) && thereAreNoZeroInColumn)
                        {
                            for (int x = 0; x < arr[y].Length; x++)
                            {
                                tempMatrix[y][x] = "X";
                            }
                        }
                    }
                    else
                    {
                        for (int x = 0; x < arr[y].Length; x++)
                        {
                            tempMatrix[y][x] = (arr[y][x]).ToString();
                        }
                    }

                }

                if (FindVal(tempMatrix, 0))
                {
                    for (int y = 0; y < arr.Length; y++)
                    {
                        for (int x = 0; x < arr[y].Length; x++)
                        {
                            if (tempMatrix[y][x] == "0")
                            {
                                for (int x2 = 0; x2 < arr[y].Length; x2++) tempMatrix[y][x2] = "X";
                            }
                        }
                    }
                }

                isDone = true;
            }

            //while (loopIterator < arr[0].Length)
            //{
            //    //string[][] rows = new string[arr.Length][];
            //    //string[][] columns = new string[arr.Length][];
            //    //int maxLine = rowNames.Count() - 1;

            //    //rows = Initializer(rows);
            //    //columns = Initializer(columns);

            //    for (int y = 0; y < arr.Length; y++)
            //    {
            //        //for (int x = 0; x < arr.Length; x++)
            //        //{

            //        //}

            //        if (loopIterator == 0)
            //        {
            //            //string targetValue = string.Format("Row {0}", y);
            //            //for (int x = 0; x < arr.Length; x++)
            //            //{
            //            //    string otherValue = string.Format("Column {0}", y);

            //            //    if ()
            //            //}
            //        }
            //    }

            //    loopIterator++;
            //}

            return await Task.FromResult(tempMatrix);
        }

        public static async Task<string[][]> Step4Async<T>(string[][] arr, int[][] step2arr, List<T> rowNames)
        {
            //List<string> uncoveredValues = new List<string>();
            string[][] Arr2D = new string[arr.Length][];
            Arr2D = Initializer(Arr2D);
            int minValue;
            //IEnumerable<string> uncoveredValues = Enumerable.Empty<string>(); // This holds the uncovered values
            Dictionary<string, int> uncoveredValues = new Dictionary<string, int>(); // This holds the uncovered values and their coordinates
            List<int> resultNewUncoverdValues = new List<int>();

            // Filters the arr and gets the uncovered values
            for (int i = 0; i < arr.Length; i++)
            {
                //uncoveredValues = Filter2DArr(arr, (n) => n != "X");
                uncoveredValues = FilterAssignToDict(arr, (n) => n != "x");
            }

            //PrintLoop(uncoveredValues.Count(), uncoveredValues);

            minValue = GetMin(uncoveredValues.Values);
            //dynamic uncoveredValuesToSub = uncoveredValues.Select(n => int.Parse(n)).ToArray();
            dynamic uncoveredValuesToSub = uncoveredValues.Values.Select(n => int.Parse(n.ToString())).ToArray();
            resultNewUncoverdValues = SubtractionWithMinValue((int[])uncoveredValuesToSub, n => n - minValue).ToList();
            
            //PrintLoop(resultNewUncoverdValues.Count(), resultNewUncoverdValues);

            Dictionary<string, int> intersectedValues = new Dictionary<string, int>();

            intersectedValues = CheckIntersect(arr, step2arr);

            Console.WriteLine("The intercepted values");
            foreach (var value in intersectedValues.Keys)
            {
                string[] keys = new string[2];
                keys = value.ToString().Split(',');

                int targetedValue = intersectedValues[value];

                //add the min value to the corner points or the intersected points
                step2arr[Convert.ToInt16(keys[0])][Convert.ToInt16(keys[1])] = targetedValue +  minValue;
            }

            for (int i = 0; i < arr.Length; i++)
            {
                // need position
                for (int j = 0; j < arr[i].Length; j++)
                {
                    if ((step2arr[i][j]).ToString() != "X" ) //&& !intersectedValues.ContainsValue(step2arr[i][j]) )
                    {
                        for (int k = 0; k < resultNewUncoverdValues.Count(); k++)
                        {
                            string key = string.Format("{0},{1}", i, j);
                            if (step2arr[i][j] == (resultNewUncoverdValues[k] + minValue) && uncoveredValues.ContainsKey(key))
                                step2arr[i][j] = resultNewUncoverdValues[k];
                        }
                    }
                }
            }



            PrintMatrix(rowNames, rowNames, step2arr);

            Arr2D = await Step3Async(step2arr, rowNames);

            PrintTempMatrix(rowNames, rowNames, Arr2D);

            return Arr2D;
        }

        #endregion

        #region -- Print Matrix --
        public static void PrintMatrix<T>(List<T> rowNames, List<T> columnNames, int[][] values)
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
        /// <summary>
        /// This method prints in a loop depends on the iteratorLimit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iteratorLimit">The limitation of how many loops to execute</param>
        /// <param name="obj">can be list or any collection</param>
        /// <param name="optionalStringFormat">optional for string.Format</param>
        static void PrintLoop<T>(int iteratorLimit, T obj, string? optionalStringFormat = null)
        {
            //if ((obj)?.GetType() == typeof(List<T>))
            if (obj is List<string> list)
            {
                //List<T>? list = (obj as List<T>);
                if (optionalStringFormat != null)
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
            else if (obj is List<int> numList)
            {
                //List<T>? list = (obj as List<T>);
                if (optionalStringFormat != null)
                {
                    for (int i = 0; i < iteratorLimit; i++)
                    {
                        Console.Write(optionalStringFormat!, numList[i]);
                    }
                }
                else
                {
                    foreach (var item in numList!)
                    {
                        Console.WriteLine(item);
                    }
                }
            }
            else if (obj is IEnumerable<string> enumList)
            {
                foreach (var item in enumList)
                {
                    Console.Write(item);
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

        /// <summary>
        /// Returns the min value in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns true if the target value is found inside the 2d array, otherwise false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr2D"></param>
        /// <param name="target">The value you want to find in the list</param>
        /// <returns></returns>
        static bool FindVal<T>(T[][] arr2D, int target)
        {
            for (int y = 0; y < arr2D.Length; y++)
            {
                for (int x = 0; x < arr2D[y].Length; x++)
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


        static IEnumerable<T> Filter<T>(IEnumerable<T> list, Func<T, bool> func)
        {
            if (list is List<T> theList)
            {
                for (int i = 0; i < theList.Count; i++)
                {
                    if (func(theList[i]))
                    {
                        yield return theList[i];
                    }
                }
            }
            else if (list is T[] theArray)
            {
                for (int i = 0; i < theArray.Length; i++)
                {
                    if (func(theArray[i]))
                    {
                        yield return theArray[i];
                    }
                }
            }

        }

        static IEnumerable<T> Filter2DArr<T>(T[][] arr, Func<T, bool> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr[i].Length; j++)
                {
                    if (func(arr[i][j]))
                    {
                        yield return arr[i][j];
                    }
                }
            }
        }

        static Dictionary<string, int> FilterAssignToDict<T>(T[][] arr, Func<T, bool> func)
        {
            Dictionary<string, int> FilteredDictionary = new Dictionary<string, int>();

            for (int y = 0; y < arr.Length; y++)
            {
                for (int x = 0; x < arr[y].Length; x++)
                {
                    if (func(arr[y][x]))
                    {
                        string location = string.Format("{0},{1}", y, x);
                        if ((arr[y][x]!).ToString() == "X") continue;
                        FilteredDictionary[location] = Convert.ToInt16(arr[y][x]);
                    }
                }
            }

            return FilteredDictionary;
        }

        static T[][] Initializer<T>(T[][] arr)
        {
            T[][] tempArr = new T[arr.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                tempArr[i] = new T[arr.Length];
            }
            return tempArr;
        }

        static Dictionary<string, int> CheckIntersect<T>(T[][] arr2D, int[][] origArr2D)
        {
            T[][] newArr2D = new T[arr2D.Length][];
            Dictionary<string, int> Values = new Dictionary<string, int>();
            string[] corners = new string[4];

            // Calculate the dimension of arr2D then assign the corners location
            for (int i = 0; i < arr2D.Length; i++)
            {
                for (int j = 0; j < arr2D[i].Length; j++)
                {
                    var currentValue = string.Format("{0},{1}", i, j);
                    if (currentValue == "0,0")
                    {
                        corners[0] = currentValue;
                        continue;
                    }
                    else if (currentValue == string.Format("{0},{1}", 0, arr2D.Length))
                    {
                        corners[1] = currentValue;
                        continue;
                    }
                    else if (currentValue == string.Format("{0},{1}", arr2D.Length, 0))
                    {
                        corners[2] = currentValue;
                    }
                    else if (currentValue == string.Format("{0},{1}", arr2D.Length, arr2D.Length))
                    {
                        corners[3] = currentValue;
                    }
                }
            }

            for (int y = 0; y <  newArr2D.Length; y++)
            {
                for (int x = 0; x <  arr2D[y].Length; x++)
                {
                    var targetValue = arr2D[y][x];

                    var locationValue = string.Format("{0},{1}", y, x);
                    if ((targetValue!).ToString() != "X") continue; // ensure that the current location contains X
                    //check if the current location is equal to corners location then check if theres an intersecting line
                    for (int i = 0; i < corners.Length; i++)
                    {
                        if (locationValue == corners[i])
                        {
                            switch (i)
                            {
                                case 0:
                                    if ((arr2D[y][x + 1]!).ToString() == "X" &&
                                        (arr2D[y + 1][x]!).ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 1:
                                    if ((arr2D[y][x - 1]!).ToString() == "X" &&
                                        (arr2D[y + 1][arr2D.Length - 1]!).ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 2:
                                    if ((arr2D[arr2D.Length - 2][x]!).ToString() == "X" &&
                                        (arr2D[y][x+1]!).ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 3:
                                    if ((arr2D[arr2D.Length - 2][arr2D.Length - 1]!).ToString() == "X" &&
                                        (arr2D[y][x - 1]!).ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                            }
                        }
                    }
                    
                    //check the side locations
                    if(x > 0 && x < arr2D.Length - 1 && y == 0)
                    {
                        if ((arr2D[y][x - 1]!).ToString() == "X" && (arr2D[y][x + 1]!).ToString() == "X" &&
                            (arr2D[y + 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( y > 0 && y < arr2D.Length - 1 && x == 0)
                    {
                        if ((arr2D[y - 1][x]!).ToString() == "X" && (arr2D[y][x + 1]!).ToString() == "X" &&
                            (arr2D[y + 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( y > 0 && y < arr2D.Length - 1 && x == arr2D.Length - 1)
                    {
                        if ((arr2D[y - 1][x]!).ToString() == "X" && (arr2D[y][x - 1]!).ToString() == "X" &&
                            (arr2D[y + 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( x > 0 && x < arr2D.Length - 1 && y == arr2D.Length - 1)
                    {
                        if ((arr2D[y][x - 1]!).ToString() == "X" && (arr2D[y - 1][x]!).ToString() == "X" &&
                            (arr2D[y][x + 1]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }

                    //Check the inner location if theres an intersecting line
                    if ( x > 0 && x < arr2D.Length - 1 &&
                        y > 0 && y < arr2D.Length - 1)
                    {
                        if ((arr2D[y - 1][x]!).ToString() == "X" && (arr2D[y + 1][x]!).ToString() == "X" &&
                            (arr2D[y][x - 1]!).ToString() == "X" && (arr2D[y][x + 1]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }

                   
                }
            }

            return Values;
        }

        static bool CountTheCoverLines(string[][] arr, int numberOfLines)
        {
            int currentNumberOfLines = 0;


            for (int y = 0; y < arr.Length; y++)
            {
                var currentArr = arr[y];

                var targetArr = currentArr.Where(n => n == "X").ToList();
                if (targetArr.Count() == currentArr.Length) currentNumberOfLines++;

                for (int x = 0; x < arr[y].Length; x++)
                {
                    var targetValue = arr[x][y];
                    int counterX = 0;

                    if (targetValue == "X") counterX++;

                    if (counterX == targetValue.Length) currentNumberOfLines++;
                }
            }

            if (currentNumberOfLines == numberOfLines) return true;

            return false;
        }

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
