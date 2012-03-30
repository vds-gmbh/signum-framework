﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Signum.Engine.Linq
{
    internal class AliasProjectionReplacer : DbExpressionVisitor
    {
        ProjectionExpression root;

        public static Expression Replace(Expression proj)
        {
            AliasProjectionReplacer apr = new AliasProjectionReplacer()
            {
                root = proj as ProjectionExpression,
            };
            return apr.Visit(proj);
        }       

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            if (proj != root)
                return (ProjectionExpression)AliasReplacer.Replace(base.VisitProjection(proj));
            else
                return (ProjectionExpression)base.VisitProjection(proj);
        }
    }

}
