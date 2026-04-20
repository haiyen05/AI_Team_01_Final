using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace AI
{
    /// <summary>
    /// Quản lý âm thanh dùng NAudio (không cần COM/WMPLib):
    ///   - Nhạc nền: phát một mạch, KHÔNG reset khi đổi form,
    ///     tự quay lại từ đầu khi file đã phát hết.
    ///   - Sound effect: phát chồng lên trên thread riêng, không ảnh hưởng nhạc nền.
    /// </summary>
    internal static class SoundManager
    {
        // ── Nhạc nền ──────────────────────────────────────────────────────────
        private static readonly object _bgLock = new object();
        private static WaveOutEvent   _bgOut;
        private static AudioFileReader _bgReader;
        private static bool _bgRunning;

        public static void EnsureBackgroundMusic()
        {
            if (AppState.IsMuted)
            {
                StopBackgroundMusic();
                return;
            }

            lock (_bgLock)
            {
                // Đang phát bình thường → không động vào
                if (_bgRunning && _bgOut != null
                    && _bgOut.PlaybackState == PlaybackState.Playing)
                {
                    return;
                }

                // Đã bị dừng (ví dụ mute rồi unmute) → phát lại từ vị trí hiện tại
                if (_bgRunning && _bgOut != null
                    && _bgOut.PlaybackState == PlaybackState.Stopped
                    && _bgReader != null)
                {
                    // File đã hết hoặc bị stop → restart từ đầu
                    _bgReader.Position = 0;
                    _bgOut.Play();
                    return;
                }

                StartBgPlayer();
            }
        }

        private static void StartBgPlayer()
        {
            // Gọi bên trong lock(_bgLock)
            try
            {
                string path = ResolvePath(Path.Combine("Sounds", "main.wav"));
                if (!File.Exists(path)) return;

                // Dọn player cũ
                SafeDisposeBg();

                _bgReader = new AudioFileReader(path);
                _bgOut    = new WaveOutEvent();
                _bgOut.Init(_bgReader);
                _bgOut.Volume = 0.7f;

                // Khi file phát hết → quay về đầu và phát lại
                _bgOut.PlaybackStopped += OnBgStopped;

                _bgOut.Play();
                _bgRunning = true;
            }
            catch
            {
                SafeDisposeBg();
            }
        }

        private static void OnBgStopped(object sender, StoppedEventArgs e)
        {
            lock (_bgLock)
            {
                if (!_bgRunning || AppState.IsMuted) return;
                if (_bgReader == null || _bgOut == null) return;

                try
                {
                    // Quay đầu rồi phát lại → "chỉ restart khi hết file"
                    _bgReader.Position = 0;
                    _bgOut.Play();
                }
                catch { }
            }
        }

        public static void StopBackgroundMusic()
        {
            lock (_bgLock)
            {
                try { _bgOut?.Stop(); } catch { }
                _bgRunning = false;
            }
        }

        public static void RestartBackgroundMusic()
        {
            lock (_bgLock)
            {
                SafeDisposeBg();
                _bgRunning = false;
            }
            EnsureBackgroundMusic();
        }

        private static void SafeDisposeBg()
        {
            try { _bgOut?.Stop();    } catch { }
            try { _bgOut?.Dispose(); } catch { }
            try { _bgReader?.Dispose(); } catch { }
            _bgOut    = null;
            _bgReader = null;
        }

        // ── Sound effect ──────────────────────────────────────────────────────
        // Mỗi effect chạy trên thread riêng → không đụng nhạc nền
        private static void PlayEffectAsync(string relativePath)
        {
            if (AppState.IsMuted) return;

            string path = ResolvePath(relativePath);
            if (!File.Exists(path)) return;

            Thread t = new Thread(() =>
            {
                WaveOutEvent    wo = null;
                AudioFileReader rd = null;
                try
                {
                    rd = new AudioFileReader(path);
                    wo = new WaveOutEvent();
                    wo.Init(rd);
                    wo.Play();

                    // Chờ hết sound trên thread này
                    while (wo.PlaybackState == PlaybackState.Playing)
                        Thread.Sleep(50);
                }
                catch { }
                finally
                {
                    try { wo?.Dispose(); } catch { }
                    try { rd?.Dispose(); } catch { }
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static void PlayClick() => PlayEffectAsync(Path.Combine("Sounds", "click.wav"));
        public static void PlayEat()   => PlayEffectAsync(Path.Combine("Sounds", "an.wav"));
        public static void PlayWin()   => PlayEffectAsync(Path.Combine("Sounds", "win.wav"));

        // ── Helper ────────────────────────────────────────────────────────────
        private static string ResolvePath(string relativePath)
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
    }
}
