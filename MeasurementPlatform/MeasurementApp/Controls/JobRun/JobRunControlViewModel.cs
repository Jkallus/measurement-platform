using MeasurementApp.Services;
using MeasurementApp.Services.RecipeSelect;
using MeasurementApp.BusinessLogic.Recipe;
using MeasurementApp.BusinessLogic.SystemControl;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeasurementApp.Controls.JobRun;

public class JobRunControlViewModel: ObservableObject
{
    // Private members
    private readonly IServiceProvider _service;
    private readonly ILogger _logger;
    private readonly RecipeSelectService _recipeSelect;
    private readonly JobRunner _jobRunner;
    private CancellationTokenSource _tokenSource;

    // Public properties
    private ScanRecipe? _recipe;
    public ScanRecipe? Recipe
    {
        get => _recipe;
        set
        {
            if(SetProperty(ref _recipe, value))
            {
                RunJobCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IAsyncRelayCommand SelectRecipeCommand { get; private set; }
    public IAsyncRelayCommand RunJobCommand { get; private set; }
    public RelayCommand StopJobCommand { get; private set; }

    public bool IsRunning => _jobRunner.IsRunning;


    private string _progressText;
    public string ProgressText
    {
        get => _progressText;
        set => SetProperty(ref _progressText, value);
    }
    private double _progress;
    public double Progress
    {
        get => _progress;
        set
        {
            if(SetProperty(ref _progress, value))
            {
                ProgressText = _progress.ToString("0.00") + "%";
            }
        }
    }

    public JobRunControlViewModel(IServiceProvider service, ILogger<JobRunControlViewModel> logger)
    {
        _service = service;
        _logger = logger;
        _recipe = null;
        _progress = 0;
        _progressText = "N/A";
        _recipeSelect = _service.GetService(typeof(RecipeSelectService)) as RecipeSelectService ?? throw new Exception("Recipe select service is null");
        _jobRunner = _service.GetService(typeof(JobRunner)) as JobRunner ?? throw new Exception ("JobRunner is null");
        SelectRecipeCommand = new AsyncRelayCommand(SelectRecipe);
        RunJobCommand = new AsyncRelayCommand(RunJob, CanRunJob);
        StopJobCommand = new RelayCommand(StopJob, CanStopJob);
        _tokenSource = new CancellationTokenSource();
    }

    private bool CanStopJob()
    {
        return IsRunning;
    }

    private void StopJob()
    {
        _tokenSource.Cancel();
    }

    private async Task RunJob()
    {
        try
        {
            _jobRunner.Job = new Job(Recipe!);
            _tokenSource = new CancellationTokenSource();
            var t =  _jobRunner.ExecuteJob(_tokenSource.Token, new Progress<double>(ProgressHandler));
            StopJobCommand.NotifyCanExecuteChanged();
            RunJobCommand.NotifyCanExecuteChanged();
            await t;
            Progress = 100.0;
        }
        catch(OperationCanceledException ex)
        {
            await App.MainRoot!.MessageDialogAsync("Cancel Success", "The job was successfully cancelled");
            _logger.LogInformation(ex.Message);
            Progress = 0.0;
            ProgressText = "N/A";
            StopJobCommand.NotifyCanExecuteChanged();
            RunJobCommand.NotifyCanExecuteChanged();
        }
        catch(Exception ex)
        {
            await App.MainRoot!.MessageDialogAsync("Exception", ex.Message);
        }
        finally
        {
            
            StopJobCommand.NotifyCanExecuteChanged();
            RunJobCommand.NotifyCanExecuteChanged();
        }
        
    }

    

    // Private methods
    private async Task SelectRecipe()
    {
        Recipe = await _recipeSelect.SelectRecipe() ?? Recipe;
    }

    private bool CanRunJob()
    {
        return Recipe != null && !IsRunning;
    }

    private void ProgressHandler(double percent)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            Progress = percent;
        });
    }
}
