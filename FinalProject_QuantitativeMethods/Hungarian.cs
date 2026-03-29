using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

namespace FinalProject_QuantitativeMethods
{
    internal static class Hungarian
    {
        static int _row = 0;
        static int _column = 0;
        static int[][]? orgMatrix;
        static int[][]? step1Matrix;
        static int[][]? step2Matrix;
        static string[][]? step3Matrix;
        static string[][]? tempArr;
        //static int numberOfLinesToCoverZero;

        public static async Task MethodAsync<T>(List<T> rowNames, List<T> columnNames, int[][] arr)
        {
            _row = rowNames.Count;
            _column = columnNames.Count;

            orgMatrix = new int[_row][]; // holds the actual matrix
            step2Matrix = new int[_row][];
            step2Matrix = new int[_row][];
            step3Matrix = new string[_row][];
            tempArr = new string[_row][]; // holds the matrix with both horizontal and vertical lines

            for (int y = 0; y < _row; y++)
            {
                orgMatrix[y] = new int[_column];
                step2Matrix[y] = new int[_column];
                for (int x = 0; x < _column; x++)
                {
                    orgMatrix[y][x] = arr[y][x];
                    Console.Write(arr[y][x]);
                }
                Console.WriteLine(" ");
            }

            // Print the current matrix before the first step
            PrintMatrix(rowNames, columnNames, arr);

            step1Matrix = Step1(arr);
            Console.WriteLine("Step 1");
            PrintMatrix(rowNames, columnNames, step1Matrix);

            step2Matrix = Step2(step1Matrix);

            Console.WriteLine("Step 2");
            PrintMatrix(rowNames, columnNames, step2Matrix);

            step3Matrix = await Step3Async(step2Matrix, rowNames);

            Console.WriteLine("Step 3");
            PrintTempMatrix(rowNames, columnNames, step3Matrix);

            bool isNumberOfLinesEnough = CountTheCoverLines(step3Matrix, arr.Length);

            if (!isNumberOfLinesEnough)
            {
                //initialize
                StoredResults.stringMatrix = new string[_row][];
                StoredResults.intMatrix = new int[_row][];
                StoredResults.stringMatrix.Initialize();
                StoredResults.intMatrix.Initialize();

                Console.WriteLine("Step 4");
                StoredResults.stringMatrix = await Step4Async(step3Matrix, step2Matrix, rowNames);
                PrintTempMatrix(columnNames, rowNames, StoredResults.stringMatrix);

                isNumberOfLinesEnough = CountTheCoverLines(StoredResults.stringMatrix, arr.Length);

                while (!isNumberOfLinesEnough)
                {
                    StoredResults.stringMatrix = await Step4Async(StoredResults.stringMatrix, StoredResults.intMatrix,rowNames);
                    
                    isNumberOfLinesEnough = CountTheCoverLines(StoredResults.stringMatrix, arr.Length);
                    Console.WriteLine("Inside the while loop");
                    PrintTempMatrix(columnNames, rowNames, StoredResults.stringMatrix);
                    if (isNumberOfLinesEnough) break;
                }

                var result = FinalStepAssigningValues(StoredResults.stringMatrix, orgMatrix, StoredResults.intMatrix);

                //PrintTempMatrix(rowNames, columnNames, result);
                ProjectFinalOutput(result, rowNames, columnNames);
            }
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
            //int currentNumberOfLines = 0;
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
            
            bool isDone = false;

            // The covering process of zeros with horizontal and vertical lines using the "X"
            while (!isDone)
            {
                for (int y = 0; y < arr.Length; y++)
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

                            if (arr[y][x] == 0 && zeroCounterInColumn >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
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
                                if (tempMatrix[y][x] == null)tempMatrix[y][x] = (arr[y][x]).ToString();
                            }

                            if (zeroCounterInColumn == 0 && x == arr[y].Length - 1)
                            {
                                thereAreNoZeroInColumn = true;
                                break;
                            }
                        }

                        if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
                        {
                            bool isCovered = false;

                            for (int x = 0; x < arr[y].Length; x++)
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
                        else if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1) && thereAreNoZeroInColumn )
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

            return await Task.FromResult(tempMatrix);
        }

        public static async Task<string[][]> Step4Async<T>(string[][] arr, int[][] step2arr, List<T> rowNames)
        {
            string[][] Arr2D = new string[arr.Length][];
            Arr2D = Initializer(Arr2D);
            int minValue;
            Dictionary<string, int> uncoveredValues = new Dictionary<string, int>(); // This holds the uncovered values and their coordinates
            List<int> resultNewUncoveredValues = new List<int>();

            // Filters the arr and gets the uncovered values
            for (int i = 0; i < arr.Length; i++)
            {
                uncoveredValues = FilterAssignToDict(arr, (n) => n != "x");
            }

            minValue = GetMin(uncoveredValues.Values);
            dynamic uncoveredValuesToSub = uncoveredValues.Values.Select(n => int.Parse(n.ToString())).ToArray();
            resultNewUncoveredValues = SubtractionWithMinValue((int[])uncoveredValuesToSub, n => n - minValue).ToList();

            Dictionary<string, int> intersectedValues = new Dictionary<string, int>();

            intersectedValues = CheckIntersect(arr, step2arr);

            Console.WriteLine("The intercepted values");
            foreach (var value in intersectedValues.Keys)
            {
                string[] keys = new string[2];
                keys = value.ToString().Split(',');

                int targetedValue = intersectedValues[value];
                Console.WriteLine(targetedValue);
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
                        for (int k = 0; k < resultNewUncoveredValues.Count(); k++)
                        {
                            string key = string.Format("{0},{1}", i, j);
                            if (step2arr[i][j] == (resultNewUncoveredValues[k] + minValue) && uncoveredValues.ContainsKey(key))
                            {
                                step2arr[i][j] = resultNewUncoveredValues[k];
                                break;
                            }
                        }
                    }
                }
            }


            Console.WriteLine("Matrix from step2 arr");
            PrintMatrix(rowNames, rowNames, step2arr);

            StoredResults.intMatrix = step2arr;

            Arr2D = await Step3Async(step2arr, rowNames);

            Console.WriteLine("Matrix from arr2D");
            PrintTempMatrix(rowNames, rowNames, Arr2D);

            
            return Arr2D;
        }

        public static string[][] FinalStepAssigningValues<T>(T[][] matrix, int[][] intMatrix, int[][] resultMatrixWithZeros)
        {
            string[][] resultMatrix = new string[matrix.Length][];
            resultMatrix = Initializer(resultMatrix);
            bool[] columns = new bool[matrix.Length];
            bool[] rows = new bool[matrix.Length];
            Dictionary<string, int> targetedZeroLocations = new Dictionary<string, int>();
            Dictionary<string, int> numberOfZeroPerLocation = new Dictionary<string, int>();
            Dictionary<string, int> exactLocationOfZero = new Dictionary<string, int>();

            for (int i = 0; i < matrix.Length; i++)
            {
                columns[i] = false; rows[i] = false;
            }

            // Assign location with their corresponding number of Zero            
            // for column
            for (int y = 0; y < matrix.Length; y++)
            {
                string currentLocation = string.Format("Column {0}", y);
                int zeroInColumn = 0;
                for (int x = 0; x < matrix[y].Length; x++)
                {
                    if (resultMatrixWithZeros[x][y] == 0) zeroInColumn++;
                }
                numberOfZeroPerLocation[currentLocation] = zeroInColumn;
                zeroInColumn = 0;
            }
            // for row
            for (int y = 0; y < matrix.Length; y++)
            {
                string currentLocation = string.Format("Row {0}", y);
                int zeroInRow = 0;
                for (int x = 0; x < matrix[y].Length; x++)
                {
                    if (resultMatrixWithZeros[y][x] == 0)
                    {
                        exactLocationOfZero[$"{y},{x}"] = 0;
                        zeroInRow++; 
                    }

                }
                numberOfZeroPerLocation[currentLocation] = zeroInRow;
                zeroInRow = 0;
            }

            for (int y = 0; y < resultMatrixWithZeros.Length; y++)
            {
                string currentRowLocation = string.Format("Row {0}", y);
                int numberOfZeroInRow = numberOfZeroPerLocation![currentRowLocation];

                for (int x = 0; x < resultMatrixWithZeros[y].Length; x++)
                {
                    var currentValue = resultMatrixWithZeros[y][x];
                    bool isItOkayToPlaceIt = CheckZerosInEachRow(resultMatrixWithZeros, x, y);

                    if (currentValue != 0) continue; // skip if value is not 0
                    if (numberOfZeroInRow == 1 && columns[x] == false && rows[y] == false)
                    {
                        resultMatrix[y][x] = (orgMatrix![y][x]).ToString();
                        targetedZeroLocations[$"{y},{x}"] = orgMatrix![y][x];
                        columns[x] = true;
                        rows[y] = true;
                        break;
                    }
                    else if (numberOfZeroInRow > 1 && isItOkayToPlaceIt && !columns[x] && !rows[y])
                    {
                        resultMatrix[y][x] = (orgMatrix![y][x]).ToString();
                        targetedZeroLocations[$"{y},{x}"] = orgMatrix![y][x];
                        columns[x] = true;
                        rows[y] = true;
                        break;
                    }
                    else if (!isItOkayToPlaceIt)
                    {
                        if (y != resultMatrix.Length - 1)
                        {
                            for (int i = 0; i < (resultMatrix.Length - x); i++)
                            {
                                if (i == 0) continue; // skip by 1
                                int targetX = x == (resultMatrixWithZeros[y].Length - 1) ? x : (x + i);
                                var targetVar = resultMatrixWithZeros[y][ targetX];
                                if (targetVar == 0 && !columns[x] && !rows[targetX])
                                {
                                    resultMatrix[y][targetX] = (orgMatrix![y][targetX]).ToString();
                                    targetedZeroLocations[$"{y},{targetX}"] = orgMatrix![y][targetX];
                                    columns[targetX] = true;
                                    rows[y] = true;
                                    numberOfZeroPerLocation[currentRowLocation] = numberOfZeroInRow - 1;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!columns[x] && !rows[y] && currentValue == 0)
                            {
                                resultMatrix[y][x] = (orgMatrix![y][x]).ToString();
                            }
                        }
                    }
                    else
                    {
                        resultMatrix[y][x] = "X";
                    }


                }
            }

            // Finalizing and Assign the values in the result matrix
            for (int y = 0; y < matrix.Length; y++)
            {
                for (int x = 0; x < matrix[y].Length; x++)
                {
                    string currentValueInResultMatrix = resultMatrix[y][x];
                    int? currentValue = resultMatrixWithZeros[y][x];
                    string currentLocation = string.Format("{0},{1}", y, x);

                    if (currentValue != 0 || currentValueInResultMatrix == null)
                    {
                        resultMatrix[y][x] = "X";
                    }
                    else if (targetedZeroLocations.ContainsKey(currentLocation) && currentValue == 0)
                    {
                        resultMatrix[y][x] = (targetedZeroLocations[currentLocation]).ToString();
                    }
                }
            }

            return resultMatrix;
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

        #region -- Calculation Method --

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

        /// <summary>
        /// This method filters a matrix depends on the condition applied then assign uncovered values to a Dictionary then return it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="func"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Custom method. 
        /// Initializes a 2D array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        static T[][] Initializer<T>(T[][] arr)
        {
            T[][] tempArr = new T[arr.Length][];
            for (int i = 0; i < arr.Length; i++)
            {
                tempArr[i] = new T[arr.Length];
            }
            return tempArr;
        }

        /// <summary>
        /// This method returns a dictionary of exact location of intersected point as key then the value is the exact value based on the array2D
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr2D"></param>
        /// <param name="origArr2D"></param>
        /// <returns></returns>
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
                    else if (currentValue == string.Format("{0},{1}", 0, arr2D.Length - 1))
                    {
                        corners[1] = currentValue;
                        continue;
                    }
                    else if (currentValue == string.Format("{0},{1}", arr2D.Length - 1, 0))
                    {
                        corners[2] = currentValue;
                    }
                    else if (currentValue == string.Format("{0},{1}", arr2D.Length - 1, arr2D.Length - 1))
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
                                    if ((arr2D[y][x + 1]!).ToString() == "X" && (arr2D[y][arr2D.Length - 1]!.ToString() == "X") &&
                                        (arr2D[y + 1][x]!).ToString() == "X" && (arr2D[arr2D.Length - 1][x]!.ToString() == "X"))
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 1:
                                    if ((arr2D[y][x - 1]!).ToString() == "X" && (arr2D[y][0]!.ToString() == "X")&&
                                        (arr2D[y + 1][arr2D.Length - 1]!).ToString() == "X" && (arr2D[arr2D.Length - 1][arr2D.Length-1])!.ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 2:
                                    if ((arr2D[arr2D.Length - 2][x]!).ToString() == "X" && (arr2D[0][x])!.ToString() == "X" &&
                                        (arr2D[y][x+1]!).ToString() == "X" && (arr2D[y][arr2D.Length - 1])!.ToString() == "X")
                                    {
                                        Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                                    }
                                    break;
                                case 3:
                                    if ((arr2D[arr2D.Length - 2][arr2D.Length - 1]!).ToString() == "X" && (arr2D[0][arr2D.Length-1])!.ToString() == "X"&&
                                        (arr2D[y][x - 1]!).ToString() == "X" && (arr2D[y][0])!.ToString() == "X")
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
                        if ((arr2D[y][0]!).ToString() == "X" && (arr2D[y][arr2D.Length - 1]!).ToString() == "X" &&
                            (arr2D[arr2D.Length - 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( y > 0 && y < arr2D.Length - 1 && x == 0)
                    {
                        if ((arr2D[0][x]!).ToString() == "X" && (arr2D[y][arr2D.Length - 1]!).ToString() == "X" &&
                            (arr2D[arr2D.Length - 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( y > 0 && y < arr2D.Length - 1 && x == arr2D.Length - 1)
                    {
                        if ((arr2D[0][x]!).ToString() == "X" && (arr2D[y][0]!).ToString() == "X" &&
                            (arr2D[arr2D.Length - 1][x]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }
                    else if ( x > 0 && x < arr2D.Length - 1 && y == arr2D.Length - 1)
                    {
                        if ((arr2D[y][0]!).ToString() == "X" && (arr2D[0][x]!).ToString() == "X" &&
                            (arr2D[y][arr2D.Length - 1]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }

                    //Check the inner location if theres an intersecting line
                    if ( x > 0 && x < arr2D.Length - 1 &&
                        y > 0 && y < arr2D.Length - 1)
                    {
                        if ((arr2D[0][x]!).ToString() == "X" && (arr2D[arr2D.Length - 1][x]!).ToString() == "X" &&
                            (arr2D[y][0]!).ToString() == "X" && (arr2D[y][arr2D.Length - 1]!).ToString() == "X")
                        {
                            Values.Add(locationValue, Convert.ToInt16(origArr2D[y][x]));
                        }
                    }

                   
                }
            }

            return Values;
        }

        /// <summary>
        /// This method counts the vertical and horizontal lines inside the 2D array or the matrix.
        /// Returns true if the expected number of lines is equal to the actual number of lines, otherwise false
        /// </summary>
        /// <param name="arr">The matrix you want to check</param>
        /// <param name="numberOfLines">The number of horizontal and vertical lines expected to exist in the matrix</param>
        /// <returns></returns>
        static bool CountTheCoverLines(string[][] arr, int numberOfLines)
        {
            int currentNumberOfLines = 0;

            for (int y = 0; y < arr.Length; y++)
            {
                #region -- check by column --
                if (y != 0) break;
                for (int x = 0; x < arr[y].Length; x++)
                {
                    var currentValue = arr[y][x];

                    if (currentValue == "X")
                    {
                        bool isAllValueCovered = true;
                        for (int i = 0; i < arr[y].Length; i++)
                        {
                            var currentColumnValue = arr[i][x];
                            if (currentColumnValue != "X")
                            {
                                isAllValueCovered = false;
                                break;
                            }
                        }

                        if (isAllValueCovered) currentNumberOfLines++;
                        if (!isAllValueCovered) isAllValueCovered = true;
                    }
                }
                #endregion

                #region -- check by row --
                for (int x = 0; x < arr[y].Length; x++)
                {
                    var currentValue = arr[x][y];
                    
                    if (currentValue == "X")
                    {
                        bool isAllValueCovered = true;
                        for (int i = 0; i < arr[y].Length; i++)
                        {
                            var currentRowValue = arr[x][i];
                            if (currentRowValue != "X")
                            {
                                isAllValueCovered = false;
                                break;
                            }
                        }

                        if (isAllValueCovered) currentNumberOfLines++;
                        if (!isAllValueCovered) isAllValueCovered = true;
                    }
                }
                #endregion
            }

            if (currentNumberOfLines == numberOfLines || currentNumberOfLines == (numberOfLines * 2)) return true;
            return false;
        }

        /// <summary>
        /// This method checks each row of a matrix if each row contains more than 1 zero it returns true, otherwise false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static bool CheckZerosInEachRow<T>(T[][] matrix, int x, int y)
        {
            bool isAllContainsMoreThan0neZero = true;
            int zeroCounter = 0;

            for(int i = y; i < matrix.Length; i++)
            {
                for (int j = x;  j < matrix[i].Length; j++)
                {
                    var currentValue = matrix[j][i];

                    if (Convert.ToInt16(currentValue) == 0)
                    {
                        zeroCounter++;
                    }
                }
                if (zeroCounter == 1) return false;
                zeroCounter = 0;
            }

            return isAllContainsMoreThan0neZero;
        }

        static void ProjectFinalOutput<T>(string[][] matrix, List<T> rowNames, List<T> columnNames)
        {
            Console.WriteLine("Final Result!");

            PrintTempMatrix(rowNames, columnNames, matrix);

            Console.WriteLine(" ");

            var answer = new StringBuilder();
            var values = new StringBuilder();
            int totalValue = 0;

            answer.AppendLine("Assign rowName to columnName (value)");

            for (int y = 0; y < matrix.Length; y++)
            {
                answer.Replace("rowName", (rowNames[y]!).ToString());
                var rowName = (rowNames[y]!).ToString();
                for (int x = 0; x <  matrix[y].Length; x++)
                {
                    if (matrix[y][x] != "X")
                    {
                        answer.Replace("value", (matrix[y][x]).ToString());
                        answer.Replace("columnName",( columnNames[x]!).ToString());
                        var name = (columnNames[x]!).ToString(); 
                        if (y == 0)
                        {
                            values.Append(matrix[y][x]);
                        }
                        else
                        {
                            values.Append(" + " + matrix[y][x]);
                        }
                        totalValue += Convert.ToInt16(matrix[y][x]);
                        Console.WriteLine(answer.ToString());
                        answer.Replace((matrix[y][x]).ToString(), "value");
                        answer.Replace(name!, "columnName");
                        break;
                    }
                }
                answer.Replace(rowName!, "rowName");
            }
            Console.WriteLine("Z = " + values);
            Console.WriteLine("Z = " + totalValue);
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

    public static class StoredResults
    {
        public static string[][]? stringMatrix;
        public static int[][]? intMatrix;
    }
}
