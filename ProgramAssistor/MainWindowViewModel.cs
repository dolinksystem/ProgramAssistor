using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgramAssistor.Defines;
using ProgramAssistor.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ProgramAssistor
{
    public class MainWindowViewModel : ObservableObject
    {
        // WinAPI 함수 선언
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const uint SWP_NOZORDER = 0x0004;
        const int SWP_SHOWWINDOW = 0x0040;

        const int SW_RESTORE = 9;

        ObservableCollection<ProcessInfo> _processList;
        ObservableCollection<ProcessInfo> ProcessList
        {
            get { return _processList; }
            set { SetProperty(ref _processList, value); }
        }

        public IRelayCommand<string> SaveCommand { get; }
        public IRelayCommand<string> MoveCommand { get; }

        private readonly DispatcherTimer _timer;


        public MainWindowViewModel()
        {
            ProcessList = new ObservableCollection<ProcessInfo>();
            if (!LoadProcessInfo(Def.Config.Position))
            {
                MakeDefaultData();
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (sender, args) => MoveProcessInfo();
            //_timer.Start();

            SaveCommand = new RelayCommand<string>(SaveCommand_Function);
            MoveCommand = new RelayCommand<string>(MoveCommand_Function);
        }

        public void RetreiveProcess()
        {
            Process[] processes = Process.GetProcesses();

            Console.WriteLine("현재 실행 중인 프로세스 목록:");
            Console.WriteLine("---------------------------------");
            foreach (Process process in processes)
            {
                try
                {
                    Console.WriteLine($"프로세스 이름: {process.ProcessName}, PID: {process.Id}, 메모리 사용량: {process.PrivateMemorySize64 / 1024} KB");
                }
                catch (Exception ex)
                {
                    // 프로세스에 접근할 수 없는 경우 예외 처리
                    Console.WriteLine($"프로세스 이름: {process.ProcessName}, PID: {process.Id}, 접근할 수 없습니다. ({ex.Message})");
                }
            }
            Console.WriteLine("---------------------------------");
        }

        void MakeDefaultData()
        {
            ProcessInfo p1 = new ProcessInfo("notepad++", 100, 100, 800, 400);
            ProcessList.Add(p1);
            ProcessInfo p2 = new ProcessInfo("Everything", 100, 400, 800, 400);
            ProcessList.Add(p2);
        }

        public void SaveProcessInfo(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                ProcessInfo[] piArray = ProcessList.ToArray();
                string jsonString = JsonSerializer.Serialize<ProcessInfo[]>(piArray, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void UpdateProcessInfo()
        {
            for (int i = 0; i < ProcessList.Count; i++)
            {
                Process[] processes = Process.GetProcessesByName(ProcessList[i].ProcessName);
                if (processes.Length == 0)
                {
                    continue;
                }

                IntPtr hWnd = processes[0].MainWindowHandle;
                if (hWnd == IntPtr.Zero)
                {
                    continue;
                }
                GetWindowRect(hWnd, out RECT rect);
                ProcessList[i].X = rect.Left;
                ProcessList[i].Y = rect.Top;
                ProcessList[i].Width = rect.Right - rect.Left;
                ProcessList[i].Height = rect.Bottom - rect.Top;
            }
        }
        public bool LoadProcessInfo(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            string jsonString = File.ReadAllText(filePath);
            ProcessInfo[] processInfos = JsonSerializer.Deserialize<ProcessInfo[]>(jsonString);
            ProcessList = null;
            ProcessList = new ObservableCollection<ProcessInfo>(processInfos);
            return true;
        }

        public void MoveProcessInfo()
        {
            foreach (var v in ProcessList)
            {
                MovePosition(v.ProcessName, v.X, v.Y, v.Width, v.Height);
            }
        }

        public bool MovePosition(string processName, int x, int y, int cx = 0, int cy = 0)
        {
            // 프로세스 검색
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return false;
            }

            // 첫 번째 프로세스의 핸들을 가져옴
            IntPtr hWnd = processes[0].MainWindowHandle;
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }

            // 윈도우를 정상 크기로 복원 (최소화된 상태에서)
            ShowWindow(hWnd, SW_RESTORE);

            // 윈도우 위치와 크기 변경
            bool result;
            if (cx <= 0 || cy <= 0)
            {
                result = SetWindowPos(hWnd, IntPtr.Zero, x, y, cx, cy, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            else
            {
                result = SetWindowPos(hWnd, IntPtr.Zero, x, y, cx, cy, SWP_NOZORDER | SWP_SHOWWINDOW);
            }
            return result;
        }

        private void SaveCommand_Function(string para)
        {
            UpdateProcessInfo();
            SaveProcessInfo(Def.Config.Position);
        }

        private void MoveCommand_Function(string para)
        {
            MoveProcessInfo();
        }
    }
}
