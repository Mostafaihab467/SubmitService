using Newtonsoft.Json;
using SubmitServ.Helpers;
using SubmitServ.Models;
using SubmitServ.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using MediatR;

namespace SubmitServ
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _statusTimer;
        private readonly GetTokenRepository _getTokenRepository = new GetTokenRepository();
        private readonly EreciptsRepository _ereciptsRepository = new EreciptsRepository();
        private readonly ETASubmitRepo _etaSubmitRepo = new ETASubmitRepo();
        private bool _isDarkMode = true;

        private static int _dbAcceptedCount = 0;
        private static int _dbFailedCount = 0;
        private static bool _isStatsLoadedFromDb = false;
        private static readonly object _dbStatsLock = new object();

        private int _sessionSubmitted = 0;
        private int _sessionAccepted = 0;
        private int _sessionRejected = 0;
        private int _sessionFailed = 0;
        private DateTime _sessionStartTime;
        private readonly object _statsLock = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _cancellationLock = new object();

        public MainWindow()
        {
            InitializeComponent();

            _statusTimer = new DispatcherTimer();
            _statusTimer.Tick += Status_Timer_Tick;
            _statusTimer.Interval = TimeSpan.FromSeconds(2);

            SettingsManager.LoadSettings();
            LoadSettingsToUI();

            LoadInitialStatsFromDatabase();
        }

        private async void LoadInitialStatsFromDatabase()
        {
            if (_isStatsLoadedFromDb) return;

            try
            {
                var acceptedCount = await _ereciptsRepository.GetAcceptedCountAsync();
                var failedCount = await _ereciptsRepository.GetFailedCountAsync();

                lock (_dbStatsLock)
                {
                    _dbAcceptedCount = acceptedCount;
                    _dbFailedCount = failedCount;
                    _isStatsLoadedFromDb = true;
                }

                UpdateStatsDisplay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial stats: {ex.Message}");
            }
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;

            Application.Current.RequestedThemeVariant = _isDarkMode 
                ? Avalonia.Styling.ThemeVariant.Dark 
                : Avalonia.Styling.ThemeVariant.Light;

            ThemeToggleBtn.Content = _isDarkMode ? "☀️  Light Mode" : "🌙  Dark Mode";
        }

        private async void Start_Status_Service_Click(object sender, RoutedEventArgs e)
        {
            ApplySettingsFromUi();

            // Reload DB counts so accepted/failed reflect latest data
            try
            {
                var acceptedCount = await _ereciptsRepository.GetAcceptedCountAsync();
                var failedCount = await _ereciptsRepository.GetFailedCountAsync();
                lock (_dbStatsLock)
                {
                    _dbAcceptedCount = acceptedCount;
                    _dbFailedCount = failedCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading stats on Start: {ex.Message}");
            }

            lock (_statsLock)
            {
                _sessionSubmitted = 0;
                _sessionAccepted = 0;
                _sessionRejected = 0;
                _sessionFailed = 0;
                _sessionStartTime = DateTime.Now;
            }
            UpdateStatsDisplay();

            lock (_cancellationLock)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            _statusTimer.Start();
            JobMonitor();
        }

        private void JobMonitor()
        {
            _statusTimer.IsEnabled = true;
        }

        private async void Status_Timer_Tick(object sender, EventArgs e)
        {
            _statusTimer.IsEnabled = false;
            await DoSubmitAsync();
        }

        private async Task DoSubmitAsync()
        {
            CancellationToken cancellationToken;
            lock (_cancellationLock)
            {
                cancellationToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                txt_Status_Log.Text = $"Operation cancelled at {DateTimeOffset.Now}";
                return;
            }

            txt_Status_Log.Text = $"Cycle Started: {DateTimeOffset.Now}";

            try
            {
                List<EReceiptLogsForSubmit> list = await _ereciptsRepository.GetEReceiptLogsAsync(Convert.ToInt16(AppSettings.StatusLimit));

                if (cancellationToken.IsCancellationRequested)
                {
                    txt_Status_Log.Text += $"\n⚠️ Cancelled after fetch.";
                    return;
                }

                if (list != null && list.Count > 0)
                {
                    txt_Status_Log.Text += $"\nFetched {list.Count} receipts. Starting parallel submittion...";

                    int successCount = 0;
                    int failCount = 0;

                    using (SemaphoreSlim semaphore = new SemaphoreSlim(6))
                    {
                        var tasks = list.Select(async log =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            try
                            {
                                await semaphore.WaitAsync(cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }

                            try
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                receiptsModel etaRequest = JsonConvert.DeserializeObject<receiptsModel>(log.ETARequestPayload);
                                var response = await ETAApiRepository.Get().SendEReceiptSubmitDirect(log.ETARequestPayload);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                UpdateSubmitResponseDTO updateSubmit = new UpdateSubmitResponseDTO();
                                updateSubmit.LogID = log.LogID;
                                updateSubmit.EReceiptID = log.EReceiptID;

                                if (response != null && response.Result != null)
                                {
                                    string uuid = "";
                                    string error = "Unknown response structure";

                                    if (response.Result.acceptedDocuments != null && response.Result.acceptedDocuments.Any())
                                    {
                                        uuid = response.Result.acceptedDocuments.FirstOrDefault().uuid;
                                        updateSubmit.ETAResponsePayload = JsonConvert.SerializeObject(response.Result);
                                        error = response.error ?? "";
                                        updateSubmit.Error = error;
                                        UpdateSubmissionStats(accepted: true, rejected: false, failed: false);
                                        System.Threading.Interlocked.Increment(ref successCount);
                                    }
                                    else if (response.Result.rejectedDocuments != null && response.Result.rejectedDocuments.Any())
                                    {
                                        uuid = response.Result.rejectedDocuments.FirstOrDefault().uuid;
                                        updateSubmit.ETAResponsePayload = JsonConvert.SerializeObject(response.Result);
                                        error = response.Result.rejectedDocuments.FirstOrDefault().error?.details?.FirstOrDefault()?.message ?? "Rejected";
                                        UpdateSubmissionStats(accepted: false, rejected: true, failed: false);
                                        System.Threading.Interlocked.Increment(ref failCount);
                                    }
                                    else
                                    {
                                        UpdateSubmissionStats(accepted: false, rejected: false, failed: true);
                                        System.Threading.Interlocked.Increment(ref failCount);
                                        await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, uuid, "", "", error);
                                        return;
                                    }

                                    updateSubmit.ETAUUID = uuid;
                                    var _dateIssued1 = etaRequest.receipts[0].header.dateTimeIssued;
                                    if (!_dateIssued1.EndsWith("Z")) _dateIssued1 += "Z";
                                    updateSubmit.ETAQRCode = $"{AppSettings.qrURL}{uuid}/share/{_dateIssued1}#Total:{etaRequest.receipts[0].itemData.FirstOrDefault().total},IssuerRIN:{etaRequest.receipts[0].seller.rin}";
                                    updateSubmit.Error = error;

                                    await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, updateSubmit.ETAUUID, updateSubmit.ETAQRCode, updateSubmit.ETAResponsePayload, updateSubmit.Error);
                                }
                                else
                                {
                                    updateSubmit.ETAUUID = "";
                                    updateSubmit.ETAQRCode = "";
                                    updateSubmit.ETAResponsePayload = "";
                                    updateSubmit.Error = response?.error ?? "Unknown error or null response";

                                    await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, updateSubmit.ETAUUID, updateSubmit.ETAQRCode, updateSubmit.ETAResponsePayload, updateSubmit.Error);
                                    UpdateSubmissionStats(accepted: false, rejected: false, failed: true);
                                    System.Threading.Interlocked.Increment(ref failCount);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception ex)
                            {
                                System.Threading.Interlocked.Increment(ref failCount);
                                Console.WriteLine($"Error processing receipt {log.EReceiptNumber}: {ex}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });

                        await Task.WhenAll(tasks);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        txt_Status_Log.Text += $"\n⚠️ Operation Cancelled by user. {successCount} Success, {failCount} Failed before cancellation.";
                    }
                    else
                    {
                        txt_Status_Log.Text += $"\nBatch Complete: {successCount} Success, {failCount} Failed.";
                    }
                }
                else
                {
                    txt_Status_Log.Text = $"No data to be displayed at {DateTimeOffset.Now}";
                }
            }
            catch (OperationCanceledException)
            {
                txt_Status_Log.Text = $"⚠️ Operation Cancelled at {DateTimeOffset.Now}";
            }
            catch (Exception ex)
            {
                txt_Status_Log.Text = $"System Error: {ex.Message}";
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    JobMonitor();
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _statusTimer.IsEnabled = false;

            lock (_cancellationLock)
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    txt_Status_Log.Text += $"\n⚠️ Cancellation requested - stopping all operations...";
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txt_Status_Log.Text = "";
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            ApplySettingsFromUi();
            await DoSingleBatchAsync();
        }

        private async Task DoSingleBatchAsync()
        {
            CancellationTokenSource batchCts;
            lock (_cancellationLock)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                batchCts = _cancellationTokenSource;
            }
            CancellationToken cancellationToken = batchCts.Token;

            // Reload DB counts so accepted/failed reflect latest data
            try
            {
                var acceptedCount = await _ereciptsRepository.GetAcceptedCountAsync();
                var failedCount = await _ereciptsRepository.GetFailedCountAsync();
                lock (_dbStatsLock)
                {
                    _dbAcceptedCount = acceptedCount;
                    _dbFailedCount = failedCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading stats on batch: {ex.Message}");
            }

            lock (_statsLock)
            {
                _sessionSubmitted = 0;
                _sessionAccepted = 0;
                _sessionRejected = 0;
                _sessionFailed = 0;
                _sessionStartTime = DateTime.Now;
            }
            UpdateStatsDisplay();

            txt_Status_Log.Text = $"One Batch Started: {DateTimeOffset.Now}";

            try
            {
                List<EReceiptLogsForSubmit> list = await _ereciptsRepository.GetEReceiptLogsAsync(Convert.ToInt16(AppSettings.StatusLimit));

                if (cancellationToken.IsCancellationRequested)
                {
                    txt_Status_Log.Text += $"\n⚠️ Cancelled after fetch.";
                    return;
                }

                if (list != null && list.Count > 0)
                {
                    txt_Status_Log.Text += $"\nFetched {list.Count} receipts. Starting parallel submittion...";

                    int successCount = 0;
                    int failCount = 0;

                    using (SemaphoreSlim semaphore = new SemaphoreSlim(6))
                    {
                        var tasks = list.Select(async log =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            try
                            {
                                await semaphore.WaitAsync(cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }

                            try
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                receiptsModel etaRequest = JsonConvert.DeserializeObject<receiptsModel>(log.ETARequestPayload);
                                var response = await ETAApiRepository.Get().SendEReceiptSubmitDirect(log.ETARequestPayload);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                UpdateSubmitResponseDTO updateSubmit = new UpdateSubmitResponseDTO();
                                updateSubmit.LogID = log.LogID;
                                updateSubmit.EReceiptID = log.EReceiptID;

                                if (response != null && response.Result != null)
                                {
                                    string uuid = "";
                                    string error = "Unknown response structure";

                                    if (response.Result.acceptedDocuments != null && response.Result.acceptedDocuments.Any())
                                    {
                                        uuid = response.Result.acceptedDocuments.FirstOrDefault().uuid;
                                        updateSubmit.ETAResponsePayload = JsonConvert.SerializeObject(response.Result);
                                        error = response.error ?? "";
                                        updateSubmit.Error = error;
                                        UpdateSubmissionStats(accepted: true, rejected: false, failed: false);
                                        System.Threading.Interlocked.Increment(ref successCount);
                                    }
                                    else if (response.Result.rejectedDocuments != null && response.Result.rejectedDocuments.Any())
                                    {
                                        uuid = response.Result.rejectedDocuments.FirstOrDefault().uuid;
                                        updateSubmit.ETAResponsePayload = JsonConvert.SerializeObject(response.Result);
                                        error = response.Result.rejectedDocuments.FirstOrDefault().error?.details?.FirstOrDefault()?.message ?? "Rejected";
                                        UpdateSubmissionStats(accepted: false, rejected: true, failed: false);
                                        System.Threading.Interlocked.Increment(ref failCount);
                                    }
                                    else
                                    {
                                        UpdateSubmissionStats(accepted: false, rejected: false, failed: true);
                                        System.Threading.Interlocked.Increment(ref failCount);
                                        await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, uuid, "", "", error);
                                        return;
                                    }

                                    updateSubmit.ETAUUID = uuid;
                                    var _dateIssued2 = etaRequest.receipts[0].header.dateTimeIssued;
                                    if (!_dateIssued2.EndsWith("Z")) _dateIssued2 += "Z";
                                    updateSubmit.ETAQRCode = $"{AppSettings.qrURL}{uuid}/share/{_dateIssued2}#Total:{etaRequest.receipts[0].itemData.FirstOrDefault().total},IssuerRIN:{etaRequest.receipts[0].seller.rin}";
                                    updateSubmit.Error = error;

                                    await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, updateSubmit.ETAUUID, updateSubmit.ETAQRCode, updateSubmit.ETAResponsePayload, updateSubmit.Error);
                                }
                                else
                                {
                                    updateSubmit.ETAUUID = "";
                                    updateSubmit.ETAQRCode = "";
                                    updateSubmit.ETAResponsePayload = "";
                                    updateSubmit.Error = response?.error ?? "Unknown error or null response";

                                    await _ereciptsRepository.ETALogsAsync(updateSubmit.LogID, updateSubmit.EReceiptID, updateSubmit.ETAUUID, updateSubmit.ETAQRCode, updateSubmit.ETAResponsePayload, updateSubmit.Error);
                                    UpdateSubmissionStats(accepted: false, rejected: false, failed: true);
                                    System.Threading.Interlocked.Increment(ref failCount);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception ex)
                            {
                                System.Threading.Interlocked.Increment(ref failCount);
                                Console.WriteLine($"Error processing receipt {log.EReceiptNumber}: {ex}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });

                        await Task.WhenAll(tasks);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        txt_Status_Log.Text += $"\n⚠️ Batch Cancelled. {successCount} Success, {failCount} Failed before cancellation.";
                    }
                    else
                    {
                        txt_Status_Log.Text += $"\n✓ Batch Complete: {successCount} Success, {failCount} Failed.";
                    }
                }
                else
                {
                    txt_Status_Log.Text = $"No data to process at {DateTimeOffset.Now}";
                }
            }
            catch (OperationCanceledException)
            {
                txt_Status_Log.Text = $"⚠️ Batch Cancelled at {DateTimeOffset.Now}";
            }
            catch (Exception ex)
            {
                txt_Status_Log.Text = $"System Error: {ex.Message}";
            }
        }

        private void ApplySettingsFromUi()
        {
            AppSettings.client_secret = client_secret.Text;
            AppSettings.client_id = client_id.Text;
            AppSettings.posserial = posserial.Text;
            AppSettings.pososversion = pososversion.Text;
            AppSettings.StatusLimit = Limit.Text;
            AppSettings.connectionStr = Server.Text;
            AppSettings.qrURL = qrURLText.Text;
        }

        private void LoadSettingsToUI()
        {
            client_id.Text = AppSettings.client_id ?? "5ee13adf-d438-48ac-b6f9-d30c2b0a4a62";
            client_secret.Text = AppSettings.client_secret ?? "d2786429-f801-4b93-a709-353090a5a264";
            posserial.Text = AppSettings.posserial ?? "E1EA0240-3140-49CF-AFA6-25EC735E018A";
            pososversion.Text = AppSettings.pososversion ?? "Linux";
            Limit.Text = AppSettings.StatusLimit ?? "10";
            Server.Text = AppSettings.connectionStr ?? "Server=localhost;Database=TaxTech_Production;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;";
            qrURLText.Text = AppSettings.qrURL ?? "https://invoicing.eta.gov.eg/receipts/search/";
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplySettingsFromUi();
                SettingsManager.SaveSettings();
                txt_Status_Log.Text = $"✓ Settings saved successfully at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                txt_Status_Log.Text = $"✗ Error saving settings: {ex.Message}";
            }
        }

        private void UpdateStatsDisplay()
        {
            Dispatcher.UIThread.Post(() =>
            {
                int totalAccepted, totalFailed, totalSubmitted;
                lock (_dbStatsLock)
                {
                    totalAccepted = _dbAcceptedCount;
                    totalFailed = _dbFailedCount;
                }

                lock (_statsLock)
                {
                    totalAccepted += _sessionAccepted;
                    totalFailed += _sessionFailed;
                    totalSubmitted = totalAccepted + totalFailed;
                }

                txt_TotalCount.Text = totalSubmitted.ToString();
                txt_AcceptedCount.Text = totalAccepted.ToString();
                txt_RejectedCount.Text = _sessionRejected.ToString();
                txt_FailedCount.Text = totalFailed.ToString();

                double rate = 0;
                lock (_statsLock)
                {
                    var elapsed = (DateTime.Now - _sessionStartTime).TotalSeconds;
                    if (elapsed > 0)
                    {
                        rate = _sessionSubmitted / elapsed;
                    }
                }
                txt_Rate.Text = rate.ToString("F2");
            });
        }

        private void UpdateSubmissionStats(bool accepted, bool rejected, bool failed)
        {
            lock (_statsLock)
            {
                _sessionSubmitted++;
                if (accepted) _sessionAccepted++;
                else if (rejected) _sessionRejected++;
                else if (failed) _sessionFailed++;
            }
            UpdateStatsDisplay();
        }
    }
}
