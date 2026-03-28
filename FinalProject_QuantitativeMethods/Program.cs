/*Final Project*/


using FinalProject_QuantitativeMethods;

List<string> names = new (){ "Alice", "Bob", "Charlie" };
List<string> servers = new() { "Database", "Email", "Firewall" };
int[][] matrix = { new[] { 8, 6, 2 }, new[] { 6, 7, 11 }, new[] {3, 5, 7 } };

List<string> names2 = new() { "Crane1", "Crane2", "Crane3", "Crane4" };
List<string> jobs = new() { "Job1", "Job2", "Job3", "Job4" };
int[][] matrix2 = { new[] { 4, 2, 5, 7 }, new[] { 8, 3, 10, 8 }, new[] { 12, 5, 4, 5 }, new[] { 6, 3, 7, 14 } };

//await Hungarian.MethodAsync(names, servers, matrix);
await Hungarian.MethodAsync(names2, jobs, matrix2);
//var result = Hungarian.SubtractionValuesInRow(matrix, (n) => n - 2);

//int[][] result = Hungarian.Step1(matrix);
