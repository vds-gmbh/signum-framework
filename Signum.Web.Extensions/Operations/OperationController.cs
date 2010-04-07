﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Signum.Services;
using Signum.Utilities;
using Signum.Entities;
using Signum.Web;
using Signum.Engine;
using Signum.Engine.Operations;
using Signum.Entities.Operations;
using Signum.Engine.Basics;
using Signum.Web.Extensions.Properties;

namespace Signum.Web.Operations
{
    [HandleException, AuthenticationRequired]
    public class OperationController : Controller
    {
        public ActionResult OperationExecute(string sfRuntimeType, int? sfId, string sfOperationFullKey, bool isLite, string prefix, string sfOldPrefix, string sfOnOk, string sfOnCancel)
        {
            Type type = Navigator.ResolveType(sfRuntimeType);

            IdentifiableEntity entity = null;
            if (isLite)
            {
                if (sfId.HasValue)
                {
                    Lite lite = Lite.Create(type, sfId.Value);
                    entity = OperationLogic.ServiceExecuteLite(lite, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey));
                }
                else
                    throw new ArgumentException(Resources.CouldNotCreateLiteWithoutAnIdToCallOperation0.Formato(sfOperationFullKey));
            }
            else
            {
                MappingContext context = this.UntypedExtractEntity(sfOldPrefix).UntypedApplyChanges(this.ControllerContext, sfOldPrefix, true).UntypedValidateGlobal();
                entity = (IdentifiableEntity)context.UntypedValue;

                if (context.Errors.Any())
                {
                    this.ModelState.FromContext(context);
                    return Content("{\"ModelState\":" + this.ModelState.ToJsonData() + "}");
                }

                entity = OperationLogic.ServiceExecute(entity, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey));

                if (this.IsReactive())
                    Session[this.TabID()] = entity;
            }

            if (prefix.HasText() && Request.IsAjaxRequest())
            {
                ViewData[ViewDataKeys.WriteSFInfo] = true;
                return Navigator.PopupView(this, entity, prefix);
            }
            else //NormalWindow
                return Navigator.View(this, entity);
        }

        public ActionResult DeleteExecute(string sfRuntimeType, int? sfId, string sfOperationFullKey, string prefix, string sfOnOk, string sfOnCancel)
        {
            Type type = Navigator.ResolveType(sfRuntimeType);

            if (sfId.HasValue)
            {
                Lite lite = Lite.Create(type, sfId.Value);
                OperationLogic.ServiceDelete(lite, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey), null);
            }
            else
                throw new ArgumentException(Resources.CouldNotCreateLiteWithoutAnIdToCallOperation0.Formato(sfOperationFullKey));

            return Content("");
        }

        public ActionResult ConstructFromExecute(string sfRuntimeType, int? sfId, string sfOperationFullKey, bool isLite, string prefix, string sfOldPrefix, string sfOnOk, string sfOnCancel)
        {
            Type type = Navigator.ResolveType(sfRuntimeType);

            IdentifiableEntity entity = null;
            if (isLite)
            {
                if (sfId.HasValue)
                {
                    Lite lite = Lite.Create(type, sfId.Value);
                    entity = (IdentifiableEntity)OperationLogic.ServiceConstructFromLite(lite, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey));
                }
                else
                    throw new ArgumentException(Resources.CouldNotCreateLiteWithoutAnIdToCallOperation0.Formato(sfOperationFullKey));
            }
            else
            {
                MappingContext context = this.UntypedExtractEntity(sfOldPrefix).UntypedApplyChanges(this.ControllerContext, sfOldPrefix, true).UntypedValidateGlobal();
                entity = (IdentifiableEntity)context.UntypedValue;

                if (context.Errors.Any())
                {
                   this.ModelState.FromContext(context);
                    return Content("{\"ModelState\":" + this.ModelState.ToJsonData() + "}");
                }

                entity = (IdentifiableEntity)OperationLogic.ServiceConstructFrom(entity, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey));
            }

            if (prefix.HasText() && Request.IsAjaxRequest())
            {
                ViewData[ViewDataKeys.WriteSFInfo] = true;
                return Navigator.PopupView(this, entity, prefix);
            }
            else //NormalWindow
                return Navigator.View(this, entity);
        }

        public ActionResult ConstructFromManyExecute(string sfRuntimeType, string sfIds, string sfOperationFullKey, string prefix, string sfOnOk, string sfOnCancel)
        {
            Type type = Navigator.ResolveType(sfRuntimeType);
            
            List<Lite> sourceEntities = null;
            if (sfIds.HasText())
            {
                string[] ids = sfIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (ids == null || ids.Length == 0)
                    throw new ArgumentException(Resources.ConstructFromManyOperation0NeedsSourceIdsAsParameter.Formato(sfOperationFullKey));
                sourceEntities = ids.Select(idstr => Lite.Create(type, int.Parse(idstr))).ToList();
            }
            if (sourceEntities == null)
                throw new ArgumentException(Resources.ConstructFromManyOperation0NeedsSourceLazies.Formato(sfOperationFullKey));

            IdentifiableEntity entity = OperationLogic.ServiceConstructFromMany(sourceEntities, type, EnumLogic<OperationDN>.ToEnum(sfOperationFullKey));

            ViewData[ViewDataKeys.WriteSFInfo] = true;
            return Navigator.PopupView(this, entity, prefix);
        }
    }
}
