using DonationWall.Database;
using DonationWall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DonationWall.Services
{
    public class InterestServices
    {
        #region Singleton
        public static InterestServices Instance
        {
            get
            {
                if (instance == null) instance = new InterestServices();
                return instance;
            }
        }
        private static InterestServices instance { get; set; }
        private InterestServices()
        {
        }
        #endregion

        public List<Interest> GetAllInterestsByDonorID(string donorId)
        {
            using (var context = new DSContext())
            {
                return context.Interests.Where(x => x.Post.User_ID == donorId && x.Status == "Pending").ToList();
            }
        }

        public List<Interest> GetAllInterests()
        {
            using (var context = new DSContext())
            {
                return context.Interests.Where(x => x.Status == "Pending").ToList();
            }
        }

        public void SaveInterest(Interest interest)
        {
            using (var context = new DSContext())
            {
                context.Interests.Add(interest);
                context.SaveChanges();
            }
        }

        public void DeleteInterest(int id)
        {
            using (var context = new DSContext())
            {
                var target = context.Interests.FirstOrDefault(x => x.ID == id);
                if (target != null)
                {
                    context.Interests.Remove(target);
                    context.SaveChanges();
                }
            }
        }
    }
}
