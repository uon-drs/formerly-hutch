using HutchAgent.Config;
using Microsoft.Extensions.Options;
using Minio.Exceptions;

namespace HutchAgent.Services;

public class WatchFolderService : BackgroundService
{
  private readonly WatchFolderOptions _options;
  private readonly ILogger<WatchFolderService> _logger;
  private MinioService? _minioService;
  private WfexsJobService? _wfexsJobService;
  private CrateMergerService? _crateMergerService;
  private readonly IServiceProvider _serviceProvider;

  public WatchFolderService(IOptions<WatchFolderOptions> options, ILogger<WatchFolderService> logger,
    IServiceProvider serviceProvider)
  {
    _options = options.Value;
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  /// <summary>
  /// Watch a folder for results of WfExS runs and upload to an S3 bucket.
  /// </summary>
  /// <param name="stoppingToken"></param>
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation($"Starting to watch folder {_options.Path}");

    while (!stoppingToken.IsCancellationRequested)
    {
      using (var scope = _serviceProvider.CreateScope())
      {
        _minioService = scope.ServiceProvider.GetService<MinioService>() ?? throw new InvalidOperationException();
        _wfexsJobService = scope.ServiceProvider.GetService<WfexsJobService>() ?? throw new InvalidOperationException();
        _crateMergerService = scope.ServiceProvider.GetService<CrateMergerService>() ??
                              throw new InvalidOperationException();
        _watchFolder();
        MergeResults();
      }

      await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
    }
  }

  /// <summary>
  /// Stop watching the results folder for WfExS execution results.
  /// </summary>
  /// <param name="stoppingToken"></param>
  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation($"Stopping watching folder {_options.Path}");

    await base.StopAsync(stoppingToken);
  }

  private async void _watchFolder()
  {
    foreach (var file in Directory.EnumerateFiles(_options.Path))
    {
      if (await _minioService.FileExistsInBucket(Path.GetFileName(file)))
      {
        _logger.LogInformation($"{Path.GetFileName(file)} already exists in S3.");
        continue;
      }

      try
      {
        _logger.LogInformation($"Attempting to upload {file} to S3.");
        await _minioService.UploadToBucket(file);
        _logger.LogInformation($"Successfully uploaded {file} to S3.");
      }
      catch (BucketNotFoundException e)
      {
        _logger.LogCritical($"Unable to upload {file} to S3. The configured bucket does not exist.");
      }
      catch (MinioException e)
      {
        _logger.LogError($"Unable to upload {file} to S3. An error occurred with the S3 server.");
      }
    }
  }

  private async void MergeResults()
  {
    var finishedJobs = await _wfexsJobService.ListFinishedJobs();
    foreach (var job in finishedJobs)
    {
      var sourceZip = Path.Combine(_options.Path, $"{job.WfexsRunId}.zip");
      var pathToMetadata = Path.Combine(job.UnpackedPath, "ro-crate-metadata.json");
      var mergeDirInfo = new DirectoryInfo(job.UnpackedPath);
      var mergeDirParent = mergeDirInfo.Parent;
      var mergedZip = Path.Combine(mergeDirParent!.ToString(), $"{mergeDirInfo.Name}-merged.zip");

      if (!File.Exists(sourceZip))
      {
        _logger.LogError($"Could not locate {sourceZip}.");
        continue;
      }

      _crateMergerService.MergeCrates(sourceZip, job.UnpackedPath);
      _crateMergerService.UpdateMetadata(pathToMetadata, sourceZip);
      _crateMergerService.ZipCrate(job.UnpackedPath);

      if (!await _minioService.FileExistsInBucket(Path.Combine(mergeDirParent.ToString(), mergedZip)))
      {
        _logger.LogError($"Could not locate merged RO-Crate {mergedZip}.");
        continue;
      }

      await _minioService.UploadToBucket(Path.Combine(mergeDirParent.ToString(), mergedZip));
    }
  }
}