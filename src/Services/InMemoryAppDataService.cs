using AsonDemo.Model;

namespace AsonDemo.Services;

public interface IAppDataService {
    Task<List<Employee>> GetEmployeesAsync();
    Task<List<MailItem>> GetMailItemsAsync();
    Task<List<Appointment>> GetAppointmentsAsync();
}

public sealed class InMemoryAppDataService : IAppDataService {
    readonly List<Employee> _employees = new();
    readonly List<MailItem> _mailItems = new();
    readonly List<Appointment> _appointments = new();
    readonly string[] _productNames = ["CRM Subscription", "Implementation Package", "Training Session", "Premium Support", "Analytics Add-on"];
    bool _seeded;
    readonly object _lock = new();

    void EnsureSeeded() {
        if (_seeded) return;
        lock (_lock) { if (_seeded) return; Seed(); _seeded = true; }
    }

    void Seed() {
        var random = new Random(42);
        string[] firstNames = ["Olivia", "Noah", "Emma", "Oliver", "Ava", "Elijah", "Bobby", "Sophia", "Mateo", "Isabella", "James", "Mia", "Benjamin", "Charlotte", "Lucas", "Amelia", "Henry", "Harper", "Alexander", "Evelyn", "Michael", "Abigail", "Ethan", "Emily", "Daniel", "Elizabeth", "Jackson", "Sofia", "Sebastian", "Avery"];
        string[] lastNames = ["Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Star", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"];
        string[] positions = ["Account Executive", "Sales Representative", "Customer Success Manager", "Marketing Coordinator", "Support Specialist", "Business Development Rep", "Sales Manager", "Implementation Consultant", "Solutions Engineer", "Inside Sales Rep"];

        var today = DateTime.Today;
        var rangeStart = new DateTime(today.Year, today.Month, 1).AddYears(-2); 
        int monthsTotal = ((today.Year - rangeStart.Year) * 12) + today.Month - rangeStart.Month + 1; 
        double baseLambda = 0.9;
        int saleId = 1; 

        for (int i = 0; i < 30; i++) {
            var first = firstNames[i % firstNames.Length];
            var last = lastNames[i % lastNames.Length];
            var position = positions[i % positions.Length];
            var hireDate = DateTime.Today.AddDays(-random.Next(1, 5 * 365));
            var emailDomain = "contoso-crm.com";
            var employee = new Employee {
                Id = i + 1,
                FirstName = first,
                LastName = last,
                Email = $"{first.ToLower()}.{last.ToLower()}@{emailDomain}",
                Position = position,
                HireDate = hireDate,
                Sales = new List<Sale>()
            };

            int monthIndex = 0;
            for (var monthStart = hireDate; monthStart <= today; monthStart = monthStart.AddMonths(1), monthIndex++) {
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                if (employee.HireDate > monthEnd) continue;

                double progress = monthsTotal > 1 ? (double)monthIndex / (monthsTotal - 1) : 1;
                double lambda = baseLambda * (0.3 + 0.7 * progress);
                int monthSales = SamplePoisson(lambda, random);

                for (int s = 0; s < monthSales; s++) {
                    int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
                    var day = random.Next(1, daysInMonth + 1);
                    var saleDate = new DateTime(monthStart.Year, monthStart.Month, day)
                        .AddHours(random.Next(0, 24))
                        .AddMinutes(random.Next(0, 60));
                    if (saleDate < employee.HireDate) continue; // ensure not before hire

                    employee.Sales.Add(new Sale {
                        Id = saleId++,
                        ProductName = _productNames[random.Next(_productNames.Length)],
                        Quantity = random.Next(1, 10),
                        Price = Math.Round((decimal)(random.NextDouble() * 900 + 100), 2),
                        SaleDate = saleDate
                    });
                }
            }

            _employees.Add(employee);
        }

        var customerDomains = new[] { "acme.com", "ason.com", "initech.com", "umbrella-corp.com", "soylent.com", "starkindustries.com", "wonka.com", "wayneenterprises.com", "example.com" };
        string[] subjects = ["Request for product demo", "Follow-up on pricing proposal", "Question about implementation timeline", "License renewal inquiry", "Issue with analytics dashboard", "Upcoming contract negotiation", "Request for additional user seats", "Feedback after training session", "Need clarification on invoice", "Feature request: multi-region support", "Welcome Our New Team Member"];
        string[] bodies = [
            "Hi team, we would like to schedule a full product demo for our regional managers next week.",
            "Just following up on the pricing proposal you sent. Can we discuss discount options?",
            "What is the typical timeline from contract signature to full implementation?",
            "Our license is expiring soon. Can you send renewal documents?",
            "We are seeing slower load times in the analytics dashboard—any known issues?",
            "Our leadership wants to revisit contract terms for the upcoming fiscal year.",
            "We need 25 additional user seats starting next month. Please advise on process.",
            "Great training yesterday! Could you share the recording and materials?",
            "Invoice #3491 has a different total than expected—can you clarify the adjustments?",
            "We operate in EU + APAC. Is multi-region data residency on your roadmap?",
            "Hi team! Let me introduce our new employee: Michael Johnson. He will join our team as a Sales Representative"
        ];
        for (int i = 0; i < subjects.Length; i++) {
            var from = $"contact@{customerDomains[i % customerDomains.Length]}";
            var toCount = 1 + (i % 3);
            var toList = _employees.OrderBy(e => (e.Id + i) % 7).Take(toCount).Select(e => e.Email).ToList();
            _mailItems.Add(new MailItem { Subject = subjects[i], Body = bodies[i], From = from, To = toList, ReceivedDate = DateTime.Now.AddHours((i - subjects.Length) * 6) });
        }
        string[] apptTitles = ["Discovery Call - ACME", "Product Demo - Globex", "Implementation Kickoff", "Renewal Strategy Meeting", "Quarterly Business Review", "Sales Pipeline Review", "Onboarding Session", "Support Escalation Review", "Pricing Negotiation", "Marketing Alignment Meeting"];
        for (int i = 0; i < 20; i++) {
            var start = DateTime.Today.AddHours(-i * 6);
            var durationHours = (i % 3) + 1;
            var title = apptTitles[i % apptTitles.Length];
            _appointments.Add(new Appointment { Id = i + 1, Title = title, Description = $"Auto-generated appointment: {title}. Includes stakeholders and agenda items.", StartTime = start, EndTime = start.AddHours(durationHours) });
        }
    }

    static int SamplePoisson(double lambda, Random random) {
        if (lambda <= 0) return 0;
        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;
        do { k++; p *= random.NextDouble(); } while (p > L);
        return k - 1;
    }

    public Task<List<Employee>> GetEmployeesAsync() {
        EnsureSeeded();
        return SimulateLatency(_employees);
    }
    public Task<List<MailItem>> GetMailItemsAsync() {
        EnsureSeeded();
        return SimulateLatency(_mailItems);
    }
    public Task<List<Appointment>> GetAppointmentsAsync() {
        EnsureSeeded();
        return SimulateLatency(_appointments);
    }

    static async Task<List<T>> SimulateLatency<T>(List<T> source) {
        await Task.Delay(0);
        return source.ToList();
    }
}
