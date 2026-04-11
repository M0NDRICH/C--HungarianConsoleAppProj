/*Final Project*/


using FinalProject_QuantitativeMethods;

await Hungarian.DisplayTitle();

#region -- Sample Data --
List<string> names = new (){ "Alice", "Bob", "Charlie" };
List<string> servers = new() { "Database", "Email", "Firewall" };
int[][] matrix = 
{ 
    new[] { 8, 6, 2 },
    new[] { 6, 7, 11 },
    new[] { 3, 5, 7 }
};

List<string> names2 = new() { "Crane1", "Crane2", "Crane3", "Crane4" };
List<string> jobs = new() { "Job1", "Job2", "Job3", "Job4" };
int[][] matrix2 = 
{
    new[] { 4, 2, 5, 7 },
    new[] { 8, 3, 10, 8 },
    new[] { 12, 5, 4, 5 },
    new[] { 6, 3, 7, 14 }
};


List<string> names3 = new() { "1", "2", "3", "4", "5" };
List<string> machines = new() { "A", "B", "C", "D", "E" };
int[][] matrix3 = 
{ 
    new[] { 13, 8, 16, 18, 19 },
    new[] { 9, 15, 24, 9, 12 },
    new[] { 12, 9, 4, 4, 4 },
    new[] { 6, 12, 10, 8, 13 },
    new[] { 15, 17, 18, 12, 20 }
};

//List<string> jobs = new() { "Job1", "Job2", "Job3", "Job4" };
List<string> printers = new() { "Printer1", "Printer2", "Printer3", "Printer4" };
int[][] matrix4 = 
{ 
    new[] { 6, 5, 17, 7 },
    new[] { 12, 8, 4, 3 },
    new[] { 7, 6, 9, 10 },
    new[] { 6, 3, 7, 8 }
};

List<string> printers1 = new() { "Printer 1", "Printer 2", "Printer 3", "Printer 4"};
List<string> books = new List<string>() { "Book 1", "Book 2", "Book 3", "Book 4" };
int[][] valMatrix = 
{ 
    new[] { 20, 19, 11, 5 },
    new[] { 7, 15, 16, 9 },
    new[] { 5, 10, 19, 7 },
    new[] { 21, 6, 7, 9 } 
};

List<string> J1 = new List<string>() { "Job 1", "Job 2", "Job 3", "Job 4" };
List<string> n1 = new List<string>() { "Mike", "John", "Joanne", "Hannah" };
int[][] m1 = new[]
{
    new[] { 90, 75, 75, 80 },
    new[] { 35, 85, 55, 65 },
    new[] { 125, 95, 90, 105 },
    new[] { 45, 110, 95, 115 }
};

#endregion

await Hungarian.Simulate();
//await Hungarian.MethodAsync(names3, machines, matrix3);
//await Hungarian.MethodAsync(names, servers, matrix);
//await Hungarian.MethodAsync(printers, jobs, matrix4);
//await Hungarian.MethodAsync(printers1, books, valMatrix);
//await Hungarian.MethodAsync(J1, n1, m1);