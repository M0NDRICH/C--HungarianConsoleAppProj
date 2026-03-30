/*Final Project*/


using FinalProject_QuantitativeMethods;

await Hungarian.DisplayTitle();

#region -- Sample Data --
List<string> names = new (){ "Alice", "Bob", "Charlie" };
List<string> servers = new() { "Database", "Email", "Firewall" };
int[][] matrix = { new[] { 8, 6, 2 }, new[] { 6, 7, 11 }, new[] {3, 5, 7 } };

List<string> names2 = new() { "Crane1", "Crane2", "Crane3", "Crane4" };
List<string> jobs = new() { "Job1", "Job2", "Job3", "Job4" };
int[][] matrix2 = { new[] { 4, 2, 5, 7 }, new[] { 8, 3, 10, 8 }, new[] { 12, 5, 4, 5 }, new[] { 6, 3, 7, 14 } };


List<string> names3 = new() { "1", "2", "3", "4", "5" };
List<string> machines = new() { "A", "B", "C", "D", "E" };
int[][] matrix3 = { new[] { 13, 8, 16, 18, 19 }, new[] { 9, 15, 24, 9, 12 }, new[] { 12, 9, 4, 4, 4 }, new[] { 6, 12, 10, 8, 13 }, new[] { 15, 17, 18, 12, 20 } };
#endregion

await Hungarian.Simulate();
//await Hungarian.MethodAsync(names3, machines, matrix3);
//await Hungarian.MethodAsync(names, servers, matrix);
