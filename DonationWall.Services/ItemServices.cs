using DonationWall.Database;
using DonationWall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationWall.Services
{
    public class ItemServices
    {
        #region Singleton
        public static ItemServices Instance
        {
            get
            {
                if (instance == null) instance = new ItemServices();
                return instance;
            }
        }
        private static ItemServices instance { get; set; }
        private ItemServices()
        {
        }
        #endregion

        public List<Item> GetAllItems()
        {
            var context = new DSContext();
            return context.Items.ToList();
        }

        public void SaveItem(Item item)
        {
            var context = new DSContext();
            context.Items.Add(item);
            context.SaveChanges();
        }

        public Item GetItemById(int id)
        {
            using (var context = new DSContext())
            {
                return context.Items.FirstOrDefault(x => x.ID == id);
            }
        }

        public void DeleteItem(int ID)
        {
            var context = new DSContext();
            var target = context.Items.Where(x => x.ID == ID).FirstOrDefault();
            context.Items.Remove(target);
            context.SaveChanges();
        }
    }
}
