using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Aldelo
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper help = new Helper();
            //help.GetOrders();
           // help.GetCustomerEmails();
            var deliverId = Guid.NewGuid().ToString();
            var doorDashToken = help.GetJWTToken();
            help.MakeNewDelivery(deliverId,doorDashToken);
            help.RefundOrder(deliverId, doorDashToken);
        }
    }
}