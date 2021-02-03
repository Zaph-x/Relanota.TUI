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
        public NotesList NotesList { get; set; }
        public TagsList TagsList { get; set; }

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
                Width = Dim.Percent(59),
                Text = "Note Name:"
            };
            TagsList = new TagsList(this) { X = Pos.Right(nameLabel) + 1, Height = Dim.Fill(), Width = Dim.Percent(20) };
            TagsList.UpdateList();
            nameView = new TextField
            {
                Y = Pos.Bottom(nameLabel),
                X = Pos.Right(saveButton) + 1,
                //ColorScheme = color,
                Width = Dim.Percent(59),
                Height = 1,
                ColorScheme = Colors.Base,
            };


            Label contentLabel = new Label
            {
                Text = "Content:",
                Y = Pos.Bottom(nameView) + 1,
                X = Pos.Right(saveButton) + 1,
                Height = 1,
                Width = Dim.Percent(59),
                //Width = Dim.Fill(),
            };


            contentView = new TextView
            {
                Y = Pos.Bottom(contentLabel),
                X = Pos.Right(saveButton) + 1,
                //ColorScheme = color,
                TextAlignment = TextAlignment.Left,
                Height = Dim.Fill(),
                Width = Dim.Percent(59),
                //Width = Dim.Fill(),
                ColorScheme = Colors.Dialog,
            };

            Label tagLabel = new Label { Text = "Add Tag:", X=Pos.X(TagsList), Y=Pos.Bottom(TagsList) + 2, Width = Dim.Percent(20) };

            TextField addTagField = new TextField { Y = Pos.Bottom(tagLabel), Width = Dim.Percent(20), X = Pos.X(tagLabel) };

            addTagField.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.Enter)
                {
                    string name = addTagField.Text.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) return;

                    using (Database context = new Database())
                    {
                        if (context.TryGetTag(name, out Tag tag) && tag != null)
                        {
                            if (Note != null)
                            {
                                Note.AddTag(tag, context);
                            }
                        } else
                        {
                            tag = new Tag()
                            {
                                Name = name,
                                Description = "",
                            };
                            tag.Save(context);
                            if (Note != null)
                            {
                                Note.AddTag(tag, context);
                            }
                        }
                        
                    }
                }
            };

            TagsList.Height = Dim.Fill() - tagLabel.Height - addTagField.Height - Dim.Sized(2);

            this.Add(newButton, saveButton, notesLabel, searchLabel, searchField, NotesList, nameLabel, contentLabel, nameView, contentView, TagsList, tagLabel,addTagField);
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
                    string content = Regex.Replace(contentView.Text.ToString()?.Trim(), @"\r\n?|\n", "\n");
                    string actualNote = Regex.Replace(context.Notes.First(n => n.Key == Note.Key).Content.Trim(), @"\r\n?|\n", "\n");
                    if (!content.Equals(actualNote, StringComparison.InvariantCultureIgnoreCase))
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
            if (note == null)
            {
                TagsList.UpdateList();
                NotesList.UpdateList();
            }
            nameView.Text = note?.Name ?? "";
            contentView.Text = Regex.Replace(note?.Content ?? "", @"\r\n?|\n", "\n");
        }
    }
}
