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
    public class TagsList : ListView
    {

        List<Tag> Tags { get; set; }
        Relanota Relanota { get; set; }
        public TagsList(Relanota relanota) : base()
        {
            Relanota = relanota;
            using (Database context = new Database())
            {
                Tags = context.Tags.OrderBy(n => n.Name).ToList();
            }
            OpenSelectedItem += TagsList_OpenSelectedItem;
        }

        private void TagsList_OpenSelectedItem(ListViewItemEventArgs obj)
        {
            Relanota.NotesList.SearchFromTag((obj.Value as Tag).Name);
        }

        public void SetFromNote(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                using (Database context = new Database())
                {
                    Tags = context.Tags.OrderBy(n => n.Name).ToList();
                }
            } else
            {
                using (Database context = new Database())
                {
                    Tags = context.Tags
                        .Include(t => t.NoteTags)
                        .ThenInclude(nt => nt.Note)
                        .Where(t => t.NoteTags
                            .Any(nt => nt.Note.Name.ToLower() == name.ToLower()))
                    .ToList();
                }
            }
            this.SetSource(Tags);

        }

        public void UpdateList()
        {
            using (Database context = new Database())
            {
                Tags = context.Tags.OrderBy(n => n.Name).ToList();
            }
            this.SetSource(Tags);
        }
    }
}
