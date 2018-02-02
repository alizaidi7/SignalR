
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Web.Mvc;
using SignalR.Hubs;
using Microsoft.AspNet.SignalR;

namespace SignalR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DBChange()
        {
            //initiate connetion
            //get connection paramters from web.config
            OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["DBString"]].ConnectionString);
            OracleCommand cmd = new OracleCommand();

            //create the slect parameters
            cmd.CommandText = "select ROWID, QUEUE_ARN, QUEUE_ID, QUEUE_NUMBER, ACTIVITY_DATE from KU_CUSTOM.ugapp_queue";


            cmd.Connection = con;
            con.Open();

            //initiate 
            OracleDependency dependency = new OracleDependency(cmd);

            dependency.QueryBasedNotification = true;
            cmd.Notification.IsNotifiedOnce = false;

            //on change call the action method or function.. our case alert user
            dependency.OnChange += new OnChangeEventHandler(AlertUser);
            //register the current state
            cmd.ExecuteReader();
             return RedirectToAction("Index", "home");
        }
        static void AlertUser(Object sender, OracleNotificationEventArgs args)
        {
            DataTable dt = args.Details;
            string msg = "";
            msg = "The following database objects were changed: ";
            foreach (string resource in args.ResourceNames)
                msg += resource;

            msg += "\n\n Details: ";

            for (int rows = 1; rows < dt.Rows.Count; rows++)
            {
                msg += "Resource name: " + dt.Rows[rows].ItemArray[0];

                string type = Enum.GetName(typeof(OracleNotificationInfo), dt.Rows[rows].ItemArray[1]);
                msg += "\n\nChange type: " + type;
                msg += " ";
            }
            Notify("Oracle", msg);
        }
        static void Notify(string name, string msg)
        {
            var statsContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            statsContext.Clients.All.addNewMessageToPage(name, msg);
        }

    }
}