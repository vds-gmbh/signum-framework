﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Extensions.Dashboard.Views
{
    using System;
    using System.Collections.Generic;
    
    #line 1 "..\..\Dashboard\Views\PanelPartView.cshtml"
    using System.Configuration;
    
    #line default
    #line hidden
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Signum.Entities;
    
    #line 2 "..\..\Dashboard\Views\PanelPartView.cshtml"
    using Signum.Entities.Dashboard;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 3 "..\..\Dashboard\Views\PanelPartView.cshtml"
    using Signum.Web.Dashboard;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Dashboard/Views/PanelPartView.cshtml")]
    public partial class PanelPartView : System.Web.Mvc.WebViewPage<PanelPartDN>
    {
        public PanelPartView()
        {
        }
        public override void Execute()
        {
            
            #line 6 "..\..\Dashboard\Views\PanelPartView.cshtml"
   
    string prefix = "r{0}c{1}".Formato(Model.Row, Model.StartColumn);
    DashboardClient.PartViews config = DashboardClient.PanelPartViews[Model.Content.GetType()];
    var link = config.TitleLink == null ? null : config.TitleLink(Model.Content); 

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteAttribute("class", Tuple.Create(" class=\"", 378), Tuple.Create("\"", 431)
, Tuple.Create(Tuple.Create("", 386), Tuple.Create("panel", 386), true)
, Tuple.Create(Tuple.Create(" ", 391), Tuple.Create("panel-", 392), true)
            
            #line 11 "..\..\Dashboard\Views\PanelPartView.cshtml"
, Tuple.Create(Tuple.Create("", 398), Tuple.Create<System.Object, System.Int32>(Model.Style.ToString().ToLower()
            
            #line default
            #line hidden
, 398), false)
);

WriteLiteral(">\r\n    <div");

WriteLiteral(" class=\"panel-heading\"");

WriteLiteral(">\r\n");

            
            #line 13 "..\..\Dashboard\Views\PanelPartView.cshtml"
        
            
            #line default
            #line hidden
            
            #line 13 "..\..\Dashboard\Views\PanelPartView.cshtml"
         if (link == null)
        {
            
            
            #line default
            #line hidden
            
            #line 15 "..\..\Dashboard\Views\PanelPartView.cshtml"
       Write(Model.ToString());

            
            #line default
            #line hidden
            
            #line 15 "..\..\Dashboard\Views\PanelPartView.cshtml"
                             
        }
        else
        {

            
            #line default
            #line hidden
WriteLiteral("            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 588), Tuple.Create("\"", 600)
            
            #line 19 "..\..\Dashboard\Views\PanelPartView.cshtml"
, Tuple.Create(Tuple.Create("", 595), Tuple.Create<System.Object, System.Int32>(link
            
            #line default
            #line hidden
, 595), false)
);

WriteLiteral(">");

            
            #line 19 "..\..\Dashboard\Views\PanelPartView.cshtml"
                       Write(Model.ToString());

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n");

            
            #line 20 "..\..\Dashboard\Views\PanelPartView.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 21 "..\..\Dashboard\Views\PanelPartView.cshtml"
         if (config.HasFullScreenLink)
        {

            
            #line default
            #line hidden
WriteLiteral("            <a");

WriteAttribute("id", Tuple.Create(" id=\"", 701), Tuple.Create("\"", 759)
            
            #line 23 "..\..\Dashboard\Views\PanelPartView.cshtml"
, Tuple.Create(Tuple.Create("", 706), Tuple.Create<System.Object, System.Int32>(TypeContextUtilities.Compose(prefix, "sfFullScreen")
            
            #line default
            #line hidden
, 706), false)
);

WriteLiteral(" class=\"sf-ftbl-header-fullscreen\"");

WriteLiteral(" href=\"#\"");

WriteLiteral(">\r\n                <span");

WriteLiteral(" class=\"glyphicon glyphicon-new-window\"");

WriteLiteral("></span>\r\n            </a>\r\n");

            
            #line 26 "..\..\Dashboard\Views\PanelPartView.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </div>\r\n\r\n    <div");

WriteLiteral(" class=\"panel-body\"");

WriteLiteral(">\r\n");

            
            #line 30 "..\..\Dashboard\Views\PanelPartView.cshtml"
        
            
            #line default
            #line hidden
            
            #line 30 "..\..\Dashboard\Views\PanelPartView.cshtml"
           Html.RenderPartial(config.FrontEndView, TypeContextUtilities.UntypedNew(Model.Content, prefix)); 
            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n\r\n</div>\r\n");

        }
    }
}
#pragma warning restore 1591
