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
                }
            }

            // Print the current matrix before the first step
            PrintSeparator(rowNames, columnNames);
            Console.WriteLine("Actual Values");
            PrintMatrix(rowNames, columnNames, arr);

            step1Matrix = Step1(arr);
            PrintSeparator(rowNames, columnNames);
            Console.WriteLine("Step 1");
            PrintMatrix(rowNames, columnNames, step1Matrix);

            step2Matrix = Step2(step1Matrix);
            PrintSeparator(rowNames, columnNames);
            Console.WriteLine("Step 2");
            PrintMatrix(rowNames, columnNames, step2Matrix);

            step3Matrix = await Step3Async(step2Matrix, rowNames);
            PrintSeparator(rowNames, columnNames);
            Console.WriteLine("Step 3");
            PrintTempMatrix(rowNames, columnNames, step3Matrix);

            bool isNumberOfLinesEnough = CountTheCoverLines(step3Matrix, arr.Length);

            // if number of lines is not enough proceed to step 4
            if (!isNumberOfLinesEnough)
            {
                //initialize
                StoredResults.stringMatrix = new string[_row][];
                StoredResults.intMatrix = new int[_row][];
                StoredResults.stringMatrix.Initialize();
                StoredResults.intMatrix.Initialize();

                PrintSeparator(rowNames, columnNames);
                Console.WriteLine("Step 4");
                StoredResults.stringMatrix = await Step4Async(step3Matrix, step2Matrix, rowNames, columnNames);

                isNumberOfLinesEnough = CountTheCoverLines(StoredResults.stringMatrix, arr.Length);

                // if number of lines is not enough go back again to step 4
                while (!isNumberOfLinesEnough)
                {
                    StoredResults.stringMatrix = await Step4Async(StoredResults.stringMatrix, StoredResults.intMatrix, rowNames, columnNames);
                    
                    isNumberOfLinesEnough = CountTheCoverLines(StoredResults.stringMatrix, arr.Length);
                    if (isNumberOfLinesEnough) break;
                }

                var result = FinalStepAssigningValues(StoredResults.stringMatrix, orgMatrix, StoredResults.intMatrix);

                PrintSeparator(rowNames, columnNames);
                ProjectFinalOutput(result, rowNames, columnNames);
            }
            else
            {
                PrintSeparator(rowNames, columnNames);
                var resultOfStep3 = FinalStepAssigningValues(step3Matrix, orgMatrix, StoredResults.intMatrix!);
                ProjectFinalOutput(resultOfStep3, rowNames, columnNames);
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
            //while (!isDone)
            //{
            //    for (int y = 0; y < arr.Length; y++)
            //    {
            //        var keyRow = string.Format("Row {0}", y);
            //        var zeroCounterInRow = values[keyRow];
            //        if (zeroCounterInRow != 0)
            //        {
            //            bool thereAreNoZeroInColumn = false;
                        
            //            for (int x = 0; x < arr[y].Length; x++)
            //            {
            //                var location = string.Format("Column {0}", x);
            //                int zeroCounterInColumn = values[location];

            //                if (arr[y][x] == 0 && zeroCounterInColumn >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1) || zeroCounterInColumn >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
            //                {
            //                    for (int i = 0; i < arr[y].Length; i++)
            //                    {
            //                        tempMatrix[i][x] = "X";
            //                    }
            //                }
            //                else if (arr[y][x] == 0 && zeroCounterInColumn == 1)
            //                {
            //                    tempMatrix[y][x] = (arr[y][x]).ToString();
            //                }
            //                else
            //                {
            //                    if (tempMatrix[y][x] == null)tempMatrix[y][x] = (arr[y][x]).ToString();
            //                }

            //                if (zeroCounterInColumn == 0 && x == arr[y].Length - 1)
            //                {
            //                    thereAreNoZeroInColumn = true;
            //                    break;
            //                }
            //            }

            //            if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1))
            //            {
            //                bool isCovered = false;

            //                for (int x = 0; x < arr[y].Length; x++)
            //                {
            //                    if (tempMatrix[y][x] == "X") isCovered = true;
            //                }

            //                if (!isCovered)
            //                {
            //                    for (int x = 0; x < arr[y].Length; x++)
            //                    {
            //                        tempMatrix[y][x] = "X";
            //                    }
            //                }
            //            }
            //            else if (zeroCounterInRow >= (arr.Length % 2 == 0 ? (arr.Length / 2) : (arr.Length / 2) + 1) && thereAreNoZeroInColumn )
            //            {
            //                for (int x = 0; x < arr[y].Length; x++)
            //                {
            //                    tempMatrix[y][x] = "X";
            //                }
            //            }
                        
                        
            //        }
            //        else
            //        {
            //            for (int x = 0; x < arr[y].Length; x++)
            //            {
            //                tempMatrix[y][x] = (arr[y][x]).ToString();
            //            }
            //        }

            //    }

            //    if (FindVal(tempMatrix, 0))
            //    {
            //        for (int y = 0; y < arr.Length; y++)
            //        {
            //            for (int x = 0; x < arr[y].Length; x++)
            //            {
            //                if (tempMatrix[y][x] == "0")
            //                {
            //                    for (int x2 = 0; x2 < arr[y].Length; x2++) tempMatrix[y][x2] = "X";
            //                }
            //            }
            //        }
            //    }

            //    isDone = true;
            //}

            while (!isDone)
            {
                //Covering by column
                for (int y = 0; y <arr.Length; y++)
                {
                    string currentColumn = string.Format("Column {0}", y);
                    int numberOfZeroInColumn = values[currentColumn];

                    if (numberOfZeroInColumn >= (arr.Length % 2 == 0 ? arr.Length / 2 : (arr.Length / 2) + 1))
                    {
                        for (int x = 0; x < arr.Length; x++)
                        {
                            string currentRow = string.Format("Row {0}", x);
                            int numberOfZeroInRow = values[currentRow];
                            if (arr[x][y] == 0)
                            {
                                values[string.Format("Row {0}", x)] = values[string.Format("Row {0}", x)] - 1;
                                //values[currentColumn] = numberOfZeroInColumn - 1;
                            }

                            tempMatrix[x][y] = "X";
                        }
                    }
                }

                //Covering by row
                for (int x = 0; x < arr.Length; x++)
                {
                    string currentRow = string.Format("Row {0}", x);
                    string currentColumn = string.Format("Column {0}", x);
                    int numberOfZeroInRow = values[currentRow];
                    int numberOfZeroInColumn = values[currentColumn];

                    if (numberOfZeroInRow >= (arr.Length % 2 == 0 ? arr.Length / 2 : (arr.Length / 2) + 1))
                    {
                        for (int y = 0; y < arr.Length; y++)
                        {
                            tempMatrix[x][y] = "X";
                            if (arr[x][y] == 0)
                            {
                                //values[currentRow] = numberOfZeroInRow - 1;
                                values[string.Format("Column {0}", y)] = values[string.Format("Column {0}", y)] - 1;
                            }
                        }
                    }
                }

                //Assign org values to tempMatrix
                for (int y = 0; y < arr.Length; y++)
                {
                    for (int x = 0; x < arr.Length; x++)
                    {
                        if (tempMatrix[y][x] == null) tempMatrix[y][x] = (arr[y][x]).ToString();
                    }
                }

                // check if theres still zeros remaining in the matrix
                for (int y = 0; y < arr.Length; y++)
                {
                    string currentColumn = string.Format("Column {0}", y);
                    int valueInColumn = values[currentColumn];

                    if (valueInColumn > 1)
                    {
                        for (int x = 0; x < arr.Length; x++)
                        {
                            tempMatrix[x][y] = "X";
                        }
                    }
                }
                for (int y = 0; y < arr.Length; y++)
                {
                    string currentRow = string.Format("Row {0}", y);
                    int valueInRow = values[currentRow];

                    if (valueInRow > 1)
                    {
                        for (int x = 0; x < arr.Length; x++)
                        {
                            tempMatrix[y][x] = "X";
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

            StoredResults.intMatrix = arr;

            return await Task.FromResult(tempMatrix);
        }

        public static async Task<string[][]> Step4Async<T>(string[][] arr, int[][] step2arr, List<T> rowNames, List<T> columnNames)
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

            Console.WriteLine("The intersected values of the matrix");
            foreach (var value in intersectedValues.Keys)
            {
                string[] keys = new string[2];
                keys = value.ToString().Split(',');

                int targetedValue = intersectedValues[value];
                Console.WriteLine(targetedValue);
                //add the min value to the corner points or the intersected points
                step2arr[Convert.ToInt16(keys[0])][Convert.ToInt16(keys[1])] = targetedValue +  minValue;
            }
            Console.WriteLine(" ");
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


            Console.WriteLine("Uncovered Matrix");
            PrintMatrix(rowNames, columnNames, step2arr);
            Console.WriteLine("");

            StoredResults.intMatrix = step2arr;

            Arr2D = await Step3Async(step2arr, rowNames);

            Console.WriteLine("Covered Matrix");
            PrintTempMatrix(rowNames, columnNames, Arr2D);
            Console.WriteLine("");

            return Arr2D;
        }

        #region -- Temp Assigningvalues --
        /*
        static string[][] FinalStepAssigningValues<T>(T[][] matrix, int[][] intMatrix, int[][] resultMatrixWithZeros)
        {
            string[][] ResultMatrix = new string[matrix.Length][];
            ResultMatrix = Initializer(ResultMatrix);

            Dictionary<string, int> RowsAndTotalZero = new Dictionary<string, int>(); // Rows and their corresponding total number of zero
            Dictionary<string, int> ColumnsAndTotalZero = new Dictionary<string, int>(); // Columns and their corresponding total number of zero
            Dictionary<string, int> TargetedZero = new Dictionary<string, int>(); // This holds the assigned zero and its coordinates

            for (int y = 0; y < matrix.Length; y++)
            {
                string currentRow = string.Format("Row {0}", y);
                string currentColumn = string.Format("Column {0}", y);
                int zeroCounterRow = 0;
                int zeroCounterColumn = 0;
                for (int x = 0; x < matrix[y].Length; x++)
                {
                    if (resultMatrixWithZeros[y][x] == 0) zeroCounterRow++;
                    if (resultMatrixWithZeros[x][y] == 0) zeroCounterColumn++;
                }
                RowsAndTotalZero[currentRow] = zeroCounterRow;
                ColumnsAndTotalZero[currentColumn] = zeroCounterColumn;
            }

            if (RowsAndTotalZero.ContainsValue(1))
            {
                for (int y = 0; y < matrix.Length; y++)
                {
                    if (RowsAndTotalZero[string.Format("Row {0}", y)] == 1)
                    {
                        for (int x = 0; x < matrix[y].Length; x++)
                        {
                            if (resultMatrixWithZeros[y][x] == 0)
                            {
                                TargetedZero[string.Format("{0},{1}", y, x)] = intMatrix[y][x];
                                ColumnsAndTotalZero[string.Format("Column {0}", x)] = 1;
                                //RowsAndTotalZero[string.Format("Row {0}", y)] -= ;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < matrix.Length; y++)
            {
                var currentValue = RowsAndTotalZero[string.Format("Row {0}", y)];
                
                if (currentValue == 1)
                {
                    for (int x = 0; x < matrix.Length; x++)
                    {
                        if (resultMatrixWithZeros[y][x] == 0)
                        {
                            TargetedZero[string.Format("{0},{1}", y, x)] = intMatrix[y][x];
                            ColumnsAndTotalZero[string.Format("Column {0}", x)] = 1;
                        };
                    }
                }
                else if (currentValue > 1)
                {
                    bool isItOkToPlace = false;
                    for (int x = 0; x < matrix.Length; x++)
                    {
                        if (resultMatrixWithZeros[y][x] == 0 && ColumnsAndTotalZero[string.Format("Column {0}", x)] == 1)
                        {
                            TargetedZero[string.Format("{0},{1}", y, x)] = intMatrix[y][x];
                            break;
                        }
                        else if (resultMatrixWithZeros[y][x] == 0)
                        {
                            // loop then check each zero
                            int targetRow = 0;
                            bool thereAreNoZeroInThisColumn = false;
                            for (int i = (y == matrix.Length - 1 ? y : y + 1); i < matrix.Length; i++)
                            {
                                int numberOfZeroInRow = RowsAndTotalZero[string.Format("Row {0}", i)];
                                if (resultMatrixWithZeros[i][x] == 0 && RowsAndTotalZero[string.Format("Row {0}", i)] > 1)
                                {
                                    for (int j = 0; j < matrix.Length; j++)
                                    {
                                        if (resultMatrixWithZeros[i][j] == 0 && ColumnsAndTotalZero[string.Format("Column {0}", j)] != 1)
                                        {
                                            targetRow = i;
                                            isItOkToPlace = true;
                                        }
                                        if (resultMatrixWithZeros[i][j] == 0) --numberOfZeroInRow;
                                        if (numberOfZeroInRow == 1) break;
                                    }
                                }
                                //else { thereAreNoZeroInThisColumn = true; }
                            }

                            if (isItOkToPlace)
                            {
                                RowsAndTotalZero[string.Format("Row {0}", targetRow)] -= 1;
                                ColumnsAndTotalZero[string.Format("Column {0}", x)] = 1;
                                TargetedZero[string.Format("{0},{1}", y, x)] = intMatrix[y][x];
                                break;
                            }

                            if (thereAreNoZeroInThisColumn)
                            {
                                RowsAndTotalZero[string.Format("Row {0}", targetRow)] -= 1;
                                ColumnsAndTotalZero[string.Format("Column {0}", x)] = 1;
                                TargetedZero[string.Format("{0},{1}", y, x)] = intMatrix[y][x];
                                break;
                            }
                        }
                    }
                }
            }

            // Assignning the zeros with original value from the original matrix
            for (int y = 0; y < matrix.Length; y++)
            {
                for (int x = 0; x < matrix.Length; x++)
                {
                    var target = string.Format("{0},{1}", y, x);
                    if (TargetedZero.ContainsKey(target))
                    {
                        ResultMatrix[y][x] = (TargetedZero[target]).ToString();
                    }
                    else
                    {
                        ResultMatrix[y][x] = "X";
                    }
                }
            }


            return ResultMatrix;
        }

        static string[][] Fin<T>(T[][] matrix, int[][] intMatrix, int[][] resultMatrixWithZeros)
        {
            string[][] ResultMatrix = new string[resultMatrixWithZeros.Length][];
            ResultMatrix = Initializer(ResultMatrix);

            Dictionary<string, int> LocationAndZeroValue = new Dictionary<string, int>();
            Dictionary<string, bool> IsOccupied = new Dictionary<string, bool>();

            int zeroCounterRow = 0;
            int zeroCounterColumn = 0;

            for (int y = 0; y < resultMatrixWithZeros.Length; y++)
            {
                for (int x = 0; x < resultMatrixWithZeros.Length; x++)
                {
                    if (resultMatrixWithZeros[y][x] == 0) zeroCounterRow++;
                    if (resultMatrixWithZeros[x][y] == 0) zeroCounterColumn++; 
                }
                LocationAndZeroValue[string.Format("Row {0}", y)] = zeroCounterRow;
                LocationAndZeroValue[string.Format("Column {0}", y)] = zeroCounterColumn;
                IsOccupied[string.Format("Column {0}", y)] = false;
                IsOccupied[string.Format("Row {0}", y)] = false;
                zeroCounterRow = 0;
                zeroCounterColumn = 0;
            }

            if (LocationAndZeroValue.ContainsValue(1))
            {
                int locationContains1Zero = LocationAndZeroValue.Where(obj => obj.Value == 1).Select(obj => obj).ToList().Count();

                while (locationContains1Zero > 0)
                {
                    for (int y = 0; y < matrix.Length; y++)
                    {
                        var currentRow = string.Format("Row {0}", y);
                        if (LocationAndZeroValue[currentRow] != 1) continue;
                        for (int x = 0; x < matrix[y].Length; x++)
                        {
                            if (resultMatrixWithZeros[y][x] == 0)
                            {
                                ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                                IsOccupied[string.Format("Column {0}", x)] = true;
                                IsOccupied[string.Format("Row {0}", y)] = true;
                                LocationAndZeroValue = DeductAll(LocationAndZeroValue, string.Format("{0},{1}", y, x), resultMatrixWithZeros);
                                locationContains1Zero--;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < matrix.Length; y++)
            {
                var currentRowValue = LocationAndZeroValue[string.Format("Row {0}", y)];
                if (currentRowValue > 1)
                {
                    int totalZero = currentRowValue;
                    for (int x = 0; x < matrix.Length; x++)
                    {
                        if (resultMatrixWithZeros[y][x] == 0)
                        {
                            bool isOkayToPlace = false;
                            for (int i = (y == matrix.Length - 1 ? y : y + 1); i < matrix[y].Length; i++)
                            {
                                if (IsOccupied[string.Format("Row {0}", i)] == true) break;
                                if (LocationAndZeroValue[string.Format("Row {0}", i)] > 1)
                                {
                                    isOkayToPlace = true;
                                }
                                else if (LocationAndZeroValue[string.Format("Row {0}", i)] == 1)
                                {
                                    isOkayToPlace = false;
                                    break;
                                }

                            }

                            if (isOkayToPlace && !IsOccupied[string.Format("Column {0}", x)] && !IsOccupied[string.Format("Row {0}", y)])
                            {
                                ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                                IsOccupied[string.Format("Column {0}", x)] = true;
                                IsOccupied[string.Format("Row {0}", y)] = true;
                                LocationAndZeroValue = DeductAll(LocationAndZeroValue, string.Format("{0},{1}", y, x), resultMatrixWithZeros);
                                break;
                            }
                            else if (totalZero == 1 && !IsOccupied[string.Format("Column {0}", x)] && !IsOccupied[string.Format("Row {0}", y)])
                            {
                                ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                                IsOccupied[string.Format("Column {0}", x)] = true;
                                IsOccupied[string.Format("Row {0}", y)] = true;
                                LocationAndZeroValue = DeductAll(LocationAndZeroValue, string.Format("{0},{1}", y, x), resultMatrixWithZeros);
                                break;
                            }
                        }
                        totalZero--;
                    }
                }
                else if (currentRowValue == 1)
                {
                    for(int x = 0; x < matrix.Length; x++)
                    {
                        if (resultMatrixWithZeros[y][x] == 0)
                        {
                            ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                            IsOccupied[string.Format("Column {0}", x)] = true;
                            IsOccupied[string.Format("Row {0}", y)] = true;
                            LocationAndZeroValue = DeductAll(LocationAndZeroValue, string.Format("{0},{1}", y, x), resultMatrixWithZeros);
                        }
                    }
                }
            }

            return ResultMatrix;
        }
        */
        #endregion

        static string[][] FinalStepAssigningValues<T>(T[][]matrix, int[][] intMatrix, int[][] resultMatrixWithZeros)
        {
            int Length = matrix.Length;
            string[][] ResultMatrix = new string[Length][]; // This will hold the final matrix result and return it
            ResultMatrix = Initializer(ResultMatrix);

            string[][] ExactLocationOfZeroPerRow = new string[Length][];
            Dictionary<string, int> NumOfZeroPerLocation = new Dictionary<string, int>();
            Dictionary<string, bool> IsOccupied = new Dictionary<string, bool>();

            // Assigning the values to collections
            for (int y = 0; y < Length; y++)
            {
                int zeroCounterRow = 0;
                int zeroCounterColumn = 0;

                for (int x = 0; x < intMatrix[y].Length; x++)
                {
                    if (resultMatrixWithZeros[y][x] == 0) zeroCounterRow++;
                    if (resultMatrixWithZeros[x][y] == 0) zeroCounterColumn++;
                }
                NumOfZeroPerLocation[string.Format("Row {0}", y)] = zeroCounterRow;
                NumOfZeroPerLocation[string.Format("Column {0}", y)] = zeroCounterColumn;
                IsOccupied[string.Format("Row {0}", y)] = false;
                IsOccupied[string.Format("Column {0}", y)] = false;
                zeroCounterRow = 0;
                zeroCounterColumn = 0;
            }

            // Assign exact location of zeros
            for (int y = 0; y < Length; y++)
            {
                int size = NumOfZeroPerLocation[string.Format("Row {0}", y)];
                string[] currentArr = new string[size];
                for (int x = 0; x < resultMatrixWithZeros[y].Length; x++)
                {
                    if (resultMatrixWithZeros[y][x] != 0) continue;
                    currentArr[size - 1] = string.Format("{0},{1}", y, x);
                    if (size == 0) break;
                    size--;
                }

                if (size == 0)
                {
                    ExactLocationOfZeroPerRow[y] = currentArr;
                }
            }

            // Assigning to ResultMatrix

            // Automatic assign when there is a row that contains 1 zero
            if (NumOfZeroPerLocation.ContainsValue(1))
            {
                for (int y = 0; y < Length; y++)
                {
                    int zeroInRow = NumOfZeroPerLocation[string.Format("Row {0}", y)];
                    if (zeroInRow != 1) continue;
                    for (int x = zeroInRow; x < Length; x++)
                    {
                        int zeroInColumn = NumOfZeroPerLocation[string.Format("Column {0}", x)];

                        if (resultMatrixWithZeros[y][x] != 0) continue;
                        ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                        DeductAll(resultMatrixWithZeros, NumOfZeroPerLocation, y, x);
                        IsOccupied[string.Format("Row {0}", y)] = true;
                        IsOccupied[string.Format("Column {0}", x)] = true;
                        break;
                    }
                }
            }

            // Assign the values per each row
            // Only one zero must exist per row and column
            for (int y = 0; y < Length; y++)
            {
                if (IsOccupied[string.Format("Row {0}", y)]) continue;
                int zeroInRow = NumOfZeroPerLocation[string.Format("Row {0}", y)];

                if (zeroInRow > 1)
                {
                    int zeroCounter = zeroInRow;
                    // loop side by row
                    for (int x = 0; x < Length; x++)
                    {
                        if (resultMatrixWithZeros[y][x] == 0)
                        {
                            bool isOkToPlace = true;
                            // loop down by column
                            for (int i = y; i < Length; i++)
                            {
                                if (i == y) continue;
                                if (resultMatrixWithZeros[i][x] != 0) continue;
                                int rowVal = NumOfZeroPerLocation[string.Format("Row {0}", i)];
                                if (IsOccupied[string.Format("Row {0}", i)]) continue;
                                if (rowVal == 1)
                                {
                                    isOkToPlace = false;
                                    zeroCounter--;
                                    break;
                                }
                            }

                            if (isOkToPlace && !IsOccupied[string.Format("Column {0}",x)] && !IsOccupied[string.Format("Row {0}", y)])
                            {
                                ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                                DeductAll(resultMatrixWithZeros, NumOfZeroPerLocation, y, x);
                                IsOccupied[string.Format("Row {0}", y)] = true;
                                IsOccupied[string.Format("Column {0}", x)] = true;
                                break;
                            }

                            zeroCounter--;
                        }

                        if (zeroCounter == 1) break;
                    }

                    if (zeroCounter == 1)
                    {
                        string[] exactLoc = ExactLocationOfZeroPerRow[y];
                        int[] coordinates = exactLoc[0].Split(",").Select(obj => int.Parse(obj)).ToArray();
                        DeductAll(resultMatrixWithZeros, NumOfZeroPerLocation, coordinates[0], coordinates[1]);

                        ResultMatrix[coordinates[0]][coordinates[1]] = (intMatrix[coordinates[0]][coordinates[1]]).ToString();
                    }
                }
                else
                {
                    // Automatic assign since this row contains 1 zero
                    for (int x = 0; x < Length; x++)
                    {
                        if (IsOccupied[string.Format("Column {0}", x)] || IsOccupied[string.Format("Row {0}", y)]) continue;
                        if (resultMatrixWithZeros[y][x] != 0) continue;
                        ResultMatrix[y][x] = (intMatrix[y][x]).ToString();
                        DeductAll(resultMatrixWithZeros, NumOfZeroPerLocation, y, x);
                        IsOccupied[string.Format("Row {0}", y)] = true;
                        IsOccupied[string.Format("Column {0}", x)] = true;
                        break;
                    }
                }
            }

            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Length; x++)
                {
                    var currentVal = ResultMatrix[y][x];
                    if (currentVal != null) continue;
                    ResultMatrix[y][x] = "X";
                }
            }
            

            return ResultMatrix;
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

        /// <summary>
        /// This method projects or display the final output of the hungarian method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="rowNames"></param>
        /// <param name="columnNames"></param>
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

        /// <summary>
        /// Prints a separator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowNames"></param>
        /// <param name="columnNames"></param>
        static void PrintSeparator<T>(List<T> rowNames, List<T> columnNames)
        {
            int rowSpace = 0;
            for (int i = 0; i < rowNames.Count; i++)
            {
                if (i == 0)
                {
                    rowSpace = (rowNames[i]!).GetLength();
                    continue;
                }
                if (rowSpace < rowNames[i]!.GetLength()) rowSpace = rowNames[i]!.GetLength();
            }

            int columnSpace = 0;
            for (int i = 0; i < columnNames.Count; i++)
            {
                columnSpace += columnNames[i]!.GetLength() + 2;
            }

            int loopLimit = rowSpace  + columnSpace + 2;

            for (int i = 0; i < loopLimit; i++) Console.Write("-");

            Console.WriteLine("");
        }

        /// <summary>
        /// Displays the title of the project
        /// </summary>
        /// <returns></returns>
        public static async Task DisplayTitle()
        {
            string title = await File.ReadAllTextAsync("ProjectTitle.txt");

            Console.WriteLine(title);
            Console.WriteLine("Press any key to continue....");
            Console.ReadKey();
            Console.Clear();
        }

        public static async Task Simulate()
        {
            String[] rowNames;
            String[] columnNames;
            int[][] theMatrix;
            int Dimension;

            Console.WriteLine("What is the Dimension of the 2D array or the matrix? (e.g., 4, 3)");
            Dimension = int.Parse(Console.ReadLine()!);

            rowNames = new String[Dimension];
            columnNames = new String[Dimension];
            theMatrix = new int[Dimension][];
            theMatrix = Initializer(theMatrix);

            Console.WriteLine("");

            Console.WriteLine("Time to assign column names.");
            for(int i = 0; i < Dimension; i++)
            {
                Console.Write("What is the name of column {0}? ", i + 1);
                columnNames[i] = Console.ReadLine()!;
            }

            Console.WriteLine("");

            Console.WriteLine("Time to assign row names.");
            for (int i = 0; i < Dimension; i++)
            {
                Console.Write("What is the name of row {0}? ", i + 1);
                rowNames[i] = Console.ReadLine()!;
            }

            Console.WriteLine("");

            Console.WriteLine("Time to assign the values in the 2D array or matrix.");
            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    Console.Write("What value should be in coordinates[{0}][{1}] or in row[{0}] and column[{1}] ? ", y, x);
                    int value = int.Parse((Console.ReadLine()!).Trim());
                    theMatrix[y][x] = value;
                }
            }

            Console.WriteLine("\nNow Calculating... \n");
            await Task.Delay(1000);
            

            await MethodAsync((rowNames).ToList(), (columnNames).ToList(), theMatrix);
        }

        static void DeductAll<T>(T[][] matrix, Dictionary<string, int> zeroDict, int yLoc, int xLoc)
        {
            // Decrement by row
            for (int y = 0; y < matrix.Length; y++)
            {
                string currentLocation = string.Format("{0},{1}", y, xLoc);
                string rowLocation = string.Format("Row {0}", y);

                if (zeroDict[rowLocation] == 1) continue;

                if (currentLocation == string.Format("{0},{1}", yLoc, xLoc))
                {
                    zeroDict[rowLocation] = 1;
                    continue;
                }

                if (Convert.ToInt16(matrix[y][xLoc]) != 0) continue;

                zeroDict[rowLocation] -= 1;
            }

            // Decrement by column
            for (int x = 0; x < matrix.Length; x++)
            {
                string currentLocation = string.Format("{0},{1}", yLoc, x);
                string columnLocation = string.Format("Column {0}", x);

                if (zeroDict[columnLocation] == 1) continue;

                if (currentLocation == string.Format("{0},{1}", yLoc, xLoc))
                {
                    zeroDict[columnLocation] = 1;
                    continue;
                }

                if (Convert.ToInt16(matrix[yLoc][x]) != 0) continue;

                zeroDict[columnLocation] -= 1;
            }
        }

        static Dictionary<string, int> DeductAll(Dictionary<string, int> zeroAndLocation, string exactLocation, int[][] matrix)
        {
            Dictionary<string, int> zeroAndLocations = zeroAndLocation;
            string[] location = exactLocation.Split(',');

            for (int y = 0; y < matrix.Length; y++)
            {
                if(y == Convert.ToInt16(location[0]))
                {
                    for(int x = 0; x < matrix[y].Length; x++)
                    {
                        if (matrix[y][x] == 0)
                        {
                            for (int i = 0; i < matrix.Length; i++)
                            {
                                if (i == Convert.ToInt16(location[0])) continue;
                                if (matrix[i][x] == 0)
                                {
                                    zeroAndLocation[string.Format("Row {0}", i)] -= 1;
                                }
                            }
                            break;
                        }
                        //break;
                    }
                    break;
                }
            }

            return zeroAndLocation;
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
