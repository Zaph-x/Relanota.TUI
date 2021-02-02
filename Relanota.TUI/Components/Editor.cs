using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Relanota.TUI.Components
{
    class Editor : Window
    {
        View Parent { get; set; }
        public Editor(View controler) : base("Editor")
        {
            X = Pos.Right(controler);
            Width = Dim.Fill();
            Height = Dim.Fill();
            Parent = controler;
        }

    }
}
