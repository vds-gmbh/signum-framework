﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using Signum.Utilities;
    using Signum.Entities;
    using Signum.Web;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Caching;
    using System.Web.DynamicData;
    using System.Web.SessionState;
    using System.Web.Profile;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.UI.HtmlControls;
    using System.Xml.Linq;
    using Signum.Engine;
    using Signum.Entities.Mailing;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MvcRazorClassGenerator", "1.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Mailing/Views/EmailMessage.cshtml")]
    public class _Page_Mailing_Views_EmailMessage_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {


        public _Page_Mailing_Views_EmailMessage_cshtml()
        {
        }
        protected System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
        public override void Execute()
        {



 using (var e = Html.TypeContext<EmailMessageDN>())
{
    
Write(Html.EntityLine(e, f => f.Recipient));

                                         
    
Write(Html.EntityLine(e, f => f.Template, f => f.ReadOnly = true));

                                                                
    
Write(Html.ValueLine(e, f => f.Sent, f => f.ReadOnly = true));

                                                           
    
Write(Html.ValueLine(e, f => f.Received, f => f.ReadOnly = true));

                                                               
    
Write(Html.EntityLine(e, f => f.Exception, f => f.ReadOnly = true));

                                                                 
    
Write(Html.ValueLine(e, f => f.State, f => f.ReadOnly = true));

                                                            
    
Write(Html.EntityLine(e, f => f.Package, f => f.ReadOnly = true));

                                                               
    
Write(Html.ValueLine(e, f => f.Subject));

                                      

WriteLiteral("    <h3>\r\n        Message:</h3>\r\n");



WriteLiteral("    <div>\r\n        ");


   Write(Html.Raw(e.Value.Body));

WriteLiteral("\r\n    </div>\r\n");


}


        }
    }
}
