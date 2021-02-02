using Core;
using Core.Objects.Entities;
using Core.SqlHelper;
using Relanota.TUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Relanota.TUI
{
    public class Relanota : Window
    {
        ColorScheme color = new ColorScheme();

        View CurrView { get; set; }
        Note Note { get; set; }
        NotesList NotesList { get; set; }

        TextField nameView;
        TextView contentView;
        TextField searchField;
        private ScrollBarView _scrollBar;


        public Relanota() : base("Relanota - Ctrl + Q to quit")
        {
            Constants.DATABASE_NAME = "notes.db";
            Constants.DATABASE_PATH = "./";

            using (Database context = new Database())
                context.Database.EnsureCreated();

            //ColorScheme = Colors.TopLevel;
            Y = X = 0;
            Width = Height = Dim.Fill();
            Init();

        }

        static Terminal.Gui.Attribute MakeColor(ConsoleColor f, ConsoleColor b)
        {
            // Encode the colors into the int value.
            return new Terminal.Gui.Attribute(
                value: ((((int)f) & 0xffff) << 16) | (((int)b) & 0xffff),
                foreground: (Color)f,
                background: (Color)b
                );
        }

        public void FocusSearch()
        {
            searchField.SetFocus();
        }

        private void Init()
        {
            //Colors.TopLevel.Normal = MakeColor(ConsoleColor.Green, ConsoleColor.Black);
            //Colors.TopLevel.Focus = MakeColor(ConsoleColor.White, ConsoleColor.DarkCyan);
            //Colors.TopLevel.HotNormal = MakeColor(ConsoleColor.DarkYellow, ConsoleColor.Black);
            //Colors.TopLevel.HotFocus = MakeColor(ConsoleColor.Gray, ConsoleColor.DarkCyan);
            //Colors.TopLevel.Disabled = MakeColor(ConsoleColor.Gray, ConsoleColor.Black);



            Button newButton = new Button("_New Note") { Width = Dim.Percent(20) };
            Button saveButton = new Button("_Save Note") { Width = Dim.Percent(20), Y = Pos.Bottom(newButton), Height = 1 };
            Label notesLabel = new Label("Notes:") { Width = Dim.Percent(20), Y = Pos.Bottom(saveButton) + 1 };
            Label searchLabel = new Label("Search: ") { Y = Pos.Bottom(notesLabel) + 1 };
            searchField = new TextField { Width = Dim.Percent(20) - searchLabel.Width, Height = 1, X = Pos.Right(searchLabel), Y = Pos.Bottom(notesLabel) + 1 };

            this.KeyPress += (obj) =>
            {
                bool isAlt = obj.KeyEvent.IsAlt;
                Key key = obj.KeyEvent.Key;
                Key altClean = Key.AltMask ^ key;

                if (isAlt && altClean == Key.F)
                {
                    searchField.SetFocus();
                }
            };

            saveButton.Clicked += Save;
            newButton.Clicked += New;
            NotesList = new NotesList(this) { Y = Pos.Bottom(searchField) + 1, Width = Dim.Percent(20), Height = Dim.Fill() };

            searchField.TextChanged += (str) =>
            {
                NotesList.Search(searchField.Text.ToString()?.Trim() ?? "");
            };

            Label nameLabel = new Label()
            {
                X = Pos.Right(saveButton) + 1,
                Height = 1,
                Width = Dim.Fill(),
                Text = "Note Name:"
            };

            nameView = new TextField
            {
                Y = Pos.Bottom(nameLabel),
                X = Pos.Right(saveButton) + 1,
                //ColorScheme = color,
                Width = Dim.Fill(),
                Height = 1,
                ColorScheme = Colors.Base,
            };


            Label contentLabel = new Label
            {
                Text = "Content:",
                Y = Pos.Bottom(nameView) + 1,
                X = Pos.Right(saveButton) + 1,
                Height = 1,
                Width = Dim.Fill(),
            };

            contentView = new TextView
            {
                Y = Pos.Bottom(contentLabel),
                X = Pos.Right(saveButton) + 1,
                //ColorScheme = color,
                TextAlignment = TextAlignment.Left,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                ColorScheme = Colors.Dialog,
            };


            this.Add(newButton, saveButton, notesLabel, searchLabel, searchField, NotesList, nameLabel, contentLabel, nameView, contentView);
        }

        private void Relanota_KeyDown(KeyEventEventArgs obj)
        {

        }

        public void New()
        {
            if (string.IsNullOrWhiteSpace(nameView.Text.ToString()?.Trim()) && string.IsNullOrWhiteSpace(contentView.Text.ToString()?.Trim()))
            {
                LoadNote(null);
                return;
            }

            Button yesButton = new Button { Text = "Yes" };
            yesButton.Clicked += () =>
            {
                Save();
                if (!CanSave()) return;
                LoadNote(null);
                Application.RequestStop();
            };
            Button noButton = new Button { Text = "No" };
            noButton.Clicked += () =>
            {
                LoadNote(null);
                Application.RequestStop();
            };
            Button cancelButton = new Button { Text = "Cancel" };
            cancelButton.Clicked += () =>
            {
                Application.RequestStop();
            };

            using (Database context = new Database())
            {
                if (Note != null && context.Notes.Any(n => n.Key == Note.Key))
                {
                    if (!context.Notes.First(n => n.Key == Note.Key).Content.Equals(contentView.Text.ToString()?.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        Dialog dialog = new Dialog("Unsaved Changes", yesButton, noButton, cancelButton)
                        {
                            //ColorScheme = color,
                            Text = "You have unsaved changes. Do you wish to save these changes?",
                            X = Pos.Center(),
                            Y = Pos.Center(),
                            Width = Dim.Percent(50),
                            Height = Dim.Percent(50),
                        };
                        Application.Run(dialog);
                    }
                    else
                    {
                        LoadNote(null);
                    }

                }
                else
                {
                    Dialog dialog = new Dialog("Unsaved Changes", yesButton, noButton, cancelButton)
                    {
                        Text = "You have unsaved changes. Do you wish to save these changes?",
                        X = Pos.Center(),
                        Y = Pos.Center(),
                        Width = Dim.Percent(50),
                        Height = Dim.Percent(50),
                    };
                    Application.Run(dialog);
                }
            }
        }
        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(nameView.Text.ToString()?.Trim());
        }

        public void Save()
        {
            if (!CanSave())
            {
                Button button = new Button("Okay", true);
                button.Clicked += () => Application.RequestStop();
                Dialog dialog = new Dialog("Note not saved!", button)
                {
                    Text = "You must supply a name to save the note.",
                    Width = Dim.Percent(50),
                    Height = Dim.Percent(50),
                    //ColorScheme = color,
                };
                Application.Run(dialog);
                return;
            }
            using (Database context = new Database())
            {
                if (Note != null && context.Notes.Any(note => note.Key == Note.Key))
                {
                    Note.Update(contentView.Text.ToString()?.Trim(), nameView.Text.ToString()?.Trim(), context);
                }
                else
                {
                    Note.Save(contentView.Text.ToString()?.Trim(), nameView.Text.ToString()?.Trim(), context);
                }
            }
        }

        public void LoadNote(Note note)
        {
            Note = note;
            nameView.Text = note?.Name ?? "";
            contentView.Text = Regex.Replace(note?.Content ?? "", @"(\r\n|\r|\n)", "\n");
        }
    }
}
