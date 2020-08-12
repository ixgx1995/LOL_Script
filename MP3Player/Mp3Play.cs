using NAudio.Wave;
using NAudioDemo.Mp3StreamingDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MP3Player
{
    enum StreamingPlaybackState
    {
        Stopped,//停止
        Playing,//播放
        Buffering,//缓冲
        Paused//暂停
    }

    public class Mp3Play
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private VolumeWaveProvider16 volumeProvider;
        private Timer timer;
        object _playLock = "播放锁";
        

        string inputFilePath = null;

        public Mp3Play(string path)
        {
            inputFilePath = path;
            //Init();
        }

        private void Init()
        {
            var inputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(inputFolder);
            inputFilePath = Path.Combine(inputFolder, "捕获音乐48khz.mp3");
        }

        #region 缓冲部分

        public void StarReceive()
        {
            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            new Thread(() =>
            {
                try
                {
                    using (var responseStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var readFullyStream = new ReadFullyStream(responseStream);
                        do
                        {
                            if (IsBufferNearlyFull)
                            {
                                Console.WriteLine("缓冲区满了，休息一下");
                                Thread.Sleep(500);
                            }
                            else
                            {
                                Mp3Frame frame;
                                try
                                {
                                    frame = Mp3Frame.LoadFromStream(readFullyStream, true);
                                }
                                catch (EndOfStreamException ex)
                                {
                                    //fullyDownloaded = true;
                                    //已到达MP3文件/流的结尾
                                    continue;
                                }
                                if (frame == null) break;
                                if (decompressor == null)
                                {
                                    // 不要认为这些细节太重要-只要帮助ACM选择正确的编解码器
                                    // 但是，缓冲提供程序不知道它的采样率是多少
                                    // 直到我们有了一个框架
                                    decompressor = CreateFrameDecompressor(frame);
                                    bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                    bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // 设置缓冲区20秒大小
                                }
                                int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                                //Console.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                                bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                            }
                            Thread.Sleep(1);
                        } while (true);
                    }

                }
                finally
                {
                    if (decompressor != null)
                    {
                        decompressor.Dispose();
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        #endregion

        #region 播放部分

        public void StarPlay()
        {
            timer = new Timer(Timer_Tick, null, 1000, 250);
            playbackState = StreamingPlaybackState.Buffering;
        }

        private void Timer_Tick(object state)
        {
            lock (_playLock)
            {
                if (playbackState != StreamingPlaybackState.Stopped)
                {
                    if (waveOut == null && bufferedWaveProvider != null)
                    {
                        Console.WriteLine("Creating WaveOut Device");
                        waveOut = new WaveOut();
                        waveOut.PlaybackStopped += OnPlaybackStopped;
                        volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                        //volumeProvider.Volume = volumeSlider1.Volume;
                        volumeProvider.Volume = 1;
                        waveOut.Init(volumeProvider);
                    }
                    else if (bufferedWaveProvider != null)
                    {
                        var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                        // 如果我们在比赛前缓冲了一个适当的数量，就可以减少口吃
                        if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
                        {
                            Buffering();
                        }
                        else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
                        {
                            Play();
                        }
                        else if (fullyDownloaded)
                        {
                            Console.WriteLine("Reached end of stream");
                            StopPlayback();
                        }
                    }

                }
            }
        }

        public void Play()
        {
            if (waveOut != null && playbackState != StreamingPlaybackState.Playing)
            {
                waveOut.Play();
                Console.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                playbackState = StreamingPlaybackState.Playing;
            }
        }

        public void Pause()
        {
            if (waveOut != null && playbackState != StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Paused;
                waveOut.Pause();
            }
        }

        public void Buffering()
        {
            if (waveOut != null && playbackState != StreamingPlaybackState.Buffering)
            {
                playbackState = StreamingPlaybackState.Buffering;
                waveOut.Pause();
                Console.WriteLine(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Console.WriteLine("Playback Stopped");
            if (e.Exception != null)
            {
                //MessageBox.Show(String.Format("Playback Error {0}", e.Exception.Message));
            }
        }

        public void StopPlayback()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (!fullyDownloaded)
                {

                }

                playbackState = StreamingPlaybackState.Stopped;
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }

                // n、 可能尚未退出线程b
                Thread.Sleep(500);
            }
        }

        #endregion
    }
}
