﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Extensions.ControlPanel.Views.Admin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    
    #line 3 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
    using System.Reflection;
    
    #line default
    #line hidden
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
    
    #line 1 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
    using Signum.Entities.ControlPanel;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 2 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
    using Signum.Web.ControlPanel;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/ControlPanel/Views/Admin/ControlPanelAdmin.cshtml")]
    public partial class ControlPanelAdmin : System.Web.Mvc.WebViewPage<dynamic>
    {
        public ControlPanelAdmin()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 5 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
Write(Html.ScriptsJs("~/ControlPanel/Scripts/SF_FlowTable.js"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 6 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
Write(Html.ScriptCss("~/ControlPanel/Content/SF_FlowTable.css",
                "~/ControlPanel/Content/SF_ControlPanel.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n<div>\r\n");

            
            #line 10 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
    
            
            #line default
            #line hidden
            
            #line 10 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
     using (var tc = Html.TypeContext<ControlPanelDN>())
    {
        
            
            #line default
            #line hidden
            
            #line 12 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
   Write(Html.EntityLine(tc, cp => cp.Related, el => el.Create = false));

            
            #line default
            #line hidden
            
            #line 12 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
                                                                       
        
            
            #line default
            #line hidden
            
            #line 13 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
   Write(Html.ValueLine(tc, cp => cp.DisplayName));

            
            #line default
            #line hidden
            
            #line 13 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
                                                 
        
            
            #line default
            #line hidden
            
            #line 14 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
   Write(Html.ValueLine(tc, cp => cp.HomePagePriority));

            
            #line default
            #line hidden
            
            #line 14 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
                                                      
        
            
            #line default
            #line hidden
            
            #line 15 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
   Write(Html.ValueLine(tc, cp => cp.NumberOfColumns));

            
            #line default
            #line hidden
            
            #line 15 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
                                                     
        
        
            
            #line default
            #line hidden
            
            #line 17 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
   Write(Html.EntityLine(tc, cp => cp.EntityType, el => { el.AutocompleteUrl = Url.Action("TypeAutocomplete", "Signum"); }));

            
            #line default
            #line hidden
            
            #line 17 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
                                                                                                                           

        Html.RenderPartial(ControlPanelClient.AdminViewPrefix.Formato("PanelParts"), tc.Value);

            
            #line default
            #line hidden
WriteLiteral("        <div");

WriteLiteral(" class=\"clearall\"");

WriteLiteral("></div>   \r\n");

            
            #line 21 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n\r\n<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n    $(function () {\r\n        require([\"");

            
            #line 26 "..\..\ControlPanel\Views\Admin\ControlPanelAdmin.cshtml"
             Write(ControlPanelClient.FlowTableModule);

            
            #line default
            #line hidden
WriteLiteral("\"], function (FlowTable) {\r\n            FlowTable.init($(\"#sfCpAdminContainer\"));" +
"\r\n        }); \r\n    });\r\n</script>");

        }
    }
}
#pragma warning restore 1591
