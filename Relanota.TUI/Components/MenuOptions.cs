using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Relanota.TUI.Components
{
    public class MenuOptions : ListView
    {
        List<(string, View)> Options;

        public MenuOptions(List<(string, View)> options) : base()
        {
            Options = options;
            Width = 20;
            Height = Dim.Percent(50);

            this.SetSource(options.Select(opt => opt.Item1).ToList());
        }
    }
}
