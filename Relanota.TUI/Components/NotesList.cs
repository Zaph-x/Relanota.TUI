using Core.Objects.Entities;
using Core.SqlHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Relanota.TUI.Components
{
    public class NotesList : ListView
    {
        List<Note> Notes { get; set; }
        Relanota Relanota { get; set; }
        public NotesList(Relanota relanota) : base()
        {
            Relanota = relanota;
            using (Database context = new Database())
            {
                Notes = context.Notes.OrderBy(n => n.Name).ToList();
            }
            this.SetSource(Notes);
            OpenSelectedItem += NotesList_OpenSelectedItem;
        }

        private void NotesList_OpenSelectedItem(ListViewItemEventArgs obj)
        {
            Relanota.LoadNote(obj.Value as Note);
            Relanota.TagsList.SetFromNote((obj.Value as Note).Name);
        }

        private static string NoteToString(Note note)
        {
            string retval = note.Name;
            if (retval.Length > 17) retval = retval[0..17] + "...";
            return retval;
        }

        public void Search(string pred)
        {
            using (Database context = new Database())
            {
                Notes = context.Notes.Where(n => n.Name.ToLower().Contains(pred.ToLower())).OrderBy(n => n.Name).ToList();
            }
            this.SetSource(Notes);
        }

        public void SearchFromTag(string name)
        {
            using (Database context = new Database())
            {
                Notes = context.Notes
                    .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                    .Where(n => n.NoteTags
                        .Any(nt => nt.Tag.Name.ToLower() == name.ToLower()))
                .ToList();
            }
            this.SetSource(Notes);
        }

        public void UpdateList()
        {
            using (Database context = new Database())
            {
                Notes = context.Notes.OrderBy(n => n.Name).ToList();
            }
            this.SetSource(Notes);
        }
    }
}
