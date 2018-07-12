using FileParking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    public Plan ProPlan = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        ProPlan = PlanManager.GetActivePlans().SingleOrDefault(p => p.ID == Utility.ProPlanId);

    }
}