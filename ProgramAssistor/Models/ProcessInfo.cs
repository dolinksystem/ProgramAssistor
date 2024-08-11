using CommunityToolkit.Mvvm.ComponentModel;

namespace ProgramAssistor.Models
{
    public class ProcessInfo : ObservableObject
    {
        private string _processName;
        public string ProcessName
        {
            get { return _processName; }
            set { SetProperty(ref _processName, value); }
        }

        private int _x;
        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        public ProcessInfo()
        {
        }

        public ProcessInfo(string processName, int x, int y, int width, int height)
        {
            ProcessName = processName;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
    }
}
