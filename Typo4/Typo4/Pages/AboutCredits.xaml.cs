using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using Typo4.About;

namespace Typo4.Pages {
    public partial class AboutCredits {
        public AboutCredits() {
            InitializeComponent();

            foreach (var license in Licenses.Entries.OrderBy(x => x.DisplayName)) {
                TreeView.Items.Add(new TreeViewItem {
                    Header = license.DisplayName,
                    Items = {
                        new BbCodeBlock {
                            BbCode = $"[url=\"{license.Url}\"]Homepage[/url]" + (license.Content == null ? "" : $"\n\n[mono]{PrepareLicense(license.Content)}[/mono]")
                        }
                    }
                });
            }
        }

        private static string PrepareLicense(string source) {
            return Regex.Replace(BbCodeBlock.Encode(source), @"https?://\S+", x => {
                var last = x.Value[x.Value.Length - 1];

                string url, postfix;
                if (last == '.' || last == ';' || last == ')' || last == ',') {
                    url = x.Value.Substring(0, x.Value.Length - 1);
                    postfix = last.ToString();
                } else {
                    url = x.Value;
                    postfix = "";
                }

                return $"[url={BbCodeBlock.EncodeAttribute(BbCodeBlock.Decode(url))}]{url}[/url]{postfix}";
            });
        }

        private void OnTreeViewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
            (((FrameworkElement)sender).Parent as UIElement)?.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            });
        }
    }
}
