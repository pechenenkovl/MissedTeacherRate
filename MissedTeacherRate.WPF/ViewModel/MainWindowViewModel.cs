
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MissedTeacherRate.Models;
using MissedTeacherRate.Algorithms.COMAS;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using MissedTeacherRate.Parsers.MissedTeacherRate.Parsers;
using MissedTeacherRate.Algorithms;

namespace MissedTeacherRate.WPF.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private string? _excelFilePath;
    private MarksMatrix? _sourceMatrix;
    private MarksMatrix? _resultMatrix;

    private DataView? _sourceMatrixView;
    public DataView? SourceMatrixView
    {
        get => _sourceMatrixView;
        set => SetProperty(ref _sourceMatrixView, value);
    }

    private DataView? _resultMatrixView;
    public DataView? ResultMatrixView
    {
        get => _resultMatrixView;
        set => SetProperty(ref _resultMatrixView, value);
    }

    public ObservableCollection<IMissedMarksCalculator> Algorithms { get; } = new();
    [ObservableProperty] 
    private IMissedMarksCalculator? selectedAlgorithm;

    public MainWindowViewModel()
    {
        // Use reflection to find all non-abstract, public classes implementing IMissedMarksCalculator
        var calculatorTypes = typeof(IMissedMarksCalculator).Assembly
            .GetTypes()
            .Where(t => typeof(IMissedMarksCalculator).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in calculatorTypes)
        {
            if (Activator.CreateInstance(type) is IMissedMarksCalculator instance)
                Algorithms.Add(instance);
        }

        SelectedAlgorithm = Algorithms.FirstOrDefault();
    }

    [RelayCommand]
    private void OpenExcel()
    {
        var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx", DefaultDirectory = Environment.CurrentDirectory };
        if (dlg.ShowDialog() == true)
        {
            _excelFilePath = dlg.FileName;
            _sourceMatrix = null;
            _resultMatrix = null;
            ResultMatrixView = new DataView();
            Parse();
        }
    }

    private void Parse()
    {
        if (_excelFilePath == null) return;
        using var stream = File.OpenRead(_excelFilePath);
        var parser = new ExcelMatrixParser();
        _sourceMatrix = parser.Parse(stream);
        SourceMatrixView = ToDataTable(_sourceMatrix).DefaultView;
        OnPropertyChanged(nameof(CanCalculate));
        CalculateCommand.NotifyCanExecuteChanged();
    }

    public bool CanCalculate => _sourceMatrix != null && SelectedAlgorithm != null;

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (_sourceMatrix == null || SelectedAlgorithm == null) return;
        _resultMatrix = SelectedAlgorithm.CalculateMissedMarks(_sourceMatrix);
        ResultMatrixView = ToDataTable(_resultMatrix).DefaultView;
    }

    // Helper: Convert MarksMatrix to DataTable for DataGrid
    private static DataTable ToDataTable(MarksMatrix matrix)
    {
        var dt = new DataTable();
        var raters = matrix.GetRaters().ToList();
        dt.Columns.Add("Work");
        foreach (var rater in raters)
            dt.Columns.Add(rater.Id);

        foreach (var work in matrix.GetWorks())
        {
            var row = dt.NewRow();
            row["Work"] = work.Id;
            foreach (var rater in raters)
            {
                var mark = matrix.GetMarkForWork(rater, work);
                row[rater.Id] = mark?.ToString("0.#") ?? "";
            }
            dt.Rows.Add(row);
        }
        return dt;
    }
}
