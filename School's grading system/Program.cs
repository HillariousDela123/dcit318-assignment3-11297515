
namespace SchoolsGradingSystem
{
    // Custom Exception: Invalid Score Format
    public class InvalidScoreFormatException : Exception
    {
        public InvalidScoreFormatException(string message) : base(message) { }
    }

    // Custom Exception: Missing Field
    public class MissingFieldException : Exception
    {
        public MissingFieldException(string message) : base(message) { }
    }

    // Student Class
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Score { get; set; }

        public string GetGrade()
        {
            if (Score >= 80 && Score <= 100) return "A";
            else if (Score >= 70) return "B";
            else if (Score >= 60) return "C";
            else if (Score >= 50) return "D";
            else return "F";
        }
    }

    // StudentResultProcessor Class
    public class StudentResultProcessor
    {
        public List<Student> ReadStudentsFromFile(string inputFilePath)
        {
            List<Student> students = new List<Student>();

            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');

                    if (parts.Length < 3)
                        throw new MissingFieldException($"Missing data in line: {line}");

                    if (!int.TryParse(parts[0], out int id))
                        throw new FormatException($"Invalid ID format in line: {line}");

                    string fullName = parts[1].Trim();

                    if (!int.TryParse(parts[2], out int score))
                        throw new InvalidScoreFormatException($"Invalid score format in line: {line}");

                    students.Add(new Student { Id = id, FullName = fullName, Score = score });
                }
            }

            return students;
        }

        public void WriteReportToFile(List<Student> students, string outputFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (var student in students)
                {
                    writer.WriteLine($"{student.FullName} (ID: {student.Id}): Score = {student.Score}, Grade = {student.GetGrade()}");
                }
            }
        }
    }

    // Program Entry Point
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputPath = "students.txt";
            string outputPath = "report.txt";

            // Auto-create students.txt if it doesn't exist
            if (!File.Exists(inputPath))
            {
                using (StreamWriter sw = new StreamWriter(inputPath))
                {
                    sw.WriteLine("1, John Doe, 85");
                    sw.WriteLine("2, Jane Smith, 73");
                    sw.WriteLine("3, David Johnson, 59");
                    sw.WriteLine("4, Emily Brown, 47");
                    sw.WriteLine("5, Michael White, 90");
                }
                Console.WriteLine($"'{inputPath}' created with sample data.");
            }

            StudentResultProcessor processor = new StudentResultProcessor();

            try
            {
                var students = processor.ReadStudentsFromFile(inputPath);
                processor.WriteReportToFile(students, outputPath);

                Console.WriteLine("Report generated successfully!");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File error: {ex.Message}");
            }
            catch (InvalidScoreFormatException ex)
            {
                Console.WriteLine($"Score format error: {ex.Message}");
            }
            catch (MissingFieldException ex)
            {
                Console.WriteLine($"Missing field error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}