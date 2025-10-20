using System.Collections.ObjectModel;
using Ason;
using AsonDemo.Model;
using AsonDemo.Services;
using AsonDemo.Ason;
using Microsoft.AspNetCore.Components;

namespace AsonDemo.State;

public class SessionState {
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Appointment> Appointments { get; } = new();
    public ObservableCollection<MailItem> Emails { get; } = new();

    public List<string> ChartLabels { get; } = new();
    public List<double> ChartValues { get; } = new();
    public string? ChartDescription { get; set; }

    bool _seeded;
    public RootOperator MainAppOperator { get; }
    public NavigationManager Nav { get; }
    readonly IAppDataService _dataService;

    public SessionState(NavigationManager nav, IAppDataService dataService) {
        Nav = nav; _dataService = dataService; MainAppOperator = new BlazorMainAppOperator(this);
    }

    public async Task EnsureSeededAsync() {
        if (_seeded) return; _seeded = true;
        foreach (var e in await _dataService.GetEmployeesAsync()) Employees.Add(e);
        foreach (var m in await _dataService.GetMailItemsAsync()) Emails.Add(m);
        foreach (var a in await _dataService.GetAppointmentsAsync()) Appointments.Add(a);
    }
}
