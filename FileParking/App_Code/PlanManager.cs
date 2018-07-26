using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileParking.Models
{
    public class PlanManager
    {
        public PlanManager()
        {

        }

        /// <summary>
        /// Get currently active plans
        /// </summary>
        /// <returns></returns>
        public static List<Plan> GetActivePlans()
        {
            using (FileParkingDataContext dc = new FileParkingDataContext())
            {
                List<Plan> list = dc.Plans.OrderBy(t => t.Price).ToList();
                list.Insert(0, FreePlan);

                return list;
            }
        }

        public static Plan FreePlan
        {
            get
            {
                return new Plan()
                {
                    ID = Guid.Empty,
                    Limit = 10,
                    Name = "Free",
                    Price = 0,
                    Term = 2,
                    FileSize = (1024 * 5)
                };
            }
        }

        public static string FreePlanDisplayString
        {
            get
            {
                return string.Format("Store or share upto {0} GB for free, get more with <a href='{1}'>pro account</a>.", 
                    (FreePlan.FileSize / 1024) * FreePlan.Limit,
                    Utility.SiteURL);
            }
        }

        public static MemberPlan GetUserActivePlan(int memberId)
        {
            using (FileParkingDataContext dc = new FileParkingDataContext())
            {
                MemberPlan mp = dc.MemberPlans.SingleOrDefault(t => t.MemberID == memberId && t.Status == (byte)MemberPlanStatus.Active);

                if (mp == null)
                {
                    mp = new MemberPlan()
                    {
                        Amount = 0,
                        DateCreated = DateTime.UtcNow,
                        ExpiryDate = DateTime.UtcNow.AddDays(2),
                        ID = 0,
                        Limit = FreePlan.Limit,
                        MemberID = memberId,
                        Name = FreePlan.Name,
                        Status = (byte)MemberPlanStatus.Active,
                        Term = FreePlan.Term,
                        TransactionCode = "",
                        TransactionDetails = "",
                        FileSize = FreePlan.FileSize
                    };
                }

                return mp;
            }
        }

        public static List<MemberPlan> GetUserPurchaseHistory(int memberId)
        {
            using (FileParkingDataContext dc = new FileParkingDataContext())
            {
                return dc.MemberPlans.Where(t => t.MemberID == memberId).OrderByDescending(t => t.DateCreated).ToList();
            }
        }

        
    }
}