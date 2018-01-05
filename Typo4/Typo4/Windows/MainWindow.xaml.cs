using System.Windows.Forms.Integration;

namespace Typo4.Windows {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
            ElementHost.EnableModelessKeyboardInterop(this);
        }
    }
}
