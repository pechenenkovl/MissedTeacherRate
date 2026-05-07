using MissedTeacherRate.Algorithms.COMAS;
using MissedTeacherRate.Models;

var marksMatrix = new MarksMatrix();

// Work
var d1 = new Work("D1");
var d2 = new Work("D2");
var d3 = new Work("D3");
var d4 = new Work("D4");
marksMatrix.AddWorks(d1, d2, d3, d4);

// Teacher and students
var teacher = new Teacher("P");
var student1 = new Student("S1");
var student2 = new Student("S2");
var student3 = new Student("S3");
var student4 = new Student("S4");
marksMatrix.SetTeacher(teacher);
marksMatrix.AddStudents(student1, student2, student3, student4);

// Add marks
// Teacher
marksMatrix.AddRating(teacher, d2, 8);

// Student1
marksMatrix.AddRating(student1, d1, 8);
marksMatrix.AddRating(student1, d3, 6);
marksMatrix.AddRating(student1, d4, 5);

// Student2
marksMatrix.AddRating(student2, d1, 5);
marksMatrix.AddRating(student2, d3, 8);

// Student3
marksMatrix.AddRating(student3, d2, 8);
marksMatrix.AddRating(student3, d4, 9);

// Student 4
marksMatrix.AddRating(student4, d4, 6);

// Marks before calculation
Console.WriteLine("Init rate");
marksMatrix.PrintMatrix();

// Comas algorithm calculation
var comasAlgorithmCalculator = new ComasAlgorithmCalculator();
var newMatrix = comasAlgorithmCalculator.CalculateMissedMarks(marksMatrix);

Console.WriteLine();
Console.WriteLine("Rate missed marks for teacher after COMAS");
// Marks after calculation
newMatrix.PrintMatrix();


Console.Read();