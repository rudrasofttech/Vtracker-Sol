<%@ WebHandler Language="C#" Class="mplanhandler" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FileParking.Models;
using System.Web.Script.Serialization;

public class mplanhandler : IHttpHandler
{
    private Member CurrentMember = null;
    private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();
    public HttpContext Context { get; set; }

    /// <summary>
    /// Action to be taken
    /// </summary>
    public string Action
    {
        get
        {
            if (Context.Request.QueryString["a"] != null)
            {
                return Context.Request.QueryString["a"];
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Authorization Token for the Request
    /// </summary>
    public bool Token
    {
        get
        {
            if (HttpContext.Current.Request.QueryString["token"] != null)
            {
                CurrentMember = MemberManager.GetUserByToken(new Guid(HttpContext.Current.Request.QueryString["token"].Trim()));
                if (CurrentMember != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public void ProcessRequest(HttpContext context)
    {
        Context = context;
        context.Response.ContentType = "application/json";
        context.Response.AddHeader("Pragma", "no-cache");
        context.Response.AddHeader("Cache-Control", "private, no-cache");


        switch (Action)
        {
            case "plans":
                context.Response.Write(GetPlanList());
                break;
            case "active":
                GetActivePlan();
                break;
            case "history":
                GetHistory();
                break;
            default:
                context.Response.Write("");
                break;
        }
    }

    public void GetHistory()
    {
        if (!Token)
        {
            Context.Response.StatusCode = 401;
            Context.Response.TrySkipIisCustomErrors = true;
            Context.Response.End();
        }
        try
        {
            Context.Response.Write(_javaScriptSerializer.Serialize(new { success = true, list = PlanManager.GetUserPurchaseHistory(CurrentMember.Id) }));
        }
        catch
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            Context.Response.Write(_javaScriptSerializer.Serialize(new { success = false, message = "Invalid Request" }));
        }

    }

    public void GetActivePlan()
    {
        if (!Token)
        {
            Context.Response.StatusCode = 401;
            Context.Response.TrySkipIisCustomErrors = true;
            Context.Response.End();
        }
        try
        {
            MemberPlan obj = PlanManager.GetUserActivePlan(CurrentMember.Id);
            Context.Response.Write(_javaScriptSerializer.Serialize(new { success = true,
                plan = new {
                    Name = obj.Name,
                    DateCreated = obj.DateCreated.ToString(),
                    Amount = obj.Amount.ToString(),
                    Term = obj.Term,
                    Limit = obj.Limit,
                    FileSize = string.Format("{0} GB", obj.FileSize)
                } }));
        }
        catch
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            Context.Response.Write(_javaScriptSerializer.Serialize(new { success = false, message = "Invalid Request" }));
        }

    }

    public string GetPlanList()
    {
        try
        {
            return _javaScriptSerializer.Serialize(new { success = true, plans = PlanManager.GetActivePlans() });
        }
        catch
        {
            Context.Response.StatusCode = 500;
            Context.Response.TrySkipIisCustomErrors = true;
            return _javaScriptSerializer.Serialize(new { success = false, message = "Invalid Request" });
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}