using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Models;

public class DownloadProgressTracker(DownloadDataModel download)
{
    private const double SmoothingFactor = 0.2;
    private readonly Lock _lock = new();
    private readonly Queue<(long Bytes, DateTime Timestamp)> _speedWindow = new();
    private readonly Queue<(long Bytes, DateTime Timestamp)> _longTermSpeedWindow = new();
    private long _totalBytesInWindow = 0;
    private long _totalBytesInLongTermWindow = 0;
    private long _emaSpeed = 0;
    private long _currentSpeedBytesPerSecond = 0;
    private long _longTermEmaSpeed = 0;
    private bool _needsRecalculation = true;
    private bool _needsLongTermRecalculation = true;

    // Window sizes in seconds
    private const int ShortWindowSize = 5; // For current speed
    private const int LongWindowSize = 30; // For remaining time
    private const int DownsampleInterval = 1; // Downsample to 1-second intervals

    private DateTime _lastDownsampleTime = DateTime.MinValue;
    private long _bytesSinceLastDownsample = 0;

    /// <summary>
    /// Current download speed in bytes per second (calculated on-demand)
    /// </summary>
    public long CurrentSpeedBytesPerSecond
    {
        get
        {
            if (_needsRecalculation)
                CalculateSpeeds();
            return _currentSpeedBytesPerSecond;
        }
    }

    /// <summary>
    /// Long-term EMA speed for remaining time calculation
    /// </summary>
    public long LongTermEmaSpeedBytesPerSecond
    {
        get
        {
            if (_needsLongTermRecalculation)
                CalculateLongTermSpeeds();
            return _longTermEmaSpeed;
        }
    }

    /// <summary>
    /// Updates the progress with new data but defers speed calculations.
    /// </summary>
    public void UpdateProgress(long downloadedBytes)
    {
        var now = DateTime.Now;
        lock (_lock)
        {
            // Update short-term window
            _speedWindow.Enqueue((downloadedBytes, now));
            _totalBytesInWindow += downloadedBytes;
            _needsRecalculation = true;

            // Downsample for long-term window
            _bytesSinceLastDownsample += downloadedBytes;
            if ((now - _lastDownsampleTime).TotalSeconds >= DownsampleInterval)
            {
                if (_lastDownsampleTime != DateTime.MinValue)
                {
                    // Add aggregated sample to long-term window
                    _longTermSpeedWindow.Enqueue((_bytesSinceLastDownsample, _lastDownsampleTime));
                    _totalBytesInLongTermWindow += _bytesSinceLastDownsample;
                }
                _lastDownsampleTime = now;
                _bytesSinceLastDownsample = 0;
                _needsLongTermRecalculation = true;
            }
        }
    }

    /// <summary>
    /// Calculates short-term speeds (for current speed).
    /// </summary>
    private void CalculateSpeeds()
    {
        lock (_lock)
        {
            if (download.FileSizeInBytes > 0 && download.FileSizeInBytes == download.DownloadedBytes)
            {
                _speedWindow.Clear();
            }
            
            if (_speedWindow.Count == 0)
            {
                _currentSpeedBytesPerSecond = 0;
                _needsRecalculation = false;
                return;
            }

            var now = DateTime.Now;
            // Remove outdated samples from short-term window
            while (_speedWindow.Count > 0 && (now - _speedWindow.Peek().Timestamp).TotalSeconds > ShortWindowSize)
            {
                var oldSample = _speedWindow.Dequeue();
                _totalBytesInWindow -= oldSample.Bytes;
            }

            // Calculate current speed
            var elapsedTime = (now - _speedWindow.Peek().Timestamp).TotalSeconds;
            if (elapsedTime <= 0.05) // Clamp to avoid division by very small numbers
            {
                _currentSpeedBytesPerSecond = 0;
            }
            else
            {
                _currentSpeedBytesPerSecond = (long)(_totalBytesInWindow / elapsedTime);
            }

            // Update EMA for short-term speed
            if (_emaSpeed == 0)
                _emaSpeed = _currentSpeedBytesPerSecond;
            else
                _emaSpeed = (long)(_currentSpeedBytesPerSecond * SmoothingFactor + _emaSpeed * (1 - SmoothingFactor));

            _needsRecalculation = false;
        }
    }

    /// <summary>
    /// Calculates long-term speeds (for remaining time).
    /// </summary>
    private void CalculateLongTermSpeeds()
    {
        lock (_lock)
        {
            if (download.FileSizeInBytes > 0 && download.FileSizeInBytes == download.DownloadedBytes)
            {
                _longTermSpeedWindow.Clear();
            }
            
            if (_longTermSpeedWindow.Count == 0)
            {
                _longTermEmaSpeed = 0;
                _needsLongTermRecalculation = false;
                return;
            }

            var now = DateTime.Now;
            // Remove outdated samples from long-term window
            while (_longTermSpeedWindow.Count > 0 && (now - _longTermSpeedWindow.Peek().Timestamp).TotalSeconds > LongWindowSize)
            {
                var oldSample = _longTermSpeedWindow.Dequeue();
                _totalBytesInLongTermWindow -= oldSample.Bytes;
            }

            // Calculate long-term speed
            var elapsedTime = (now - _longTermSpeedWindow.Peek().Timestamp).TotalSeconds;
            if (elapsedTime <= 0.05) // Clamp to avoid division by very small numbers
            {
                _longTermEmaSpeed = 0;
            }
            else
            {
                var longTermSpeed = (long)(_totalBytesInLongTermWindow / elapsedTime);
                // Update EMA for long-term speed
                if (_longTermEmaSpeed == 0)
                    _longTermEmaSpeed = longTermSpeed;
                else
                    _longTermEmaSpeed = (long)(longTermSpeed * SmoothingFactor + _longTermEmaSpeed * (1 - SmoothingFactor));
            }

            _needsLongTermRecalculation = false;
        }
    }

    /// <summary>
    /// Estimates the remaining time for the download using long-term EMA speed.
    /// </summary>
    public TimeSpan EstimateRemainingTime()
    {
        if (LongTermEmaSpeedBytesPerSecond <= 0 || download.FileSizeInBytes <= 0)
            return TimeSpan.Zero;

        
        if (download.FileSizeInBytes == download.DownloadedBytes)
            return TimeSpan.Zero;
        
        long remainingBytes = download.FileSizeInBytes - download.DownloadedBytes;
        return TimeSpan.FromSeconds(remainingBytes / LongTermEmaSpeedBytesPerSecond);
    }
}