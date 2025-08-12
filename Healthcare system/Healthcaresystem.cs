namespace HealthcareSystem
{
    // Generic repository for entity management
    public class Repository<T>
    {
        private readonly List<T> items = new();

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            items.Add(item);
        }

        public List<T> GetAll()
        {
            // Return a shallow copy to avoid external modification of internal list
            return new List<T>(items);
        }

        public T? GetById(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return items.FirstOrDefault(predicate);
        }

        public bool Remove(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var item = items.FirstOrDefault(predicate);
            if (item == null) return false;
            return items.Remove(item);
        }
    }

    // Patient entity
    public class Patient
    {
        public int Id { get; }
        public string Name { get; }
        public int Age { get; }
        public string Gender { get; }

        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Age = age;
            Gender = gender ?? throw new ArgumentNullException(nameof(gender));
        }

        public override string ToString() => $"Patient(Id={Id}, Name={Name}, Age={Age}, Gender={Gender})";
    }

    // Prescription entity
    public class Prescription
    {
        public int Id { get; }
        public int PatientId { get; }
        public string MedicationName { get; }
        public DateTime DateIssued { get; }

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName ?? throw new ArgumentNullException(nameof(medicationName));
            DateIssued = dateIssued;
        }

        public override string ToString() => $"Prescription(Id={Id}, PatientId={PatientId}, Medication='{MedicationName}', Date={DateIssued:d})";
    }

    // Main application class
    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo = new();
        private readonly Repository<Prescription> _prescriptionRepo = new();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new();

        // Add sample patients and prescriptions
        public void SeedData()
        {
            // Seed patients (2-3)
            _patientRepo.Add(new Patient(1, "Ada Mensah", 29, "Female"));
            _patientRepo.Add(new Patient(2, "Kofi Owusu", 42, "Male"));
            _patientRepo.Add(new Patient(3, "Esi Tetteh", 34, "Female"));

            // Seed prescriptions (4-5) with valid PatientIds
            _prescriptionRepo.Add(new Prescription(101, 1, "Amoxicillin 500mg", DateTime.Today.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(102, 1, "Paracetamol 500mg", DateTime.Today.AddDays(-5)));
            _prescriptionRepo.Add(new Prescription(103, 2, "Metformin 850mg", DateTime.Today.AddDays(-30)));
            _prescriptionRepo.Add(new Prescription(104, 3, "Atorvastatin 20mg", DateTime.Today.AddDays(-2)));
            _prescriptionRepo.Add(new Prescription(105, 2, "Lisinopril 10mg", DateTime.Today.AddDays(-1)));
        }

        // Build the dictionary map: PatientId => List<Prescription>
        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();

            var allPrescriptions = _prescriptionRepo.GetAll();
            foreach (var p in allPrescriptions)
            {
                if (!_prescriptionMap.TryGetValue(p.PatientId, out var list))
                {
                    list = new List<Prescription>();
                    _prescriptionMap[p.PatientId] = list;
                }
                list.Add(p);
            }
        }

        // Print all patients
        public void PrintAllPatients()
        {
            var patients = _patientRepo.GetAll();
            Console.WriteLine("---- All Patients ----");
            if (!patients.Any())
            {
                Console.WriteLine("(no patients)");
                return;
            }

            foreach (var patient in patients)
            {
                Console.WriteLine(patient);
            }
            Console.WriteLine();
        }

        // Print prescriptions for a specified patient id (uses the map)
        public void PrintPrescriptionsForPatient(int patientId)
        {
            Console.WriteLine($"---- Prescriptions for PatientId={patientId} ----");
            if (!_prescriptionMap.TryGetValue(patientId, out var prescriptions) || prescriptions.Count == 0)
            {
                Console.WriteLine("No prescriptions found for this patient.\n");
                return;
            }

            foreach (var pres in prescriptions)
            {
                Console.WriteLine(pres);
            }
            Console.WriteLine();
        }

        // Helper: demonstrate GetById and Remove operations on repositories (optional)
        public void DemoRepositoryOperations()
        {
            // Example: find patient by id
            var p = _patientRepo.GetById(x => x.Id == 2);
            Console.WriteLine("Found by GetById(2): " + (p ?? (object)"(not found)"));

            // Remove a prescription by predicate
            var removed = _prescriptionRepo.Remove(px => px.Id == 105);
            Console.WriteLine($"Attempted to remove prescription 105: {(removed ? "removed" : "not found")}");

            Console.WriteLine();
        }
    }

    // Program entry point
    public static class Program
    {
        public static void Main()
        {
            var app = new HealthSystemApp();

            // Main flow requested:
            // i. Instantiate HealthSystemApp -> done above
            // ii. Call SeedData()
            app.SeedData();

            // iii. Call BuildPrescriptionMap()
            app.BuildPrescriptionMap();

            // iv. Print all patients
            app.PrintAllPatients();

            // v. Select one PatientId and display prescriptions
            // We'll pick patient id = 2 (Kofi Owusu) as an example
            int selectedPatientId = 2;
            app.PrintPrescriptionsForPatient(selectedPatientId);

            // Optional demo of repository ops
            app.DemoRepositoryOperations();

            // Show prescriptions for id 2 again (note: we removed prescription 105 above so map would be stale;
            // to reflect repository changes you'd call BuildPrescriptionMap() again)
            app.BuildPrescriptionMap();
            app.PrintPrescriptionsForPatient(selectedPatientId);
        }
    }
}