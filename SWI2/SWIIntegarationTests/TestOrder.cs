using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnit.Project.Attributes;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("XUnit.Project.Orderers.TestPriority", "XUnit.Project")]
    public class TestOrder : BasicTests
    {
        public TestOrder(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        [Fact, TestPriority(0, TestType.Get)]
        public void GMethod()
        {
            System.Threading.Thread.Sleep(1000);
        }
        [Fact, TestPriority(0, TestType.Insert)]
        public void AMethod()
        {
            System.Threading.Thread.Sleep(1000);
        }
        [Fact, TestPriority(0, TestType.Edit)]
        public void EMethod()
        {
            System.Threading.Thread.Sleep(1000);
        }
        [Fact, TestPriority(0, TestType.Remove)]
        public void RMethod()
        {
            System.Threading.Thread.Sleep(1000);
        }
    }
}
