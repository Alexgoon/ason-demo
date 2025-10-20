using Ason;
using AsonDemo.State;
using AsonDemo.Model;
using AsonDemo.Components.Pages;

namespace AsonDemo.Ason;

[AsonOperator]
public class BlazorMainAppOperator : RootOperator<SessionState> {
    public BlazorMainAppOperator(SessionState associated) : base(associated) { }

    [AsonMethod]
    public Task<EmployeesOperator> GetEmployeesOperatorAsync()
        => GetViewOperator<EmployeesOperator>(() => { AttachedObject.Nav.NavigateTo("/employees"); });

    [AsonMethod]
    public Task<EmailsOperator> GetEmailsOperatorAsync()
        => GetViewOperator<EmailsOperator>(() => { AttachedObject.Nav.NavigateTo("/emails"); });

    [AsonMethod("Gets charts operator. Call this when creating/drawing charts")]
    public Task<ChartsOperator> GetChartsOperatorAsync()
        => GetViewOperator<ChartsOperator>(() => { AttachedObject.Nav.NavigateTo("/charts"); });
}

[AsonOperator]
public class EmployeesOperator : OperatorBase<Employees> {
    [AsonMethod]
    public List<Employee> GetEmployees()
        => AttachedObject?.EmployeesSnapshot!;
    [AsonMethod]
    public void AddEmployee(Employee employee)
        => AttachedObject?.AddEmployee(employee);
    [AsonMethod]
    public void DeleteEmployee(int employeeId)
        => AttachedObject?.DeleteEmployee(employeeId);

    [AsonMethod("Opens Employee editing tab to update employee data")]
    public async Task<EmployeeEditViewOperator> GetEmployeeEditingOperatorAsync(int employeeId) =>
        await GetViewOperator<EmployeeEditViewOperator>(() => AttachedObject?.OpenEditor(employeeId), employeeId.ToString());

}

[AsonOperator]
public class EmployeeEditViewOperator : OperatorBase<EmployeeEditModel> {
    [AsonMethod("Use this method to update any data of the Employee class or its Sales")]
    public void UpdateEmployee(Employee updatedEmployee) =>
        AttachedObject?.ReplaceEditable(updatedEmployee);
}

[AsonOperator]
public class EmailsOperator : OperatorBase<Emails> {
    [AsonMethod("Returns a list of emails")]
    public List<MailItem> GetEmails() => AttachedObject!.EmailsSnapshot.OrderByDescending(e => e.ReceivedDate).ToList();

}

[AsonOperator("Chart operations. Choose the most appropriate chart type based on the user task.")]
public class ChartsOperator : OperatorBase<Charts> {
    [AsonMethod("Adds bar chart and assigns data displayed in chart. Use a bar chart when you need to compare values across discrete categories or highlight differences in magnitude.")]
    public void CreateBarChart(BarValue[] barValues, string xAxisCaption, string yAxisCaption, string shortTitle) =>
        AttachedObject?.AddBarChart(barValues, xAxisCaption, yAxisCaption, shortTitle);

    [AsonMethod("Adds line chart and assigns data displayed in chart. Use a line chart when you need to show trends or changes in values over time or continuous data.")]
    public void CreateLineChart(LineValue[] lineValues, string xAxisCaption, string yAxisCaption, string shortTitle) =>
        AttachedObject?.AddLineChart(lineValues, xAxisCaption, yAxisCaption, shortTitle);

    [AsonMethod("Adds pie chart and assigns data displayed in chart. Use a pie chart when you need to show how parts contribute to a whole, and the number of categories is small and clearly distinguishable.")]
    public void CreatePieChart(PieValue[] pieValues, string shortTitle) =>
        AttachedObject?.AddPieChart(pieValues, shortTitle);
}

[AsonModel]
public class BarValue {
    public string Caption { get; set; } = string.Empty;
    public object Value { get; set; } = new();
}

[AsonModel]
public class LineValue {
    public object xValue { get; set; } = new();
    public object yValue { get; set; } = new();
}

[AsonModel]
public class PieValue {
    public string Category { get; set; } = string.Empty;
    public object Value { get; set; } = new();
}

