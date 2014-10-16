using NUnit.Framework;
using System;
using Microsoft.Boogie;
using Symbooglix;

namespace ConstantFoldingTests
{
    [TestFixture()]
    public class FoldAnd : TestBase
    {
        [Test()]
        public void Simple()
        {
            // Try all 4 combinations of input to boolean and.
            for (int arg0 = 0; arg0 <= 1; ++arg0)
            {
                for (int arg1 = 0; arg1 <=1; ++arg1)
                {
                    bool expected = (arg0 == 1) && (arg1 == 1);
                    Expr arg0AsExpr = getBool(arg0);
                    Expr arg1AsExpr = getBool(arg1);

                    Expr andExpr = Expr.And(arg0AsExpr, arg1AsExpr);
                    var TC = new TypecheckingContext(this);
                    andExpr.Typecheck(TC);

                    var CFT = new ConstantFoldingTraverser();
                    Expr result = CFT.Traverse(andExpr);
                    result.Typecheck(TC);
                    Assert.IsTrue(result is LiteralExpr);

                    if (expected)
                        Assert.IsTrue(( result as LiteralExpr ).IsTrue);
                    else
                        Assert.IsTrue(( result as LiteralExpr ).IsFalse);
                }
            }
        }

        public LiteralExpr getBool(int v)
        {
            if (v == 0)
                return Expr.False;
            else
                return Expr.True;
        }

        [Test()]
        public void OneSideTrue()
        {
            var TrueExpr = builder.ConstantBool(true);

            var v = new IdentifierExpr(Token.NoToken, new GlobalVariable(Token.NoToken, new TypedIdent(Token.NoToken, "v", Microsoft.Boogie.Type.Bool)));
            var AndExpr = builder.And(TrueExpr, v);

            var CFT = new ConstantFoldingTraverser();
            var result = CFT.Traverse(AndExpr);
            Assert.AreSame(v, result);

            var AndExpr2 = builder.And(v, TrueExpr);
            result = CFT.Traverse(AndExpr2);
            Assert.AreSame(v, result);
        }
    }
}

